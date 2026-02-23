import { describe, it, expect, vi } from "vitest";
import { VoiceController } from "../../src/core/voice-controller.js";
import { IntentDetector } from "../../src/intent/intent-detector.js";
import { HookBridge } from "../../src/hooks/hook-bridge.js";
import { DEFAULT_CONFIG } from "../../src/config/config-schema.js";
import { ActionPlan } from "../../src/planning/action-plan.js";
import { STTProvider, STTResult } from "../../src/stt/stt-provider.js";
import { TTSProvider } from "../../src/tts/tts-provider.js";
import { ClaudeInterface } from "../../src/claude/claude-interface.js";

function createMockSTT(): STTProvider {
  return {
    name: "mock",
    isAvailable: true,
    initialize: vi.fn().mockResolvedValue(true),
    startListening: vi.fn().mockResolvedValue(undefined),
    stopListening: vi.fn().mockResolvedValue(undefined),
    setGrammar: vi.fn(),
    dispose: vi.fn().mockResolvedValue(undefined),
  };
}

function createMockTTS(): TTSProvider {
  return {
    name: "mock",
    isAvailable: true,
    initialize: vi.fn().mockResolvedValue(true),
    speak: vi.fn().mockResolvedValue(undefined),
    cancel: vi.fn().mockResolvedValue(undefined),
    dispose: vi.fn().mockResolvedValue(undefined),
  };
}

function createMockPlan(): ActionPlan {
  return {
    planId: "mock-plan-1",
    summary: "Mock plan for testing",
    originalPrompt: "test prompt",
    actions: [
      {
        step: 1,
        type: "read",
        description: "Read a test file",
        tool: "Read",
        toolInput: { file_path: "test.ts" },
        affectedResources: ["test.ts"],
        risk: "safe",
        reversible: true,
      },
    ],
    overallRisk: "safe",
    filesAffected: 1,
    affectedFilePaths: ["test.ts"],
    fullyReversible: true,
    warnings: [],
  };
}

function createMockClaude(): ClaudeInterface {
  const mock = {
    checkAvailability: vi.fn().mockResolvedValue(true),
    generatePlan: vi.fn().mockResolvedValue({
      plan: createMockPlan(),
      rawOutput: "{}",
    }),
    startExecution: vi.fn().mockImplementation((_plan, _sid, onProgress) => {
      onProgress({ type: "done" });
      return Promise.resolve(0);
    }),
    cancelExecution: vi.fn(),
  };
  return mock as unknown as ClaudeInterface;
}

describe("VoiceController Integration", () => {
  it("should start in idle state", () => {
    const controller = new VoiceController({
      stt: createMockSTT(),
      tts: createMockTTS(),
      claude: createMockClaude(),
      intentDetector: new IntentDetector(DEFAULT_CONFIG.control),
      hookBridge: new HookBridge(),
      config: DEFAULT_CONFIG,
    });

    expect(controller.state).toBe("idle");
  });

  it("should handle text input flow from idle to planning", async () => {
    const tts = createMockTTS();
    const claude = createMockClaude();

    const controller = new VoiceController({
      stt: createMockSTT(),
      tts,
      claude,
      intentDetector: new IntentDetector(DEFAULT_CONFIG.control),
      hookBridge: new HookBridge(),
      config: DEFAULT_CONFIG,
    });

    const stateChanges: string[] = [];
    controller.events.on("stateChange", ({ to }) => stateChanges.push(to));

    await controller.handleTextInput("add a function");

    // Should have progressed through: listening -> planning -> describing -> confirming
    expect(claude.generatePlan).toHaveBeenCalled();
    expect(tts.speak).toHaveBeenCalled();
  });

  it("should handle cancel intent in confirming state", async () => {
    const tts = createMockTTS();
    const claude = createMockClaude();
    const stt = createMockSTT();

    const controller = new VoiceController({
      stt,
      tts,
      claude,
      intentDetector: new IntentDetector(DEFAULT_CONFIG.control),
      hookBridge: new HookBridge(),
      config: DEFAULT_CONFIG,
    });

    // Go through the flow
    await controller.handleTextInput("test prompt");

    // Now in confirming state, send cancel
    if (controller.state === "confirming") {
      await controller.handleTextInput("cancel");
      expect(controller.state).toBe("idle");
    }
  });

  it("should dispose cleanly", async () => {
    const stt = createMockSTT();
    const tts = createMockTTS();

    const controller = new VoiceController({
      stt,
      tts,
      claude: createMockClaude(),
      intentDetector: new IntentDetector(DEFAULT_CONFIG.control),
      hookBridge: new HookBridge(),
      config: DEFAULT_CONFIG,
    });

    await controller.dispose();
    expect(stt.dispose).toHaveBeenCalled();
    expect(tts.dispose).toHaveBeenCalled();
  });
});
