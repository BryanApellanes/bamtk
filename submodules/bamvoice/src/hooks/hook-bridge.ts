import { createServer, Server, Socket } from "node:net";
import { ActionPlan } from "../planning/action-plan.js";
import { isDestructive } from "../planning/risk-assessor.js";
import { logger } from "../util/logger.js";

export interface HookRequest {
  tool: string;
  toolInput: Record<string, unknown>;
  requestId: string;
}

export interface HookResponse {
  decision: "allow" | "deny" | "ask";
  reason?: string;
  announce?: string;
}

export type HookHandler = (request: HookRequest) => Promise<HookResponse>;

export class HookBridge {
  private server: Server | null = null;
  private activePlan: ActionPlan | null = null;
  private currentStep = 0;
  private customHandler: HookHandler | null = null;

  setActivePlan(plan: ActionPlan): void {
    this.activePlan = plan;
    this.currentStep = 0;
  }

  clearPlan(): void {
    this.activePlan = null;
    this.currentStep = 0;
  }

  setHandler(handler: HookHandler): void {
    this.customHandler = handler;
  }

  async start(port: number, maxRetries = 5): Promise<number> {
    for (let attempt = 0; attempt <= maxRetries; attempt++) {
      const tryPort = port + attempt;
      try {
        await this.listen(tryPort);
        return tryPort;
      } catch (err) {
        if ((err as NodeJS.ErrnoException).code === "EADDRINUSE" && attempt < maxRetries) {
          logger.warn("Port %d in use, trying %d", tryPort, tryPort + 1);
          continue;
        }
        throw err;
      }
    }
    throw new Error(`All ports ${port}-${port + maxRetries} in use`);
  }

  private listen(port: number): Promise<void> {
    return new Promise((resolve, reject) => {
      this.server = createServer((socket: Socket) => {
        this.handleConnection(socket);
      });

      this.server.on("error", reject);
      this.server.listen(port, "127.0.0.1", () => {
        logger.info("Hook bridge listening on port %d", port);
        resolve();
      });
    });
  }

  async stop(): Promise<void> {
    return new Promise((resolve) => {
      if (this.server) {
        this.server.close(() => {
          this.server = null;
          resolve();
        });
      } else {
        resolve();
      }
    });
  }

  private handleConnection(socket: Socket): void {
    let buffer = "";

    socket.on("data", (chunk: Buffer) => {
      buffer += chunk.toString();

      const newlineIdx = buffer.indexOf("\n");
      if (newlineIdx >= 0) {
        const line = buffer.substring(0, newlineIdx);
        buffer = buffer.substring(newlineIdx + 1);

        this.processRequest(line)
          .then((response) => {
            socket.write(JSON.stringify(response) + "\n");
            socket.end();
          })
          .catch((err) => {
            logger.error("Hook bridge error: %s", (err as Error).message);
            const fallback: HookResponse = { decision: "allow", reason: "bridge error" };
            socket.write(JSON.stringify(fallback) + "\n");
            socket.end();
          });
      }
    });

    socket.on("error", (err) => {
      logger.error("Hook bridge socket error: %s", err.message);
    });
  }

  private async processRequest(line: string): Promise<HookResponse> {
    let request: HookRequest;
    try {
      request = JSON.parse(line) as HookRequest;
    } catch {
      return { decision: "allow", reason: "invalid request" };
    }

    if (this.customHandler) {
      return this.customHandler(request);
    }

    return this.defaultHandler(request);
  }

  private async defaultHandler(request: HookRequest): Promise<HookResponse> {
    if (!this.activePlan) {
      return { decision: "allow", reason: "no active plan" };
    }

    // Check if this tool call matches the next expected step
    const expectedAction = this.activePlan.actions[this.currentStep];
    if (expectedAction && expectedAction.tool === request.tool) {
      this.currentStep++;

      if (isDestructive(expectedAction.risk)) {
        return {
          decision: "ask",
          announce: `Step ${expectedAction.step}: ${expectedAction.description}. This is a high-risk action.`,
        };
      }

      return {
        decision: "allow",
        announce: `Step ${expectedAction.step}: ${expectedAction.description}.`,
      };
    }

    // Unplanned tool call
    return {
      decision: "ask",
      announce: `Unplanned action: ${request.tool}. This was not in the approved plan.`,
    };
  }
}
