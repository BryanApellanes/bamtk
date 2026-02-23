import { describe, it, expect } from "vitest";
import { validateConfig } from "../src/config/config-manager.js";
import { DEFAULT_CONFIG, BamvoiceConfig } from "../src/config/config-schema.js";

describe("ConfigManager", () => {
  it("should accept valid default config", () => {
    const result = validateConfig({ ...DEFAULT_CONFIG });
    expect(result).toEqual(DEFAULT_CONFIG);
  });

  it("should reject invalid sampleRate", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.stt.sampleRate = 100;
    expect(() => validateConfig(config)).toThrow("sampleRate");
  });

  it("should reject invalid TTS rate", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.tts.rate = 0;
    expect(() => validateConfig(config)).toThrow("TTS rate");
  });

  it("should reject invalid TTS volume", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.tts.volume = 2;
    expect(() => validateConfig(config)).toThrow("TTS volume");
  });

  it("should reject low confirmation timeout", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.control.confirmationTimeoutSeconds = 2;
    expect(() => validateConfig(config)).toThrow("Confirmation timeout");
  });

  it("should reject low listening timeout", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.control.listeningTimeoutSeconds = 1;
    expect(() => validateConfig(config)).toThrow("Listening timeout");
  });

  it("should reject invalid IPC port", () => {
    const config = structuredClone(DEFAULT_CONFIG);
    config.hooks.ipcPort = 80;
    expect(() => validateConfig(config)).toThrow("IPC port");
  });
});
