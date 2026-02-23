#!/usr/bin/env node

import { createInterface } from "node:readline";
import { loadConfig } from "./config/config-manager.js";
import { createBamvoiceApp } from "./bamvoice.js";
import { logger, setLogLevel } from "./util/logger.js";

async function main(): Promise<void> {
  const args = process.argv.slice(2);

  // Parse arguments
  let configPath: string | undefined;
  let verbose = false;

  for (let i = 0; i < args.length; i++) {
    switch (args[i]) {
      case "--config":
      case "-c":
        configPath = args[++i];
        break;
      case "--verbose":
      case "-v":
        verbose = true;
        break;
      case "--help":
      case "-h":
        printHelp();
        process.exit(0);
        break;
      case "--version":
        console.log("bamvoice 0.1.0");
        process.exit(0);
        break;
    }
  }

  if (verbose) {
    setLogLevel("debug");
  }

  console.log("bamvoice - Voice-Interactive Control Layer for Claude Code");
  console.log("-----------------------------------------------------------");

  // Load config
  const config = await loadConfig(configPath);
  logger.debug("Config loaded: %o", config);

  // Create app
  let app;
  try {
    app = await createBamvoiceApp(config);
  } catch (err) {
    console.error(`Fatal: ${(err as Error).message}`);
    process.exit(1);
  }

  const controller = app.controller;

  // Subscribe to events for console output
  controller.events.on("stateChange", ({ from, to }) => {
    logger.debug("[%s] -> [%s]", from, to);
  });

  controller.events.on("message", (msg) => {
    console.log(`> ${msg}`);
  });

  controller.events.on("error", (err) => {
    console.error(`! Error: ${err.message}`);
  });

  // Set up text input fallback
  const rl = createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  console.log("\nReady. Type a prompt and press Enter, or press Enter to start voice listening.");
  console.log("Type 'quit' or 'exit' to stop.\n");

  const prompt = () => {
    rl.question("bamvoice> ", async (input) => {
      const trimmed = input.trim();

      if (trimmed === "quit" || trimmed === "exit") {
        await shutdown();
        return;
      }

      if (trimmed === "") {
        // Start voice listening
        await controller.startListening();
      } else if (controller.state === "confirming") {
        // In confirmation mode, interpret as intent
        await controller.handleTextInput(trimmed);
      } else {
        // Text input as prompt
        await controller.handleTextInput(trimmed);
      }

      // Wait for the controller to return to idle before prompting again
      if (controller.state === "idle" || controller.state === "error") {
        prompt();
      } else {
        const unsub = controller.events.on("stateChange", ({ to }) => {
          if (to === "idle" || to === "error") {
            unsub();
            prompt();
          }
        });
      }
    });
  };

  // Graceful shutdown
  const shutdown = async () => {
    console.log("\nShutting down...");
    rl.close();
    await app!.dispose();
    process.exit(0);
  };

  process.on("SIGINT", shutdown);
  process.on("SIGTERM", shutdown);

  prompt();
}

function printHelp(): void {
  console.log(`
bamvoice - Voice-Interactive Control Layer for Claude Code

Usage: bamvoice [options]

Options:
  -c, --config <path>   Path to config file (default: ./bamvoice.config.json)
  -v, --verbose         Enable debug logging
  -h, --help            Show this help
  --version             Show version

Interaction:
  - Type a prompt and press Enter to plan via Claude
  - Press Enter with no text to start voice listening
  - After a plan is described, say or type:
    'proceed'  - execute the plan
    'repeat'   - hear the plan again
    'rephrase' - re-plan with modifications
    'cancel'   - abort the plan
  - Type 'quit' or 'exit' to stop

Config file (bamvoice.config.json):
  See documentation for all available options.
  `.trim());
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
