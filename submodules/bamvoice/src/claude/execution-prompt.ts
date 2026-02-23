export function buildExecutionPrompt(planSummary: string): string {
  return `The user has reviewed and approved the following plan. Execute it step by step.

Plan summary: ${planSummary}

Execute each step in order. If a step fails, report the error clearly but continue with remaining steps if possible.`;
}
