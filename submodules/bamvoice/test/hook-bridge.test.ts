import { describe, it, expect, beforeEach, afterEach } from "vitest";
import { createConnection } from "node:net";
import { HookBridge, HookResponse } from "../src/hooks/hook-bridge.js";
import { ActionPlan } from "../src/planning/action-plan.js";

function createTestPlan(): ActionPlan {
  return {
    planId: "test-1",
    summary: "test",
    originalPrompt: "test",
    actions: [
      {
        step: 1,
        type: "read",
        description: "Read a file",
        tool: "Read",
        toolInput: { file_path: "x.ts" },
        affectedResources: ["x.ts"],
        risk: "safe",
        reversible: true,
      },
      {
        step: 2,
        type: "edit",
        description: "Edit a file",
        tool: "Edit",
        toolInput: {},
        affectedResources: ["x.ts"],
        risk: "low",
        reversible: true,
      },
    ],
    overallRisk: "low",
    filesAffected: 1,
    affectedFilePaths: ["x.ts"],
    fullyReversible: true,
    warnings: [],
  };
}

function queryBridge(port: number, request: Record<string, unknown>): Promise<HookResponse> {
  return new Promise((resolve, reject) => {
    const socket = createConnection({ port, host: "127.0.0.1" }, () => {
      socket.write(JSON.stringify(request) + "\n");
    });

    let buffer = "";
    socket.on("data", (chunk: Buffer) => {
      buffer += chunk.toString();
    });
    socket.on("end", () => {
      try {
        resolve(JSON.parse(buffer.trim()) as HookResponse);
      } catch (e) {
        reject(e);
      }
    });
    socket.on("error", reject);
  });
}

describe("HookBridge", () => {
  let bridge: HookBridge;
  const TEST_PORT = 19847;

  beforeEach(async () => {
    bridge = new HookBridge();
    await bridge.start(TEST_PORT);
  });

  afterEach(async () => {
    await bridge.stop();
  });

  it("should allow calls when no plan is set", async () => {
    const response = await queryBridge(TEST_PORT, {
      tool: "Read",
      toolInput: { file_path: "x.ts" },
      requestId: "r1",
    });
    expect(response.decision).toBe("allow");
  });

  it("should allow planned tool calls in order", async () => {
    bridge.setActivePlan(createTestPlan());

    const r1 = await queryBridge(TEST_PORT, {
      tool: "Read",
      toolInput: { file_path: "x.ts" },
      requestId: "r1",
    });
    expect(r1.decision).toBe("allow");
    expect(r1.announce).toContain("Step 1");
  });

  it("should ask for unplanned tool calls", async () => {
    bridge.setActivePlan(createTestPlan());

    const r1 = await queryBridge(TEST_PORT, {
      tool: "Bash",
      toolInput: { command: "echo hi" },
      requestId: "r1",
    });
    expect(r1.decision).toBe("ask");
    expect(r1.announce).toContain("Unplanned");
  });
});
