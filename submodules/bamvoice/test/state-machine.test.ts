import { describe, it, expect, vi } from "vitest";
import { StateMachine } from "../src/core/state-machine.js";

type TestState = "idle" | "active" | "done" | "error";
type TestEvent = "START" | "FINISH" | "FAIL" | "RESET";

function createTestSM() {
  return new StateMachine<TestState, TestEvent>({
    initial: "idle",
    transitions: [
      { from: "idle", event: "START", to: "active" },
      { from: "active", event: "FINISH", to: "done" },
      { from: "active", event: "FAIL", to: "error" },
      { from: "done", event: "RESET", to: "idle" },
      { from: "error", event: "RESET", to: "idle" },
    ],
  });
}

describe("StateMachine", () => {
  it("should start in initial state", () => {
    const sm = createTestSM();
    expect(sm.state).toBe("idle");
  });

  it("should transition on valid events", async () => {
    const sm = createTestSM();
    const result = await sm.send("START");
    expect(result).toBe(true);
    expect(sm.state).toBe("active");
  });

  it("should reject invalid events", async () => {
    const sm = createTestSM();
    const result = await sm.send("FINISH");
    expect(result).toBe(false);
    expect(sm.state).toBe("idle");
  });

  it("should follow a transition chain", async () => {
    const sm = createTestSM();
    await sm.send("START");
    await sm.send("FINISH");
    expect(sm.state).toBe("done");
    await sm.send("RESET");
    expect(sm.state).toBe("idle");
  });

  it("should call onChange listeners", async () => {
    const sm = createTestSM();
    const handler = vi.fn();
    sm.onChange(handler);

    await sm.send("START");

    expect(handler).toHaveBeenCalledWith("idle", "active", "START");
  });

  it("should support guards", async () => {
    let allowed = false;
    const sm = new StateMachine<TestState, TestEvent>({
      initial: "idle",
      transitions: [
        { from: "idle", event: "START", to: "active", guard: () => allowed },
      ],
    });

    expect(await sm.send("START")).toBe(false);
    expect(sm.state).toBe("idle");

    allowed = true;
    expect(await sm.send("START")).toBe(true);
    expect(sm.state).toBe("active");
  });

  it("should call transition actions", async () => {
    const action = vi.fn();
    const sm = new StateMachine<TestState, TestEvent>({
      initial: "idle",
      transitions: [
        { from: "idle", event: "START", to: "active", action },
      ],
    });

    await sm.send("START");
    expect(action).toHaveBeenCalled();
  });

  it("should call onEnter and onExit handlers", async () => {
    const onExit = vi.fn();
    const onEnter = vi.fn();
    const sm = new StateMachine<TestState, TestEvent>({
      initial: "idle",
      transitions: [
        { from: "idle", event: "START", to: "active" },
      ],
      onExit: { idle: onExit },
      onEnter: { active: onEnter },
    });

    await sm.send("START");
    expect(onExit).toHaveBeenCalled();
    expect(onEnter).toHaveBeenCalled();
  });

  it("should report canSend correctly", () => {
    const sm = createTestSM();
    expect(sm.canSend("START")).toBe(true);
    expect(sm.canSend("FINISH")).toBe(false);
  });

  it("should unsubscribe onChange listeners", async () => {
    const sm = createTestSM();
    const handler = vi.fn();
    const unsub = sm.onChange(handler);
    unsub();

    await sm.send("START");
    expect(handler).not.toHaveBeenCalled();
  });
});
