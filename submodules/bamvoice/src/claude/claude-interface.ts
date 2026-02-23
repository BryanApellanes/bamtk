import { ChildProcess } from "node:child_process";
import { spawnAsync, spawnStreaming } from "../util/process-spawn.js";
import { ClaudeConfig } from "../config/config-schema.js";
import { ActionPlan, isActionPlan } from "../planning/action-plan.js";
import { PLANNING_SYSTEM_PROMPT } from "./planning-prompt.js";
import { buildExecutionPrompt } from "./execution-prompt.js";
import { parseStreamLine, StreamMessage } from "./claude-stream-parser.js";
import { logger } from "../util/logger.js";

export interface PlanResult {
  plan: ActionPlan | null;
  sessionId?: string;
  rawOutput: string;
  error?: string;
}

export interface ExecutionProgress {
  type: "tool_call" | "text" | "result" | "error" | "done";
  tool?: string;
  toolInput?: Record<string, unknown>;
  content?: string;
}

export class ClaudeInterface {
  private readonly config: ClaudeConfig;
  private activeProcess: ChildProcess | null = null;

  constructor(config: ClaudeConfig) {
    this.config = config;
  }

  async checkAvailability(): Promise<boolean> {
    try {
      const result = await spawnAsync(this.config.cliPath, ["--version"]);
      return result.exitCode === 0;
    } catch {
      return false;
    }
  }

  async generatePlan(prompt: string): Promise<PlanResult> {
    logger.info("Generating plan for prompt: %s", prompt.substring(0, 80));

    try {
      const result = await spawnAsync(this.config.cliPath, [
        "--print",
        prompt,
        "--output-format", "json",
        "--model", this.config.planningModel,
        "--max-turns", "1",
        "--system-prompt", PLANNING_SYSTEM_PROMPT,
      ]);

      if (result.exitCode !== 0) {
        return {
          plan: null,
          rawOutput: result.stderr || result.stdout,
          error: `Claude CLI exited with code ${result.exitCode}: ${result.stderr}`,
        };
      }

      const plan = this.extractPlan(result.stdout);
      if (!plan) {
        return {
          plan: null,
          rawOutput: result.stdout,
          error: "Failed to parse action plan from Claude output",
        };
      }

      plan.originalPrompt = prompt;

      return {
        plan,
        rawOutput: result.stdout,
      };
    } catch (err) {
      return {
        plan: null,
        rawOutput: "",
        error: `Failed to run Claude CLI: ${(err as Error).message}`,
      };
    }
  }

  startExecution(
    plan: ActionPlan,
    sessionId: string | undefined,
    onProgress: (progress: ExecutionProgress) => void,
  ): Promise<number | null> {
    const executionPrompt = buildExecutionPrompt(plan.summary);

    const args: string[] = [
      "--print",
      executionPrompt,
      "--output-format", "stream-json",
      "--model", this.config.executionModel,
    ];

    if (sessionId) {
      args.push("--resume", sessionId);
    }

    return new Promise((resolve, reject) => {
      this.activeProcess = spawnStreaming(this.config.cliPath, args);

      let buffer = "";

      this.activeProcess.stdout?.on("data", (chunk: Buffer) => {
        buffer += chunk.toString();
        const lines = buffer.split("\n");
        buffer = lines.pop() ?? "";

        for (const line of lines) {
          const msg = parseStreamLine(line);
          if (msg) {
            this.handleStreamMessage(msg, onProgress);
          }
        }
      });

      this.activeProcess.stderr?.on("data", (chunk: Buffer) => {
        logger.error("Claude stderr: %s", chunk.toString().trim());
      });

      this.activeProcess.on("close", (code) => {
        this.activeProcess = null;
        if (buffer.trim()) {
          const msg = parseStreamLine(buffer);
          if (msg) this.handleStreamMessage(msg, onProgress);
        }
        onProgress({ type: "done" });
        resolve(code);
      });

      this.activeProcess.on("error", (err) => {
        this.activeProcess = null;
        reject(err);
      });
    });
  }

  cancelExecution(): void {
    if (this.activeProcess) {
      this.activeProcess.kill();
      this.activeProcess = null;
      logger.info("Claude execution cancelled");
    }
  }

  private handleStreamMessage(
    msg: StreamMessage,
    onProgress: (progress: ExecutionProgress) => void,
  ): void {
    if (msg.type === "tool_use" || msg.subtype === "tool_use") {
      onProgress({
        type: "tool_call",
        tool: msg.tool,
        toolInput: msg.tool_input,
      });
    } else if (msg.type === "tool_result" || msg.subtype === "tool_result") {
      onProgress({
        type: "result",
        content: msg.result ?? msg.content,
      });
    } else if (msg.type === "text" || msg.content) {
      onProgress({
        type: "text",
        content: msg.content,
      });
    }
  }

  private extractPlan(output: string): ActionPlan | null {
    // Try parsing the entire output as JSON first
    try {
      const parsed = JSON.parse(output);
      // Claude --print --output-format json wraps in a result structure
      if (parsed.result) {
        return this.parsePlanFromText(parsed.result);
      }
      if (isActionPlan(parsed)) return parsed;
    } catch {
      // Not pure JSON, try extracting from text
    }

    return this.parsePlanFromText(output);
  }

  private parsePlanFromText(text: string): ActionPlan | null {
    // Try to find JSON in the text
    const jsonMatch = text.match(/\{[\s\S]*"planId"[\s\S]*"actions"[\s\S]*\}/);
    if (jsonMatch) {
      try {
        const parsed = JSON.parse(jsonMatch[0]);
        if (isActionPlan(parsed)) return parsed;
      } catch {
        // Invalid JSON
      }
    }

    // Try line-by-line for NDJSON
    const lines = text.split("\n");
    for (const line of lines) {
      try {
        const parsed = JSON.parse(line.trim());
        if (isActionPlan(parsed)) return parsed;
        if (parsed.result) {
          const inner = this.parsePlanFromText(parsed.result);
          if (inner) return inner;
        }
      } catch {
        continue;
      }
    }

    return null;
  }
}
