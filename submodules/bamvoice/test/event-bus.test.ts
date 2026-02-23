import { describe, it, expect, vi } from "vitest";
import { EventBus } from "../src/core/event-bus.js";

interface TestEvents {
  hello: string;
  count: number;
  data: { x: number; y: number };
}

describe("EventBus", () => {
  it("should emit and receive events", () => {
    const bus = new EventBus<TestEvents>();
    const handler = vi.fn();

    bus.on("hello", handler);
    bus.emit("hello", "world");

    expect(handler).toHaveBeenCalledWith("world");
  });

  it("should support multiple listeners", () => {
    const bus = new EventBus<TestEvents>();
    const h1 = vi.fn();
    const h2 = vi.fn();

    bus.on("count", h1);
    bus.on("count", h2);
    bus.emit("count", 42);

    expect(h1).toHaveBeenCalledWith(42);
    expect(h2).toHaveBeenCalledWith(42);
  });

  it("should unsubscribe via returned function", () => {
    const bus = new EventBus<TestEvents>();
    const handler = vi.fn();

    const unsub = bus.on("hello", handler);
    bus.emit("hello", "first");
    unsub();
    bus.emit("hello", "second");

    expect(handler).toHaveBeenCalledTimes(1);
    expect(handler).toHaveBeenCalledWith("first");
  });

  it("should support once()", () => {
    const bus = new EventBus<TestEvents>();
    const handler = vi.fn();

    bus.once("count", handler);
    bus.emit("count", 1);
    bus.emit("count", 2);

    expect(handler).toHaveBeenCalledTimes(1);
    expect(handler).toHaveBeenCalledWith(1);
  });

  it("should removeAllListeners for a specific event", () => {
    const bus = new EventBus<TestEvents>();
    const h1 = vi.fn();
    const h2 = vi.fn();

    bus.on("hello", h1);
    bus.on("count", h2);
    bus.removeAllListeners("hello");

    bus.emit("hello", "gone");
    bus.emit("count", 99);

    expect(h1).not.toHaveBeenCalled();
    expect(h2).toHaveBeenCalledWith(99);
  });

  it("should removeAllListeners globally", () => {
    const bus = new EventBus<TestEvents>();
    const h1 = vi.fn();
    const h2 = vi.fn();

    bus.on("hello", h1);
    bus.on("count", h2);
    bus.removeAllListeners();

    bus.emit("hello", "gone");
    bus.emit("count", 0);

    expect(h1).not.toHaveBeenCalled();
    expect(h2).not.toHaveBeenCalled();
  });

  it("should handle emitting events with no listeners", () => {
    const bus = new EventBus<TestEvents>();
    expect(() => bus.emit("hello", "nobody")).not.toThrow();
  });
});
