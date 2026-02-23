import { ChildProcess, spawn } from "node:child_process";
import { STTProvider, STTProviderOptions, STTResult } from "./stt-provider.js";
import { logger } from "../util/logger.js";

export class SapiSttProvider implements STTProvider {
  readonly name = "sapi";
  private _available = false;
  private activeProcess: ChildProcess | null = null;
  private grammar: string[] = [];

  get isAvailable(): boolean {
    return this._available;
  }

  async initialize(_options: STTProviderOptions): Promise<boolean> {
    try {
      const result = await this.runPowershell(
        "Add-Type -AssemblyName System.Speech; Write-Output 'ok'",
      );
      this._available = result.trim() === "ok";
    } catch {
      this._available = false;
    }

    if (this._available) {
      logger.info("SAPI STT initialized");
    } else {
      logger.warn("SAPI STT not available");
    }

    return this._available;
  }

  async startListening(onResult: (result: STTResult) => void): Promise<void> {
    if (!this._available) {
      logger.warn("SAPI STT not available");
      return;
    }

    await this.stopListening();

    const grammarSection = this.grammar.length > 0
      ? this.buildGrammarScript()
      : this.buildDictationScript();

    const script = `
Add-Type -AssemblyName System.Speech
$recognizer = New-Object System.Speech.Recognition.SpeechRecognitionEngine
${grammarSection}
$recognizer.SetInputToDefaultAudioDevice()
$result = $recognizer.Recognize([TimeSpan]::FromSeconds(30))
if ($result) {
  Write-Output ("RESULT:" + $result.Confidence.ToString("F2") + ":" + $result.Text)
} else {
  Write-Output "TIMEOUT"
}
$recognizer.Dispose()
`.trim();

    return new Promise<void>((resolve) => {
      this.activeProcess = spawn("powershell", ["-NoProfile", "-Command", script], {
        stdio: ["ignore", "pipe", "pipe"],
        windowsHide: true,
      });

      let output = "";
      this.activeProcess.stdout?.on("data", (chunk: Buffer) => {
        output += chunk.toString();
      });

      this.activeProcess.on("close", () => {
        this.activeProcess = null;
        const lines = output.trim().split("\n");
        for (const line of lines) {
          const trimmed = line.trim();
          if (trimmed.startsWith("RESULT:")) {
            const parts = trimmed.substring(7).split(":");
            const confidence = parseFloat(parts[0]) || 0;
            const text = parts.slice(1).join(":").trim();
            onResult({ text, confidence, isFinal: true });
          } else if (trimmed === "TIMEOUT") {
            onResult({ text: "", confidence: 0, isFinal: true });
          }
        }
        resolve();
      });

      this.activeProcess.on("error", () => {
        this.activeProcess = null;
        resolve();
      });
    });
  }

  async stopListening(): Promise<void> {
    if (this.activeProcess) {
      this.activeProcess.kill();
      this.activeProcess = null;
    }
  }

  setGrammar(words: string[]): void {
    this.grammar = [...words];
  }

  async dispose(): Promise<void> {
    await this.stopListening();
  }

  private buildGrammarScript(): string {
    const choices = this.grammar.map((w) => `"${w}"`).join(", ");
    return `
$choices = New-Object System.Speech.Recognition.Choices
$choices.Add(@(${choices}))
$builder = New-Object System.Speech.Recognition.GrammarBuilder
$builder.Append($choices)
$grammar = New-Object System.Speech.Recognition.Grammar($builder)
$recognizer.LoadGrammar($grammar)
`.trim();
  }

  private buildDictationScript(): string {
    return `
$grammar = New-Object System.Speech.Recognition.DictationGrammar
$recognizer.LoadGrammar($grammar)
`.trim();
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
