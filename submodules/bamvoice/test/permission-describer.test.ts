import { describe, it, expect } from "vitest";
import { describeActionPlan } from "../src/planning/permission-describer.js";
import { ActionPlan } from "../src/planning/action-plan.js";

function createTestPlan(overrides?: Partial<ActionPlan>): ActionPlan {
  return {
    planId: "test-plan-1",
    summary: "Add a utility function",
    originalPrompt: "add formatCurrency to utils",
    actions: [
      {
        step: 1,
        type: "read",
        description: "Read utils.ts to understand existing patterns",
        tool: "Read",
        toolInput: { file_path: "src/utils.ts" },
        affectedResources: ["src/utils.ts"],
        risk: "safe",
        reversible: true,
      },
      {
        step: 2,
        type: "edit",
        description: "Edit utils.ts to add formatCurrency function",
        tool: "Edit",
        toolInput: { file_path: "src/utils.ts", old_string: "", new_string: "" },
        affectedResources: ["src/utils.ts"],
        risk: "low",
        reversible: true,
        reversalMethod: "git checkout",
      },
    ],
    overallRisk: "low",
    filesAffected: 1,
    affectedFilePaths: ["src/utils.ts"],
    fullyReversible: true,
    warnings: [],
    ...overrides,
  };
}

describe("PermissionDescriber", () => {
  it("should describe a plan with action count and file count", () => {
    const desc = describeActionPlan(createTestPlan());
    expect(desc).toContain("2 actions");
    expect(desc).toContain("1 file");
  });

  it("should describe each step", () => {
    const desc = describeActionPlan(createTestPlan());
    expect(desc).toContain("Step 1:");
    expect(desc).toContain("Step 2:");
    expect(desc).toContain("read-only");
  });

  it("should include risk level", () => {
    const desc = describeActionPlan(createTestPlan());
    expect(desc).toContain("Overall risk level: low");
  });

  it("should note full reversibility", () => {
    const desc = describeActionPlan(createTestPlan());
    expect(desc).toContain("All changes are reversible");
  });

  it("should warn about non-reversible plans", () => {
    const desc = describeActionPlan(createTestPlan({ fullyReversible: false }));
    expect(desc).toContain("may not be easily reversible");
  });

  it("should include warnings", () => {
    const desc = describeActionPlan(createTestPlan({ warnings: ["File may not exist"] }));
    expect(desc).toContain("Warning: File may not exist");
  });

  it("should include confirmation instructions", () => {
    const desc = describeActionPlan(createTestPlan());
    expect(desc).toContain("proceed");
    expect(desc).toContain("repeat");
    expect(desc).toContain("rephrase");
    expect(desc).toContain("cancel");
  });
});
