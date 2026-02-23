export interface Transition<TState extends string, TEvent extends string> {
  from: TState;
  event: TEvent;
  to: TState;
  guard?: () => boolean;
  action?: () => void | Promise<void>;
}

export interface StateMachineConfig<TState extends string, TEvent extends string> {
  initial: TState;
  transitions: Transition<TState, TEvent>[];
  onEnter?: Partial<Record<TState, () => void | Promise<void>>>;
  onExit?: Partial<Record<TState, () => void | Promise<void>>>;
}

export type StateChangeHandler<TState extends string, TEvent extends string> = (
  from: TState,
  to: TState,
  event: TEvent,
) => void;

export class StateMachine<TState extends string, TEvent extends string> {
  private _state: TState;
  private _transitioning = false;
  private readonly transitions: Transition<TState, TEvent>[];
  private readonly onEnter: Partial<Record<TState, () => void | Promise<void>>>;
  private readonly onExit: Partial<Record<TState, () => void | Promise<void>>>;
  private readonly listeners = new Set<StateChangeHandler<TState, TEvent>>();

  constructor(config: StateMachineConfig<TState, TEvent>) {
    this._state = config.initial;
    this.transitions = config.transitions;
    this.onEnter = config.onEnter ?? {};
    this.onExit = config.onExit ?? {};
  }

  get state(): TState {
    return this._state;
  }

  get transitioning(): boolean {
    return this._transitioning;
  }

  onChange(handler: StateChangeHandler<TState, TEvent>): () => void {
    this.listeners.add(handler);
    return () => this.listeners.delete(handler);
  }

  canSend(event: TEvent): boolean {
    return this.transitions.some(
      (t) => t.from === this._state && t.event === event && (!t.guard || t.guard()),
    );
  }

  async send(event: TEvent): Promise<boolean> {
    if (this._transitioning) {
      return false;
    }

    const transition = this.transitions.find(
      (t) => t.from === this._state && t.event === event && (!t.guard || t.guard()),
    );

    if (!transition) {
      return false;
    }

    this._transitioning = true;
    const from = this._state;

    try {
      const exitHandler = this.onExit[from];
      if (exitHandler) {
        await exitHandler();
      }

      if (transition.action) {
        await transition.action();
      }

      this._state = transition.to;

      const enterHandler = this.onEnter[transition.to];
      if (enterHandler) {
        await enterHandler();
      }

      for (const listener of this.listeners) {
        listener(from, transition.to, event);
      }

      return true;
    } finally {
      this._transitioning = false;
    }
  }
}
