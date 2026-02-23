import { ChildProcess, spawn } from "node:child_process";
import { TTSProvider, TTSOptions } from "./tts-provider.js";
import { logger } from "../util/logger.js";

export class SapiTtsProvider implements TTSProvider {
  readonly name = "sapi";
  private _available = false;
  private rate = 1;
  private volume = 80;
  private activeProcess: ChildProcess | null = null;

  get isAvailable(): boolean {
    return this._available;
  }

  async initialize(options: TTSOptions): Promise<boolean> {
    this.rate = options.rate ?? 1;
    this.volume = Math.round((options.volume ?? 0.8) * 100);

    try {
      const result = await this.runPowershell(
        "Add-Type -AssemblyName System.Speech; Write-Output 'ok'",
      );
      this._available = result.trim() === "ok";
    } catch {
      this._available = false;
    }

    if (this._available) {
      logger.info("SAPI TTS initialized");
    } else {
      logger.warn("SAPI TTS not available");
    }

    return this._available;
  }

  async speak(text: string): Promise<void> {
    if (!this._available) {
      logger.warn("SAPI TTS not available, skipping speak");
      return;
    }

    await this.cancel();

    const escaped = text.replace(/'/g, "''").replace(/\n/g, " ");
    const sapiRate = Math.round((this.rate - 1) * 5);

    const script = [
      "Add-Type -AssemblyName System.Speech",
      "$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer",
      `$synth.Rate = ${sapiRate}`,
      `$synth.Volume = ${this.volume}`,
      `$synth.Speak('${escaped}')`,
      "$synth.Dispose()",
    ].join("; ");

    return new Promise<void>((resolve, reject) => {
      this.activeProcess = spawn("powershell", ["-NoProfile", "-Command", script], {
        stdio: ["ignore", "pipe", "pipe"],
        windowsHide: true,
      });

      this.activeProcess.on("close", () => {
        this.activeProcess = null;
        resolve();
      });

      this.activeProcess.on("error", (err) => {
        this.activeProcess = null;
        reject(err);
      });
    });
  }

  async cancel(): Promise<void> {
    if (this.activeProcess) {
      this.activeProcess.kill();
      this.activeProcess = null;
    }
  }

  async dispose(): Promise<void> {
    await this.cancel();
  }

  private runPowershell(script: string): Promise<string> {
    return new Promise((resolve, reject) => {
      const proc = spawn("powershell", ["-NoProfile", "-Command", script], {
        stdio: ["ignore", "pipe", "pipe"],
        windowsHide: true,
      });

      let stdout = "";
      proc.stdout.on("data", (chunk: Buffer) => {
        stdout += chunk.toString();
      });

      proc.on("close", () => resolve(stdout));
      proc.on("error", reject);
    });
  }
}
