import { BamvoiceConfig } from "./config/config-schema.js";
import { createSttProvider } from "./stt/stt-factory.js";
import { createTtsProvider } from "./tts/tts-factory.js";
import { ClaudeInterface } from "./claude/claude-interface.js";
import { IntentDetector } from "./intent/intent-detector.js";
import { HookBridge } from "./hooks/hook-bridge.js";
import { VoiceController } from "./core/voice-controller.js";
import { logger } from "./util/logger.js";

export interface BamvoiceApp {
  controller: VoiceController;
  dispose: () => Promise<void>;
}

export async function createBamvoiceApp(config: BamvoiceConfig): Promise<BamvoiceApp> {
  // Check Claude CLI availability
  const claude = new ClaudeInterface(config.claude);
  const claudeAvailable = await claude.checkAvailability();
  if (!claudeAvailable) {
    throw new Error(
      "Claude CLI not found. Install it from https://docs.anthropic.com/en/docs/claude-code and ensure it's on your PATH.",
    );
  }
  logger.info("Claude CLI available");

  // Initialize TTS
  const tts = createTtsProvider(config.tts);
  const ttsAvailable = await tts.initialize({
    rate: config.tts.rate,
    volume: config.tts.volume,
  });

  if (!ttsAvailable) {
    logger.warn("TTS not available — text-only mode");
  }

  // Initialize STT (with fallback chain)
  let stt;
  let sttAvailable = false;
  try {
    stt = await createSttProvider(config.stt);
    sttAvailable = stt.isAvailable;
  } catch {
    logger.warn("No STT provider available — keyboard input only");
    // Create a stub provider for text-only mode
    stt = {
      name: "none",
      isAvailable: false,
      initialize: async () => false,
      startListening: async () => {},
      stopListening: async () => {},
      setGrammar: () => {},
      dispose: async () => {},
    };
  }

  // Initialize intent detector
  const intentDetector = new IntentDetector(config.control);

  // Initialize hook bridge
  const hookBridge = new HookBridge();
  if (config.hooks.enablePreToolUseHook) {
    await hookBridge.start(config.hooks.ipcPort);
  }

  // Wire up the voice controller
  const controller = new VoiceController({
    stt,
    tts,
    claude,
    intentDetector,
    hookBridge,
    config,
  });

  // Log degradation mode
  if (sttAvailable && ttsAvailable) {
    logger.info("Mode: full voice control");
  } else if (ttsAvailable) {
    logger.info("Mode: TTS-only (keyboard text input)");
  } else {
    logger.info("Mode: text-only (console I/O)");
  }

  const dispose = async () => {
    await controller.dispose();
    logger.info("bamvoice shut down");
  };

  return { controller, dispose };
}
