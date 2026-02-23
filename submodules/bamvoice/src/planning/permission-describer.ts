import { ActionPlan, PlannedAction, RiskLevel } from "./action-plan.js";

export function describeActionPlan(plan: ActionPlan): string {
  const parts: string[] = [];

  parts.push("Here is what I plan to do.");
  parts.push(
    `I will take ${plan.actions.length} action${plan.actions.length === 1 ? "" : "s"} affecting ${plan.filesAffected} file${plan.filesAffected === 1 ? "" : "s"}.`,
  );

  for (const action of plan.actions) {
    parts.push(describeAction(action));
  }

  parts.push(`Overall risk level: ${describeRisk(plan.overallRisk)}.`);

  if (plan.fullyReversible) {
    parts.push("All changes are reversible.");
  } else {
    parts.push("Warning: some changes may not be easily reversible.");
  }

  if (plan.warnings.length > 0) {
    for (const warning of plan.warnings) {
      parts.push(`Warning: ${warning}`);
    }
  }

  parts.push(
    "Say 'proceed' to execute, 'repeat' to hear again, 'rephrase' to change the plan, or 'cancel' to abort.",
  );

  return parts.join(" ");
}

function describeAction(action: PlannedAction): string {
  const parts: string[] = [];
  parts.push(`Step ${action.step}: ${action.description}.`);

  if (action.type === "read" || action.type === "search") {
    parts.push("This is read-only.");
  } else {
    parts.push(`Risk: ${describeRisk(action.risk)}.`);
    if (action.reversible && action.reversalMethod) {
      parts.push(`Reversible via ${action.reversalMethod}.`);
    } else if (!action.reversible) {
      parts.push("This may not be reversible.");
    }
  }

  return parts.join(" ");
}

function describeRisk(risk: RiskLevel): string {
  switch (risk) {
    case "safe":
      return "safe";
    case "low":
      return "low";
    case "medium":
      return "medium";
    case "high":
      return "high, please review carefully";
    case "destructive":
      return "destructive, this could cause data loss";
  }
}
