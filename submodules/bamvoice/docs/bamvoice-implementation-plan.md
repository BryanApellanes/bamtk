# bamvoice: Voice-Interactive Control Layer for Claude Code

## Implementation Status: Complete

Initial implementation completed 2026-02-22.

## Architecture

Voice-controlled interface for Claude Code with pluggable STT/TTS backends, a typed state machine orchestrator, and two-phase Claude CLI integration (plan then execute).

## File Structure

```
bamvoice/
  src/
    index.ts              - CLI entry point
    bamvoice.ts           - App bootstrap + DI wiring
    core/
      voice-controller.ts - State machine orchestrator
      state-machine.ts    - Generic typed FSM engine
      event-bus.ts        - Typed event emitter
    stt/
      stt-provider.ts     - STTProvider interface
      vosk-stt-provider.ts - Vosk offline provider
      sapi-stt-provider.ts - Windows SAPI fallback
      stt-factory.ts      - Factory with fallback chain
      vosk.d.ts           - Type declarations for vosk
    tts/
      tts-provider.ts     - TTSProvider interface
      sapi-tts-provider.ts - Windows SAPI via PowerShell
      tts-factory.ts      - Factory by config
    claude/
      claude-interface.ts - Wraps claude CLI (plan + execute)
      claude-stream-parser.ts - NDJSON stream parser
      planning-prompt.ts  - System prompt for plan-only mode
      execution-prompt.ts - Follow-up prompt for execution
    planning/
      action-plan.ts      - ActionPlan types + validation
      permission-describer.ts - Spoken descriptions
      risk-assessor.ts    - Risk level evaluation
    intent/
      intent-detector.ts  - Control intent detection
      intent-types.ts     - ControlIntent enum
    config/
      config-schema.ts    - BamvoiceConfig type + defaults
      config-manager.ts   - Load, validate, merge config
    hooks/
      hook-bridge.ts      - IPC server for PreToolUse hooks
      pretooluse-hook.ts  - Hook handler (separate process)
    util/
      logger.ts           - Structured logger
      process-spawn.ts    - Safe child_process helpers
  hooks/
    bamvoice-pretooluse.js - Compiled hook entry point
  test/                    - 70 tests across 10 files
```

## Key Design Decisions

- **Vosk as optional dependency**: Native ffi-napi doesn't build on Node 24; graceful fallback to SAPI
- **Typed state machine**: No path from PLANNING to EXECUTING without DESCRIBING and CONFIRMING
- **Intent detection**: Word-boundary matching prevents false positives from substring matches
- **Hook fail-open**: If IPC fails during execution, tool calls proceed (user already confirmed)
- **Graceful degradation**: Full voice -> TTS-only -> Text-only -> Abort
