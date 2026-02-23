export interface StreamMessage {
  type: string;
  subtype?: string;
  tool?: string;
  tool_input?: Record<string, unknown>;
  content?: string;
  result?: string;
  session_id?: string;
  [key: string]: unknown;
}

export function parseStreamLine(line: string): StreamMessage | null {
  const trimmed = line.trim();
  if (!trimmed) return null;

  try {
    return JSON.parse(trimmed) as StreamMessage;
  } catch {
    return null;
  }
}

export function* parseStreamOutput(output: string): Generator<StreamMessage> {
  const lines = output.split("\n");
  for (const line of lines) {
    const msg = parseStreamLine(line);
    if (msg) yield msg;
  }
}

export function extractSessionId(messages: StreamMessage[]): string | undefined {
  for (const msg of messages) {
    if (msg.session_id) return msg.session_id;
  }
  return undefined;
}

export function extractToolCalls(messages: StreamMessage[]): StreamMessage[] {
  return messages.filter(
    (m) => m.type === "tool_use" || m.subtype === "tool_use",
  );
}

export function extractTextContent(messages: StreamMessage[]): string {
  return messages
    .filter((m) => m.type === "text" || m.type === "assistant" || m.content)
    .map((m) => m.content ?? "")
    .filter(Boolean)
    .join("\n");
}
