export type RiskLevel = "safe" | "low" | "medium" | "high" | "destructive";

export interface PlannedAction {
  step: number;
  type: "read" | "write" | "edit" | "shell" | "network" | "search" | "delete";
  description: string;
  tool: string;
  toolInput: Record<string, unknown>;
  affectedResources: string[];
  risk: RiskLevel;
  reversible: boolean;
  reversalMethod?: string;
}

export interface ActionPlan {
  planId: string;
  summary: string;
  originalPrompt: string;
  actions: PlannedAction[];
  overallRisk: RiskLevel;
  filesAffected: number;
  affectedFilePaths: string[];
  fullyReversible: boolean;
  warnings: string[];
}

export function isActionPlan(value: unknown): value is ActionPlan {
  if (typeof value !== "object" || value === null) return false;
  const obj = value as Record<string, unknown>;
  return (
    typeof obj.planId === "string" &&
    typeof obj.summary === "string" &&
    typeof obj.originalPrompt === "string" &&
    Array.isArray(obj.actions) &&
    typeof obj.overallRisk === "string" &&
    typeof obj.filesAffected === "number" &&
    Array.isArray(obj.affectedFilePaths) &&
    typeof obj.fullyReversible === "boolean" &&
    Array.isArray(obj.warnings)
  );
}

export const RISK_ORDER: Record<RiskLevel, number> = {
  safe: 0,
  low: 1,
  medium: 2,
  high: 3,
  destructive: 4,
};

export function maxRisk(a: RiskLevel, b: RiskLevel): RiskLevel {
  return RISK_ORDER[a] >= RISK_ORDER[b] ? a : b;
}
