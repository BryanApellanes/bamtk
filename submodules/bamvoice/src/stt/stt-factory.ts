import { STTProvider } from "./stt-provider.js";
import { VoskSttProvider } from "./vosk-stt-provider.js";
import { SapiSttProvider } from "./sapi-stt-provider.js";
import { STTConfig } from "../config/config-schema.js";
import { logger } from "../util/logger.js";

export async function createSttProvider(config: STTConfig): Promise<STTProvider> {
  if (config.provider === "vosk") {
    const vosk = new VoskSttProvider();
    const ok = await vosk.initialize({
      language: config.language,
      sampleRate: config.sampleRate,
    });
    if (ok) return vosk;
    logger.warn("Vosk not available, falling back to SAPI STT");
  }

  const sapi = new SapiSttProvider();
  const ok = await sapi.initialize({
    language: config.language,
    sampleRate: config.sampleRate,
  });
  if (ok) return sapi;

  throw new Error("No STT provider available. Install Vosk or ensure Windows SAPI is configured.");
}
