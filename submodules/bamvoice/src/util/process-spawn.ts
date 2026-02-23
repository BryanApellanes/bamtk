import { spawn, ChildProcess, SpawnOptions } from "node:child_process";

export interface SpawnResult {
  stdout: string;
  stderr: string;
  exitCode: number | null;
}

export function spawnAsync(
  command: string,
  args: string[],
  options?: SpawnOptions,
): Promise<SpawnResult> {
  return new Promise((resolve, reject) => {
    const proc = spawn(command, args, {
      ...options,
      stdio: ["pipe", "pipe", "pipe"],
    });

    let stdout = "";
    let stderr = "";

    proc.stdout?.on("data", (chunk: Buffer) => {
      stdout += chunk.toString();
    });

    proc.stderr?.on("data", (chunk: Buffer) => {
      stderr += chunk.toString();
    });

    proc.on("error", reject);
    proc.on("close", (exitCode) => {
      resolve({ stdout, stderr, exitCode });
    });
  });
}

export function spawnStreaming(
  command: string,
  args: string[],
  options?: SpawnOptions,
): ChildProcess {
  return spawn(command, args, {
    ...options,
    stdio: ["pipe", "pipe", "pipe"],
  });
}
