export enum ControlIntent {
  Execute = "execute",
  Repeat = "repeat",
  Rephrase = "rephrase",
  Cancel = "cancel",
  Unknown = "unknown",
}

export interface IntentResult {
  intent: ControlIntent;
  confidence: number;
  rawText: string;
}
