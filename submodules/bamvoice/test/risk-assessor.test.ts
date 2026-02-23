import { describe, it, expect } from "vitest";
import { assessActionRisk, assessOverallRisk, isDestructive } from "../src/planning/risk-assessor.js";
import { PlannedAction, RiskLevel } from "../src/planning/action-plan.js";

function makeAction(overrides: Partial<PlannedAction>): PlannedAction {
  return {
    step: 1,
    type: "read",
    description: "test",
    tool: "Read",
    toolInput: {},
    affectedResources: [],
    risk: "safe",
    reversible: true,
    ...overrides,
  };
}

describe("RiskAssessor", () => {
  it("should rate Read as safe", () => {
    expect(assessActionRisk(makeAction({ tool: "Read" }))).toBe("safe");
  });

  it("should rate Edit as low", () => {
    expect(assessActionRisk(makeAction({ tool: "Edit" }))).toBe("low");
  });

  it("should rate Bash as medium", () => {
    expect(assessActionRisk(makeAction({ tool: "Bash", toolInput: { command: "npm test" } }))).toBe("medium");
  });

  it("should rate rm -rf as destructive", () => {
    expect(assessActionRisk(makeAction({
      tool: "Bash",
      toolInput: { command: "rm -rf /tmp/stuff" },
    }))).toBe("destructive");
  });

  it("should rate git push --force as destructive", () => {
    expect(assessActionRisk(makeAction({
      tool: "Bash",
      toolInput: { command: "git push --force origin main" },
    }))).toBe("destructive");
  });

  it("should rate delete type as high", () => {
    expect(assessActionRisk(makeAction({ type: "delete", tool: "Bash" }))).toBe("high");
  });

  it("should compute overall risk as max of all actions", () => {
    const actions = [
      makeAction({ risk: "safe" }),
      makeAction({ risk: "low" }),
      makeAction({ risk: "medium" }),
    ];
    expect(assessOverallRisk(actions)).toBe("medium");
  });

  it("should identify destructive risks", () => {
    expect(isDestructive("safe")).toBe(false);
    expect(isDestructive("low")).toBe(false);
    expect(isDestructive("medium")).toBe(false);
    expect(isDestructive("high")).toBe(true);
    expect(isDestructive("destructive")).toBe(true);
  });
});
