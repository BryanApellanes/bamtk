import type { Model as VoskModel, Recognizer as VoskRecognizer } from "vosk";
import { ChildProcess, spawn } from "node:child_process";
import { STTProvider, STTProviderOptions, STTResult } from "./stt-provider.js";
import { logger } from "../util/logger.js";
import { existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const MODEL_CANDIDATES = [
  resolve(__dirname, "../../models/vosk-model-small-en-us-0.15"),
  resolve(process.cwd(), "models/vosk-model-small-en-us-0.15"),
];

function findModelPath(): string | undefined {
  for (const p of MODEL_CANDIDATES) {
    if (existsSync(p)) return p;
  }
  return undefined;
}

interface VoskModule {
  Model: typeof VoskModel;
  Recognizer: typeof VoskRecognizer;
  setLogLevel(level: number): void;
}

// PowerShell script that captures microphone audio as raw 16-bit PCM via WASAPI
const MIC_CAPTURE_SCRIPT = `
Add-Type -AssemblyName System.Speech
Add-Type @"
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

public class MicCapture {
    [DllImport("winmm.dll")]
    static extern int waveInGetNumDevs();
    [DllImport("winmm.dll")]
    static extern int waveInOpen(out IntPtr handle, int deviceId, ref WAVEFORMATEX format, IntPtr callback, IntPtr instance, int flags);
    [DllImport("winmm.dll")]
    static extern int waveInPrepareHeader(IntPtr handle, ref WAVEHDR header, int size);
    [DllImport("winmm.dll")]
    static extern int waveInAddBuffer(IntPtr handle, ref WAVEHDR header, int size);
    [DllImport("winmm.dll")]
    static extern int waveInStart(IntPtr handle);
    [DllImport("winmm.dll")]
    static extern int waveInStop(IntPtr handle);
    [DllImport("winmm.dll")]
    static extern int waveInClose(IntPtr handle);
    [DllImport("winmm.dll")]
    static extern int waveInReset(IntPtr handle);

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEFORMATEX {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEHDR {
        public IntPtr lpData;
        public int dwBufferLength;
        public int dwBytesRecorded;
        public IntPtr dwUser;
        public int dwFlags;
        public int dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    public static void Record(int sampleRate, Stream output) {
        var fmt = new WAVEFORMATEX();
        fmt.wFormatTag = 1; // PCM
        fmt.nChannels = 1;
        fmt.nSamplesPerSec = sampleRate;
        fmt.wBitsPerSample = 16;
        fmt.nBlockAlign = (short)(fmt.nChannels * fmt.wBitsPerSample / 8);
        fmt.nAvgBytesPerSec = fmt.nSamplesPerSec * fmt.nBlockAlign;
        fmt.cbSize = 0;

        IntPtr hWaveIn;
        int result = waveInOpen(out hWaveIn, -1, ref fmt, IntPtr.Zero, IntPtr.Zero, 0);
        if (result != 0) { Console.Error.WriteLine("waveInOpen failed: " + result); return; }

        int bufSize = sampleRate * 2; // 1 second buffer
        int numBufs = 3;
        var headers = new WAVEHDR[numBufs];
        var buffers = new IntPtr[numBufs];

        for (int i = 0; i < numBufs; i++) {
            buffers[i] = Marshal.AllocHGlobal(bufSize);
            headers[i].lpData = buffers[i];
            headers[i].dwBufferLength = bufSize;
            waveInPrepareHeader(hWaveIn, ref headers[i], Marshal.SizeOf(typeof(WAVEHDR)));
            waveInAddBuffer(hWaveIn, ref headers[i], Marshal.SizeOf(typeof(WAVEHDR)));
        }

        waveInStart(hWaveIn);
        int curBuf = 0;
        while (true) {
            while ((headers[curBuf].dwFlags & 1) == 0) Thread.Sleep(10); // WHDR_DONE
            byte[] data = new byte[headers[curBuf].dwBytesRecorded];
            Marshal.Copy(headers[curBuf].lpData, data, 0, data.Length);
            try { output.Write(data, 0, data.Length); output.Flush(); }
            catch { break; }
            headers[curBuf].dwFlags = 0;
            headers[curBuf].dwBytesRecorded = 0;
            waveInAddBuffer(hWaveIn, ref headers[curBuf], Marshal.SizeOf(typeof(WAVEHDR)));
            curBuf = (curBuf + 1) % numBufs;
        }
        waveInStop(hWaveIn);
        waveInReset(hWaveIn);
        waveInClose(hWaveIn);
        for (int i = 0; i < numBufs; i++) Marshal.FreeHGlobal(buffers[i]);
    }
}
"@
[MicCapture]::Record(SAMPLE_RATE_PLACEHOLDER, [Console]::OpenStandardOutput())
`;

export class VoskSttProvider implements STTProvider {
  readonly name = "vosk";
  private _available = false;
  private vosk: VoskModule | null = null;
  private model: VoskModel | null = null;
  private recognizer: VoskRecognizer | null = null;
  private sampleRate = 16000;
  private grammar: string[] = [];
  private micProcess: ChildProcess | null = null;
  private silenceFrames = 0;
  private hasSpoken = false;

  get isAvailable(): boolean {
    return this._available;
  }

  async initialize(options: STTProviderOptions): Promise<boolean> {
    this.sampleRate = options.sampleRate ?? 16000;
    const modelPath = options.modelPath ?? findModelPath();

    try {
      this.vosk = (await import("vosk")) as unknown as VoskModule;
      this.vosk.setLogLevel(-1);
    } catch (err) {
      logger.warn("Vosk module not available: %s", (err as Error).message);
      this._available = false;
      return false;
    }

    if (!modelPath) {
      logger.warn(
        "Vosk model not found. Download from https://alphacephei.com/vosk/models and extract to models/",
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

    await this.stopListening();

    this.recognizer = new this.vosk.Recognizer({
      model: this.model,
      sampleRate: this.sampleRate,
    });

    this.silenceFrames = 0;
    this.hasSpoken = false;

    const script = MIC_CAPTURE_SCRIPT.replace("SAMPLE_RATE_PLACEHOLDER", String(this.sampleRate));

    this.micProcess = spawn("powershell", ["-NoProfile", "-Command", script], {
      stdio: ["ignore", "pipe", "pipe"],
      windowsHide: true,
    });

    logger.info("Vosk STT listening via microphone");

    this.micProcess.stdout?.on("data", (chunk: Buffer) => {
      if (!this.recognizer) return;

      const done = this.recognizer.acceptWaveform(chunk);
      if (done) {
        const result = this.recognizer.result();
        if (result.text && result.text.trim()) {
          this.hasSpoken = true;
          logger.debug("Vosk partial result: %s", result.text);
        }
      } else {
        const partial = this.recognizer.partialResult();
        if (partial.partial && partial.partial.trim()) {
          this.hasSpoken = true;
          this.silenceFrames = 0;
        }
      }

      // Detect end of speech: after user has spoken, wait for ~1.5s of silence
      if (this.hasSpoken) {
        const partial = this.recognizer.partialResult();
        if (!partial.partial || !partial.partial.trim()) {
          this.silenceFrames++;
          // Each chunk is ~1s of audio, so 2 frames ≈ 2s silence
          if (this.silenceFrames >= 2) {
            const finalResult = this.recognizer.result();
            if (finalResult.text && finalResult.text.trim()) {
              onResult({
                text: finalResult.text.trim(),
                confidence: 0.85,
                isFinal: true,
              });
              this.stopListening();
            }
          }
        } else {
          this.silenceFrames = 0;
        }
      }
    });

    this.micProcess.stderr?.on("data", (chunk: Buffer) => {
      const msg = chunk.toString().trim();
      if (msg) logger.debug("Mic capture stderr: %s", msg);
    });

    this.micProcess.on("close", () => {
      // If mic process exits while we have speech, emit final result
      if (this.recognizer && this.hasSpoken) {
        const finalResult = this.recognizer.result();
        if (finalResult.text && finalResult.text.trim()) {
          onResult({
            text: finalResult.text.trim(),
            confidence: 0.85,
            isFinal: true,
          });
        }
      }
      this.micProcess = null;
    });
  }

  async stopListening(): Promise<void> {
    if (this.micProcess) {
      this.micProcess.kill();
      this.micProcess = null;
    }
    if (this.recognizer) {
      this.recognizer.free();
      this.recognizer = null;
    }
    this.silenceFrames = 0;
    this.hasSpoken = false;
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
