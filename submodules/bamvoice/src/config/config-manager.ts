import { readFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { resolve } from "node:path";
import { BamvoiceConfig, DEFAULT_CONFIG } from "./config-schema.js";

function deepMerge<T extends object>(target: T, source: Partial<T>): T {
  const result = { ...target };
  for (const key of Object.keys(source) as Array<keyof T>) {
    const sourceVal = source[key];
    const targetVal = target[key];
    if (
      sourceVal !== undefined &&
      sourceVal !== null &&
      typeof sourceVal === "object" &&
      !Array.isArray(sourceVal) &&
      typeof targetVal === "object" &&
      targetVal !== null &&
      !Array.isArray(targetVal)
    ) {
      result[key] = deepMerge(
        targetVal as Record<string, unknown>,
        sourceVal as Record<string, unknown>,
      ) as T[keyof T];
    } else if (sourceVal !== undefined) {
      result[key] = sourceVal as T[keyof T];
    }
  }
  return result;
}

export async function loadConfig(configPath?: string): Promise<BamvoiceConfig> {
  const paths = configPath
    ? [configPath]
    : [
        resolve(process.cwd(), "bamvoice.config.json"),
        resolve(process.cwd(), ".bamvoice.json"),
      ];

  for (const p of paths) {
    if (existsSync(p)) {
      const raw = await readFile(p, "utf-8");
      const parsed = JSON.parse(raw) as Partial<BamvoiceConfig>;
      return validateConfig(deepMerge(DEFAULT_CONFIG, parsed));
    }
  }

  return { ...DEFAULT_CONFIG };
}

export function validateConfig(config: BamvoiceConfig): BamvoiceConfig {
  if (config.stt.sampleRate < 8000 || config.stt.sampleRate > 48000) {
    throw new Error(`Invalid sampleRate: ${config.stt.sampleRate}. Must be between 8000 and 48000.`);
  }
  if (config.tts.rate < 0.1 || config.tts.rate > 10) {
    throw new Error(`Invalid TTS rate: ${config.tts.rate}. Must be between 0.1 and 10.`);
  }
  if (config.tts.volume < 0 || config.tts.volume > 1) {
    throw new Error(`Invalid TTS volume: ${config.tts.volume}. Must be between 0 and 1.`);
  }
  if (config.control.confirmationTimeoutSeconds < 5) {
    throw new Error("Confirmation timeout must be at least 5 seconds.");
  }
  if (config.control.listeningTimeoutSeconds < 5) {
    throw new Error("Listening timeout must be at least 5 seconds.");
  }
  if (config.hooks.ipcPort < 1024 || config.hooks.ipcPort > 65535) {
    throw new Error(`Invalid IPC port: ${config.hooks.ipcPort}. Must be between 1024 and 65535.`);
  }
  return config;
}
