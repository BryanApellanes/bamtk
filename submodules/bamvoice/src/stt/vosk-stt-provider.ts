import type { Model as VoskModel, Recognizer as VoskRecognizer } from "vosk";
import { STTProvider, STTProviderOptions, STTResult } from "./stt-provider.js";
import { logger } from "../util/logger.js";
import { existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const DEFAULT_MODEL_PATH = resolve(__dirname, "../../models/vosk-model-small-en-us-0.15");

interface VoskModule {
  Model: typeof VoskModel;
  Recognizer: typeof VoskRecognizer;
  setLogLevel(level: number): void;
}

export class VoskSttProvider implements STTProvider {
  readonly name = "vosk";
  private _available = false;
  private vosk: VoskModule | null = null;
  private model: VoskModel | null = null;
  private recognizer: VoskRecognizer | null = null;
  private sampleRate = 16000;
  private grammar: string[] = [];
  private micStream: NodeJS.ReadableStream | null = null;

  get isAvailable(): boolean {
    return this._available;
  }

  async initialize(options: STTProviderOptions): Promise<boolean> {
    this.sampleRate = options.sampleRate ?? 16000;
    const modelPath = options.modelPath ?? DEFAULT_MODEL_PATH;

    try {
      this.vosk = (await import("vosk")) as unknown as VoskModule;
      this.vosk.setLogLevel(-1);
    } catch (err) {
      logger.warn("Vosk module not available: %s", (err as Error).message);
      this._available = false;
      return false;
    }

    if (!existsSync(modelPath)) {
      logger.warn(
        "Vosk model not found at %s. Download a model from https://alphacephei.com/vosk/models",
        modelPath,
      );
      this._available = false;
      return false;
    }

    try {
      this.model = new this.vosk.Model(modelPath);
      this._available = true;
      logger.info("Vosk STT initialized with model: %s", modelPath);
    } catch (err) {
      logger.error("Failed to load Vosk model: %s", (err as Error).message);
      this._available = false;
    }

    return this._available;
  }

  async startListening(onResult: (result: STTResult) => void): Promise<void> {
    if (!this._available || !this.vosk || !this.model) {
      logger.warn("Vosk STT not available, cannot start listening");
      return;
    }

    this.recognizer = new this.vosk.Recognizer({
      model: this.model,
      sampleRate: this.sampleRate,
    });

    // Use stdin or a microphone stream — for now, log that we need mic input
    // In production, this would connect to a microphone via node-record-lpcm16 or similar
    logger.info("Vosk STT listening (microphone integration required)");
    logger.debug(
      "Grammar constraint: %s",
      this.grammar.length > 0 ? this.grammar.join(", ") : "none (free dictation)",
    );

    // Placeholder for actual microphone integration
    // Real implementation would pipe mic audio buffers to recognizer.acceptWaveform()
    // and emit results via onResult callback
    void onResult;
  }

  async stopListening(): Promise<void> {
    if (this.micStream) {
      this.micStream = null;
    }
    if (this.recognizer) {
      this.recognizer.free();
      this.recognizer = null;
    }
  }

  setGrammar(words: string[]): void {
    this.grammar = [...words];
  }

  async dispose(): Promise<void> {
    await this.stopListening();
    this.model = null;
    this.vosk = null;
  }
}
