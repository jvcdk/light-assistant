import { Listener, ListenerData } from "../Utils/ListenerPattern";
import { IScheduleTrigger, ITimeOfDay } from "./JsonTypes";

export enum DayName {
  Monday = "Mon",
  Tuesday = "Tue",
  Wednesday = "Wed",
  Thursday = "Thu",
  Friday = "Fri",
  Saturday = "Sat",
  Sunday = "Sun",
}

const DayNames = Object.values(DayName);
const dayNameIndices: Map<DayName, number> = new Map();
DayNames.forEach((day, index) => dayNameIndices.set(day, index));

function getDayIdx(key: DayName | string) {
  const result = dayNameIndices.get(key as DayName);
  if(result === undefined)
    return -1;
  return result;
}

class TimeOfDayData extends ListenerData<ITimeOfDay> {
  constructor(public hour: number = 0, public minute: number = 0) {
    super();
  }
}

export class TimeOfDay extends Listener<ITimeOfDay, TimeOfDayData> implements ITimeOfDay {
  constructor(hour: number, minute: number) {
    super(new TimeOfDayData(hour, minute));
  }

  static fromString(simeStr : string) {
    const parts = simeStr.split(':');
    if(parts.length !== 2)
      throw new Error('Invalid time string format');
    const hour = parseInt(parts[0]);
    const minute = parseInt(parts[1]);
    return new TimeOfDay(hour, minute);
  }

  get Hour() { return this._data.hour; }
  set Hour(value: number) { this._data.hour = value; this.notifyListeners(); }
  get hourStr() { return this.Hour.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false }); }

  get Minute() { return this._data.minute; }
  set Minute(value: number) { this._data.minute = value; this.notifyListeners(); }
  get minuteStr() { return this.Minute.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false }); }

  get asString() { return `${this.hourStr}:${this.minuteStr}`; }
  get asRaw() { return { Hour: this.Hour, Minute: this.Minute } as ITimeOfDay; }
}

class ScheduleTriggerData extends ListenerData<IScheduleTrigger> {
  days: number[] = [];
  time: TimeOfDay = new TimeOfDay(0, 0);
}

export class ScheduleTrigger extends Listener<IScheduleTrigger, ScheduleTriggerData> implements IScheduleTrigger {
  constructor(source: IScheduleTrigger | undefined = undefined) {
    super(new ScheduleTriggerData());
    if(source !== undefined) {
      this.Days = source.Days;
      this.Time = new TimeOfDay(source.Time.Hour, source.Time.Minute);
    }
  }

  get Days() { return this._data.days; }
  set Days(value: number[]) { this._data.days = value; this.notifyListeners(); }

  get DayNames() : DayName[] { return this.Days.map(day => DayNames[day] || "<unknown>"); }
  set DayNames(value: string[] | DayName[]) {
    this.Days = value.map(day => { return getDayIdx(day); });
  }

  get Time() { return this._data.time; }
  set Time(value: TimeOfDay) { this._data.time = value; this.notifyListeners(); }

  get asRaw() {
    return {
      Days: this.Days,
      Time: this.Time.asRaw,
    } as IScheduleTrigger;
  }
}
