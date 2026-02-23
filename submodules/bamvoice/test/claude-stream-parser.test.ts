import { describe, it, expect } from "vitest";
import {
  parseStreamLine,
  parseStreamOutput,
  extractSessionId,
  extractToolCalls,
  extractTextContent,
} from "../src/claude/claude-stream-parser.js";

describe("ClaudeStreamParser", () => {
  it("should parse a valid JSON line", () => {
    const msg = parseStreamLine('{"type":"text","content":"hello"}');
    expect(msg).toEqual({ type: "text", content: "hello" });
  });

  it("should return null for empty lines", () => {
    expect(parseStreamLine("")).toBeNull();
    expect(parseStreamLine("   ")).toBeNull();
  });

  it("should return null for invalid JSON", () => {
    expect(parseStreamLine("not json")).toBeNull();
  });

  it("should parse multiple lines from stream output", () => {
    const output = [
      '{"type":"text","content":"line1"}',
      '{"type":"tool_use","tool":"Read"}',
      "",
      '{"type":"text","content":"line3"}',
    ].join("\n");

    const messages = [...parseStreamOutput(output)];
    expect(messages).toHaveLength(3);
    expect(messages[0].content).toBe("line1");
    expect(messages[1].tool).toBe("Read");
  });

  it("should extract session IDs", () => {
    const messages = [
      { type: "init", session_id: "abc-123" },
      { type: "text", content: "hello" },
    ];
    expect(extractSessionId(messages)).toBe("abc-123");
  });

  it("should return undefined when no session ID", () => {
    const messages = [{ type: "text", content: "hello" }];
    expect(extractSessionId(messages)).toBeUndefined();
  });

  it("should extract tool calls", () => {
    const messages = [
      { type: "text", content: "thinking..." },
      { type: "tool_use", tool: "Read", tool_input: { file_path: "x.ts" } },
      { type: "text", content: "done" },
    ];
    const tools = extractToolCalls(messages);
    expect(tools).toHaveLength(1);
    expect(tools[0].tool).toBe("Read");
  });

  it("should extract text content", () => {
    const messages = [
      { type: "text", content: "hello" },
      { type: "tool_use", tool: "Read" },
      { type: "text", content: "world" },
    ];
    expect(extractTextContent(messages)).toBe("hello\nworld");
  });
});
