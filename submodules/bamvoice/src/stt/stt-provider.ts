export interface STTProviderOptions {
  language?: string;
  sampleRate?: number;
  modelPath?: string;
}

export interface STTResult {
  text: string;
  confidence: number;
  isFinal: boolean;
}

export interface STTProvider {
  readonly name: string;
  readonly isAvailable: boolean;
  initialize(options: STTProviderOptions): Promise<boolean>;
  startListening(onResult: (result: STTResult) => void): Promise<void>;
  stopListening(): Promise<void>;
  setGrammar(words: string[]): void;
  dispose(): Promise<void>;
}
