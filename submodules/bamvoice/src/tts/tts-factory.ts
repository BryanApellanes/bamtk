import { TTSProvider } from "./tts-provider.js";
import { SapiTtsProvider } from "./sapi-tts-provider.js";
import { TTSConfig } from "../config/config-schema.js";

export function createTtsProvider(config: TTSConfig): TTSProvider {
  switch (config.provider) {
    case "sapi":
      return new SapiTtsProvider();
    default:
      throw new Error(`Unknown TTS provider: ${config.provider}`);
  }
}
