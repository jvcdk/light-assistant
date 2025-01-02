import { Dispatch, SetStateAction } from "react";
import { Action } from "./Action";

export class State<S> {
  private _value: S;
  private _setValue: (value: S) => void;

  constructor(init: [S, Dispatch<SetStateAction<S>>]) {
    this._value = init[0];
    this._setValue = init[1];
  }

  get val() { return this._value; }
  set val(value: S) {
    this._setValue(value);
    this._listeners.forEach(listener => listener(value));
  }

  private _listeners: Map<string, Action<S>> = new Map();
  addListener(name: string, listener: Action<S>) {
    this._listeners.set(name, listener);
  }
}
