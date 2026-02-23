import { StateMachine } from "./state-machine.js";
import { EventBus } from "./event-bus.js";
import { STTProvider, STTResult } from "../stt/stt-provider.js";
import { TTSProvider } from "../tts/tts-provider.js";
import { ClaudeInterface, ExecutionProgress } from "../claude/claude-interface.js";
import { IntentDetector } from "../intent/intent-detector.js";
import { HookBridge } from "../hooks/hook-bridge.js";
import { ActionPlan } from "../planning/action-plan.js";
import { describeActionPlan } from "../planning/permission-describer.js";
import { BamvoiceConfig } from "../config/config-schema.js";
import { logger } from "../util/logger.js";

export type VoiceState =
  | "idle"
  | "listening"
  | "planning"
  | "describing"
  | "confirming"
  | "executing"
  | "error";

export type VoiceEvent =
  | "START_LISTENING"
  | "SPEECH_RECEIVED"
  | "TIMEOUT"
  | "CANCEL"
  | "PLAN_READY"
  | "ERROR"
  | "DESCRIPTION_DONE"
  | "INTENT_EXECUTE"
  | "INTENT_REPEAT"
  | "INTENT_REPHRASE"
  | "INTENT_CANCEL"
  | "EXECUTION_DONE";

export interface VoiceControllerEvents {
  stateChange: { from: VoiceState; to: VoiceState; event: VoiceEvent };
  speech: { text: string; confidence: number };
  plan: ActionPlan;
  description: string;
  progress: ExecutionProgress;
  error: Error;
  message: string;
}

export class VoiceController {
  private readonly sm: StateMachine<VoiceState, VoiceEvent>;
  readonly events = new EventBus<VoiceControllerEvents>();

  private readonly stt: STTProvider;
  private readonly tts: TTSProvider;
  private readonly claude: ClaudeInterface;
  private readonly intentDetector: IntentDetector;
  private readonly hookBridge: HookBridge;
  private readonly config: BamvoiceConfig;

  private currentPlan: ActionPlan | null = null;
  private currentDescription = "";
  private currentPrompt = "";
  private sessionId?: string;
  private listeningTimeout: ReturnType<typeof setTimeout> | null = null;
  private confirmationTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor(deps: {
    stt: STTProvider;
    tts: TTSProvider;
    claude: ClaudeInterface;
    intentDetector: IntentDetector;
    hookBridge: HookBridge;
    config: BamvoiceConfig;
  }) {
    this.stt = deps.stt;
    this.tts = deps.tts;
    this.claude = deps.claude;
    this.intentDetector = deps.intentDetector;
    this.hookBridge = deps.hookBridge;
    this.config = deps.config;

    this.sm = new StateMachine<VoiceState, VoiceEvent>({
      initial: "idle",
      transitions: [
        { from: "idle", event: "START_LISTENING", to: "listening" },
        { from: "listening", event: "SPEECH_RECEIVED", to: "planning" },
        { from: "listening", event: "TIMEOUT", to: "idle" },
        { from: "listening", event: "CANCEL", to: "idle" },
        { from: "planning", event: "PLAN_READY", to: "describing" },
        { from: "planning", event: "ERROR", to: "error" },
        { from: "planning", event: "CANCEL", to: "idle" },
        { from: "describing", event: "DESCRIPTION_DONE", to: "confirming" },
        { from: "confirming", event: "INTENT_EXECUTE", to: "executing" },
        { from: "confirming", event: "INTENT_REPEAT", to: "describing" },
        { from: "confirming", event: "INTENT_REPHRASE", to: "planning" },
        { from: "confirming", event: "INTENT_CANCEL", to: "idle" },
        { from: "confirming", event: "TIMEOUT", to: "idle" },
        { from: "executing", event: "EXECUTION_DONE", to: "idle" },
        { from: "executing", event: "ERROR", to: "error" },
        { from: "error", event: "START_LISTENING", to: "listening" },
        { from: "error", event: "CANCEL", to: "idle" },
      ],
    });

    this.sm.onChange((from, to, event) => {
      this.events.emit("stateChange", { from, to, event });
      logger.debug("State: %s -> %s (event: %s)", from, to, event);
    });
  }

  get state(): VoiceState {
    return this.sm.state;
  }

  async startListening(): Promise<void> {
    if (!this.sm.canSend("START_LISTENING")) {
      logger.warn("Cannot start listening from state: %s", this.sm.state);
      return;
    }

    await this.sm.send("START_LISTENING");
    await this.tts.speak("Listening.");
    this.events.emit("message", "Listening...");

    this.listeningTimeout = setTimeout(async () => {
      if (this.sm.state === "listening") {
        await this.stt.stopListening();
        await this.sm.send("TIMEOUT");
        await this.tts.speak("Listening timed out.");
        this.events.emit("message", "Listening timed out.");
      }
    }, this.config.control.listeningTimeoutSeconds * 1000);

    await this.stt.startListening((result: STTResult) => {
      if (result.isFinal && result.text.trim()) {
        this.handleSpeechResult(result);
      }
    });
  }

  async handleTextInput(text: string): Promise<void> {
    if (this.sm.state === "idle") {
      await this.sm.send("START_LISTENING");
      await this.handleSpeechResult({ text, confidence: 1.0, isFinal: true });
    } else if (this.sm.state === "confirming") {
      await this.handleConfirmationInput(text);
    }
  }

  async cancel(): Promise<void> {
    this.clearTimeouts();
    await this.stt.stopListening();
    await this.tts.cancel();
    this.claude.cancelExecution();

    if (this.sm.canSend("CANCEL")) {
      await this.sm.send("CANCEL");
      this.events.emit("message", "Cancelled.");
    }
  }

  async dispose(): Promise<void> {
    this.clearTimeouts();
    await this.stt.dispose();
    await this.tts.dispose();
    await this.hookBridge.stop();
    this.claude.cancelExecution();
    this.events.removeAllListeners();
  }

  private async handleSpeechResult(result: STTResult): Promise<void> {
    this.clearTimeouts();
    await this.stt.stopListening();

    this.events.emit("speech", { text: result.text, confidence: result.confidence });

    if (this.sm.state === "listening") {
      this.currentPrompt = result.text;
      await this.sm.send("SPEECH_RECEIVED");
      await this.generatePlan(result.text);
    }
  }

  private async generatePlan(prompt: string): Promise<void> {
    this.events.emit("message", "Planning...");
    await this.tts.speak("Planning. One moment.");

    const planResult = await this.claude.generatePlan(prompt);

    if (planResult.error || !planResult.plan) {
      const errorMsg = planResult.error ?? "Unknown planning error";
      logger.error("Planning failed: %s", errorMsg);
      this.events.emit("error", new Error(errorMsg));
      await this.tts.speak("Planning failed. " + errorMsg);
      await this.sm.send("ERROR");
      return;
    }

    this.currentPlan = planResult.plan;
    this.sessionId = planResult.sessionId;
    this.events.emit("plan", planResult.plan);

    await this.sm.send("PLAN_READY");
    await this.describePlan();
  }

  private async describePlan(): Promise<void> {
    if (!this.currentPlan) return;

    this.currentDescription = describeActionPlan(this.currentPlan);
    this.events.emit("description", this.currentDescription);
    this.events.emit("message", this.currentDescription);

    await this.tts.speak(this.currentDescription);
    await this.sm.send("DESCRIPTION_DONE");
    await this.startConfirmationListening();
  }

  private async startConfirmationListening(): Promise<void> {
    const keywords = this.intentDetector.getAllKeywords();
    this.stt.setGrammar(keywords);

    this.confirmationTimeout = setTimeout(async () => {
      if (this.sm.state === "confirming") {
        await this.stt.stopListening();
        await this.sm.send("TIMEOUT");
        await this.tts.speak("Confirmation timed out. Plan cancelled.");
        this.events.emit("message", "Confirmation timed out.");
      }
    }, this.config.control.confirmationTimeoutSeconds * 1000);

    await this.stt.startListening((result: STTResult) => {
      if (result.isFinal && result.text.trim()) {
        this.handleConfirmationInput(result.text);
      }
    });
  }

  private async handleConfirmationInput(text: string): Promise<void> {
    this.clearTimeouts();
    await this.stt.stopListening();

    const intentResult = this.intentDetector.detect(text);
    logger.info("Intent detected: %s (confidence: %f)", intentResult.intent, intentResult.confidence);

    switch (intentResult.intent) {
      case "execute":
        await this.sm.send("INTENT_EXECUTE");
        await this.executePlan();
        break;

      case "repeat":
        await this.sm.send("INTENT_REPEAT");
        await this.describePlan();
        break;

      case "rephrase":
        await this.sm.send("INTENT_REPHRASE");
        await this.tts.speak("Please rephrase your request.");
        this.events.emit("message", "Please rephrase your request.");
        await this.startListening();
        break;

      case "cancel":
        await this.sm.send("INTENT_CANCEL");
        await this.tts.speak("Plan cancelled.");
        this.events.emit("message", "Plan cancelled.");
        break;

      default:
        await this.tts.speak(
          "I didn't understand. Say 'proceed' to execute, 'repeat', 'rephrase', or 'cancel'.",
        );
        await this.startConfirmationListening();
        break;
    }
  }

  private async executePlan(): Promise<void> {
    if (!this.currentPlan) return;

    this.events.emit("message", "Executing...");
    await this.tts.speak("Executing.");

    this.hookBridge.setActivePlan(this.currentPlan);

    try {
      await this.claude.startExecution(
        this.currentPlan,
        this.sessionId,
        async (progress: ExecutionProgress) => {
          this.events.emit("progress", progress);

          if (progress.type === "tool_call" && progress.tool) {
            const step = this.findPlanStep(progress.tool);
            if (step) {
              const msg = `Step ${step.step}: ${step.description}.`;
              this.events.emit("message", msg);
              await this.tts.speak(msg);
            }
          }

          if (progress.type === "done") {
            this.events.emit("message", "All steps completed.");
          }
        },
      );

      await this.tts.speak("All steps completed successfully.");
      this.events.emit("message", "All steps completed successfully.");
    } catch (err) {
      const errorMsg = (err as Error).message;
      logger.error("Execution failed: %s", errorMsg);
      this.events.emit("error", new Error(errorMsg));
      await this.tts.speak("Execution failed. " + errorMsg);
    }

    this.hookBridge.clearPlan();
    this.currentPlan = null;
    this.sessionId = undefined;
    await this.sm.send("EXECUTION_DONE");
  }

  private findPlanStep(tool: string) {
    return this.currentPlan?.actions.find((a) => a.tool === tool);
  }

  private clearTimeouts(): void {
    if (this.listeningTimeout) {
      clearTimeout(this.listeningTimeout);
      this.listeningTimeout = null;
    }
    if (this.confirmationTimeout) {
      clearTimeout(this.confirmationTimeout);
      this.confirmationTimeout = null;
    }
  }
}
