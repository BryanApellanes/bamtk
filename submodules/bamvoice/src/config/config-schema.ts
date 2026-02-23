export type STTProviderName = "vosk" | "sapi";
export type TTSProviderName = "sapi";

export interface STTConfig {
  provider: STTProviderName;
  language: string;
  sampleRate: number;
}

export interface TTSConfig {
  provider: TTSProviderName;
  rate: number;
  volume: number;
}

export interface ClaudeConfig {
  cliPath: string;
  planningModel: string;
  executionModel: string;
  planningBudgetUsd: number;
  executionBudgetUsd: number;
}

export interface IntentPhrases {
  execute: string[];
  repeat: string[];
  rephrase: string[];
  cancel: string[];
}

export interface ControlConfig {
  executionKeyword: string;
  confirmationTimeoutSeconds: number;
  listeningTimeoutSeconds: number;
  minIntentConfidence: number;
  intentPhrases: IntentPhrases;
}

export interface HooksConfig {
  enablePreToolUseHook: boolean;
  ipcPort: number;
}

export interface BamvoiceConfig {
  stt: STTConfig;
  tts: TTSConfig;
  claude: ClaudeConfig;
  control: ControlConfig;
  hooks: HooksConfig;
}

export const DEFAULT_CONFIG: BamvoiceConfig = {
  stt: {
    provider: "vosk",
    language: "en-US",
    sampleRate: 16000,
  },
  tts: {
    provider: "sapi",
    rate: 1.0,
    volume: 0.8,
  },
  claude: {
    cliPath: "claude",
    planningModel: "sonnet",
    executionModel: "sonnet",
    planningBudgetUsd: 0.50,
    executionBudgetUsd: 5.00,
  },
  control: {
    executionKeyword: "proceed",
    confirmationTimeoutSeconds: 60,
    listeningTimeoutSeconds: 30,
    minIntentConfidence: 0.6,
    intentPhrases: {
      execute: ["proceed", "go ahead", "do it", "execute", "yes", "confirm"],
      repeat: ["repeat", "say again", "what was that", "again"],
      rephrase: ["rephrase", "try again", "different approach", "revise"],
      cancel: ["cancel", "stop", "abort", "nevermind", "forget it", "no"],
    },
  },
  hooks: {
    enablePreToolUseHook: true,
    ipcPort: 9847,
  },
};
