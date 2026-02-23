import { ControlIntent, IntentResult } from "./intent-types.js";
import { ControlConfig } from "../config/config-schema.js";

export class IntentDetector {
  private readonly phrases: Record<ControlIntent, string[]>;
  private readonly minConfidence: number;

  constructor(config: ControlConfig) {
    this.phrases = {
      [ControlIntent.Execute]: config.intentPhrases.execute,
      [ControlIntent.Repeat]: config.intentPhrases.repeat,
      [ControlIntent.Rephrase]: config.intentPhrases.rephrase,
      [ControlIntent.Cancel]: config.intentPhrases.cancel,
      [ControlIntent.Unknown]: [],
    };
    this.minConfidence = config.minIntentConfidence;
  }

  detect(rawText: string): IntentResult {
    const normalized = rawText.toLowerCase().trim();
    if (!normalized) {
      return { intent: ControlIntent.Unknown, confidence: 0, rawText };
    }

    let bestIntent = ControlIntent.Unknown;
    let bestScore = 0;

    for (const [intent, phrases] of Object.entries(this.phrases)) {
      if (intent === ControlIntent.Unknown) continue;

      for (const phrase of phrases) {
        const score = this.matchScore(normalized, phrase.toLowerCase());
        if (score > bestScore) {
          bestScore = score;
          bestIntent = intent as ControlIntent;
        }
      }
    }

    if (bestScore < this.minConfidence) {
      return { intent: ControlIntent.Unknown, confidence: bestScore, rawText };
    }

    return { intent: bestIntent, confidence: bestScore, rawText };
  }

  getAllKeywords(): string[] {
    const keywords: string[] = [];
    for (const [intent, phrases] of Object.entries(this.phrases)) {
      if (intent === ControlIntent.Unknown) continue;
      keywords.push(...phrases);
    }
    return keywords;
  }

  private matchScore(input: string, phrase: string): number {
    if (input === phrase) return 1.0;

    // Check for whole-word boundary match
    const wordBoundary = new RegExp(`\\b${this.escapeRegex(phrase)}\\b`);
    if (wordBoundary.test(input)) return 0.9;

    const inputWords = input.split(/\s+/);
    const phraseWords = phrase.split(/\s+/);

    // For single-word phrases, only match exact words
    if (phraseWords.length === 1) {
      if (inputWords.includes(phrase)) return 0.9;
      return 0;
    }

    // For multi-word phrases, check word overlap
    const matchedWords = phraseWords.filter((pw) =>
      inputWords.some((iw) => iw === pw),
    );
    const ratio = matchedWords.length / phraseWords.length;
    if (ratio >= 0.5) return ratio * 0.85;

    return 0;
  }

  private escapeRegex(str: string): string {
    return str.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  }
}
