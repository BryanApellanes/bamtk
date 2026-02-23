export interface TTSOptions {
  rate?: number;
  volume?: number;
}

export interface TTSProvider {
  readonly name: string;
  readonly isAvailable: boolean;
  initialize(options: TTSOptions): Promise<boolean>;
  speak(text: string): Promise<void>;
  cancel(): Promise<void>;
  dispose(): Promise<void>;
}
