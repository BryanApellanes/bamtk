declare module "vosk" {
  export class Model {
    constructor(path: string);
    free(): void;
  }

  export class Recognizer {
    constructor(options: { model: Model; sampleRate: number });
    acceptWaveform(buffer: Buffer): boolean;
    result(): { text: string };
    partialResult(): { partial: string };
    free(): void;
  }

  export function setLogLevel(level: number): void;
}
