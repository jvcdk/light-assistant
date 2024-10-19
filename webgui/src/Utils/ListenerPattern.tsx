import { Action } from "./Action";

export interface IListener<T> {
  readonly listeners: Map<string, Action<T>>;
}

export class ListenerData<T> implements IListener<T> {
  readonly listeners: Map<string, Action<T>> = new Map();
}

export class Listener<T, U extends IListener<T>> {
  protected readonly _data: U;

  protected constructor(data: U)  {
    this._data = data;
  }

  addListener(name: string, listener: Action<T>) {
    this._data.listeners.set(name, listener);
  }

  protected notifyListeners() {
    const copy = Object.create(this) as T;
    this._data.listeners
      .forEach(listener => listener(copy));
  }
}
