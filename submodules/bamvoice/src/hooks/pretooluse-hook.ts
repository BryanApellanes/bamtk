import { createConnection } from "node:net";

/**
 * PreToolUse hook entry point.
 * This runs as a separate process invoked by Claude Code's hook system.
 * It connects to the bamvoice HookBridge IPC server to check tool calls
 * against the approved plan.
 *
 * Input: JSON on stdin with tool call details
 * Output: JSON on stdout with decision (allow/deny/ask)
 */

interface HookInput {
  tool_name: string;
  tool_input: Record<string, unknown>;
}

async function main(): Promise<void> {
  const ipcPort = parseInt(process.env.BAMVOICE_IPC_PORT ?? "9847", 10);

  // Read stdin
  const input = await readStdin();
  let hookInput: HookInput;
  try {
    hookInput = JSON.parse(input) as HookInput;
  } catch {
    // Can't parse input, allow by default
    writeOutput({ decision: "allow" });
    return;
  }

  try {
    const response = await queryBridge(ipcPort, {
      tool: hookInput.tool_name,
      toolInput: hookInput.tool_input,
      requestId: `hook-${Date.now()}`,
    });

    writeOutput(response);
  } catch {
    // IPC failed — fail open (user already confirmed the plan)
    writeOutput({ decision: "allow", reason: "ipc_error" });
  }
}

function readStdin(): Promise<string> {
  return new Promise((resolve) => {
    let data = "";
    process.stdin.setEncoding("utf-8");
    process.stdin.on("data", (chunk) => {
      data += chunk;
    });
    process.stdin.on("end", () => resolve(data));
    // Timeout after 5 seconds
    setTimeout(() => resolve(data), 5000);
  });
}

function queryBridge(
  port: number,
  request: { tool: string; toolInput: Record<string, unknown>; requestId: string },
): Promise<Record<string, unknown>> {
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
        resolve(JSON.parse(buffer.trim()));
      } catch {
        resolve({ decision: "allow", reason: "parse_error" });
      }
    });

    socket.on("error", reject);

    setTimeout(() => {
      socket.destroy();
      reject(new Error("timeout"));
    }, 3000);
  });
}

function writeOutput(data: Record<string, unknown>): void {
  process.stdout.write(JSON.stringify(data) + "\n");
}

main().catch(() => {
  writeOutput({ decision: "allow", reason: "uncaught_error" });
});
