import { describe, it, expect } from "vitest";
import { isActionPlan, maxRisk, RISK_ORDER } from "../src/planning/action-plan.js";

describe("ActionPlan", () => {
  it("should validate a valid ActionPlan", () => {
    const plan = {
      planId: "test-1",
      summary: "test plan",
      originalPrompt: "do something",
      actions: [],
      overallRisk: "safe",
      filesAffected: 0,
      affectedFilePaths: [],
      fullyReversible: true,
      warnings: [],
    };
    expect(isActionPlan(plan)).toBe(true);
  });

  it("should reject invalid objects", () => {
    expect(isActionPlan(null)).toBe(false);
    expect(isActionPlan({})).toBe(false);
    expect(isActionPlan({ planId: "x" })).toBe(false);
    expect(isActionPlan("string")).toBe(false);
    expect(isActionPlan(42)).toBe(false);
  });

  it("should compute maxRisk correctly", () => {
    expect(maxRisk("safe", "low")).toBe("low");
    expect(maxRisk("high", "low")).toBe("high");
    expect(maxRisk("destructive", "safe")).toBe("destructive");
    expect(maxRisk("medium", "medium")).toBe("medium");
  });

  it("should have correct risk ordering", () => {
    expect(RISK_ORDER["safe"]).toBeLessThan(RISK_ORDER["low"]);
    expect(RISK_ORDER["low"]).toBeLessThan(RISK_ORDER["medium"]);
    expect(RISK_ORDER["medium"]).toBeLessThan(RISK_ORDER["high"]);
    expect(RISK_ORDER["high"]).toBeLessThan(RISK_ORDER["destructive"]);
  });
});
