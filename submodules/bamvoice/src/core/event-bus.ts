type Handler<T> = (data: T) => void;

export class EventBus<TEvents extends Record<string, any>> {
  private handlers = new Map<keyof TEvents, Set<Handler<unknown>>>();

  on<K extends keyof TEvents>(event: K, handler: Handler<TEvents[K]>): () => void {
    if (!this.handlers.has(event)) {
      this.handlers.set(event, new Set());
    }
    const set = this.handlers.get(event)!;
    set.add(handler as Handler<unknown>);
    return () => set.delete(handler as Handler<unknown>);
  }

  once<K extends keyof TEvents>(event: K, handler: Handler<TEvents[K]>): () => void {
    const unsub = this.on(event, (data) => {
      unsub();
      handler(data);
    });
    return unsub;
  }

  emit<K extends keyof TEvents>(event: K, data: TEvents[K]): void {
    const set = this.handlers.get(event);
    if (set) {
      for (const handler of set) {
        handler(data);
      }
    }
  }

  removeAllListeners(event?: keyof TEvents): void {
    if (event) {
      this.handlers.delete(event);
    } else {
      this.handlers.clear();
    }
  }
}
