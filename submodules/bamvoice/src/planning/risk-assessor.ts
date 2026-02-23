import { PlannedAction, RiskLevel, RISK_ORDER, maxRisk } from "./action-plan.js";

const TOOL_RISK_DEFAULTS: Record<string, RiskLevel> = {
  Read: "safe",
  Glob: "safe",
  Grep: "safe",
  WebSearch: "safe",
  WebFetch: "safe",
  Write: "medium",
  Edit: "low",
  Bash: "medium",
  NotebookEdit: "low",
};

const DESTRUCTIVE_PATTERNS = [
  /rm\s+-rf/i,
  /git\s+push\s+--force/i,
  /git\s+reset\s+--hard/i,
  /drop\s+table/i,
  /delete\s+from/i,
  /format\s+[a-z]:/i,
  /rmdir\s+\/s/i,
];

export function assessActionRisk(action: PlannedAction): RiskLevel {
  const baseRisk = TOOL_RISK_DEFAULTS[action.tool] ?? "medium";

  if (action.type === "delete") return maxRisk(baseRisk, "high");

  if (action.tool === "Bash" && action.toolInput.command) {
    const cmd = String(action.toolInput.command);
    for (const pattern of DESTRUCTIVE_PATTERNS) {
      if (pattern.test(cmd)) return "destructive";
    }
  }

  return baseRisk;
}

export function assessOverallRisk(actions: PlannedAction[]): RiskLevel {
  let overall: RiskLevel = "safe";
  for (const action of actions) {
    overall = maxRisk(overall, action.risk);
  }
  return overall;
}

export function isDestructive(risk: RiskLevel): boolean {
  return RISK_ORDER[risk] >= RISK_ORDER["high"];
}
