import { describe, it, expect } from "vitest";
import { IntentDetector } from "../src/intent/intent-detector.js";
import { ControlIntent } from "../src/intent/intent-types.js";
import { DEFAULT_CONFIG } from "../src/config/config-schema.js";

function createDetector() {
  return new IntentDetector(DEFAULT_CONFIG.control);
}

describe("IntentDetector", () => {
  it("should detect 'proceed' as execute intent", () => {
    const d = createDetector();
    const result = d.detect("proceed");
    expect(result.intent).toBe(ControlIntent.Execute);
    expect(result.confidence).toBeGreaterThanOrEqual(0.6);
  });

  it("should detect 'go ahead' as execute intent", () => {
    const d = createDetector();
    const result = d.detect("go ahead");
    expect(result.intent).toBe(ControlIntent.Execute);
  });

  it("should detect 'cancel' as cancel intent", () => {
    const d = createDetector();
    const result = d.detect("cancel");
    expect(result.intent).toBe(ControlIntent.Cancel);
  });

  it("should detect 'repeat' as repeat intent", () => {
    const d = createDetector();
    const result = d.detect("repeat");
    expect(result.intent).toBe(ControlIntent.Repeat);
  });

  it("should detect 'say again' as repeat intent", () => {
    const d = createDetector();
    const result = d.detect("say again");
    expect(result.intent).toBe(ControlIntent.Repeat);
  });

  it("should detect 'rephrase' as rephrase intent", () => {
    const d = createDetector();
    const result = d.detect("rephrase");
    expect(result.intent).toBe(ControlIntent.Rephrase);
  });

  it("should detect 'forget it' as cancel intent", () => {
    const d = createDetector();
    const result = d.detect("forget it");
    expect(result.intent).toBe(ControlIntent.Cancel);
  });

  it("should return unknown for gibberish", () => {
    const d = createDetector();
    const result = d.detect("flibbertigibbet wobblesnort");
    expect(result.intent).toBe(ControlIntent.Unknown);
  });

  it("should return unknown for empty input", () => {
    const d = createDetector();
    const result = d.detect("");
    expect(result.intent).toBe(ControlIntent.Unknown);
    expect(result.confidence).toBe(0);
  });

  it("should be case insensitive", () => {
    const d = createDetector();
    const result = d.detect("PROCEED");
    expect(result.intent).toBe(ControlIntent.Execute);
  });

  it("should detect intent in longer phrases", () => {
    const d = createDetector();
    const result = d.detect("yes please proceed with the plan");
    expect(result.intent).toBe(ControlIntent.Execute);
  });

  it("should return all keywords", () => {
    const d = createDetector();
    const keywords = d.getAllKeywords();
    expect(keywords).toContain("proceed");
    expect(keywords).toContain("cancel");
    expect(keywords).toContain("repeat");
    expect(keywords).toContain("rephrase");
    expect(keywords.length).toBeGreaterThan(10);
  });
});
