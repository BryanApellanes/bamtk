#!/usr/bin/env node

// Compiled hook entry point for PreToolUse interception.
// This is a thin wrapper that imports the compiled TypeScript hook handler.
// In production, this would be the tsc output. For development, use tsx.

import("../dist/hooks/pretooluse-hook.js").catch(() => {
  // If compiled version not available, try tsx
  import("tsx").then(() => {
    import("../src/hooks/pretooluse-hook.ts");
  }).catch(() => {
    // Fail open — allow the tool call
    process.stdout.write(JSON.stringify({ decision: "allow", reason: "hook_load_error" }) + "\n");
  });
});
