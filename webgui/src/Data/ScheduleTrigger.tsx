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

// eslint-disable-next-line react-refresh/only-export-components
const DayNames = Object.values(DayName);
const dayNameIndices: Map<DayName, number> = new Map();
DayNames.forEach((day, index) => dayNameIndices.set(day, index));

function getDayIdx(key: DayName | string) {
  const result = dayNameIndices.get(key as DayName);
  if(result === undefined)
    return -1;
  return result;
}

export class TimeOfDay implements ITimeOfDay {
  Hour: number;
  Minute: number;

  constructor(hour: number, minute: number) {
    this.Hour = hour;
    this.Minute = minute;
  }

  Clone() {
    return new TimeOfDay(this.Hour, this.Minute);
  }

  WithHour(hour: number) {
    return new TimeOfDay(hour, this.Minute);
  }

  WithMinutes(minute: number) {
    return new TimeOfDay(this.Hour, minute);
  }

  static fromString(simeStr : string) {
    const parts = simeStr.split(':');
    if(parts.length !== 2)
      throw new Error('Invalid time string format');
    const hour = parseInt(parts[0]);
    const minute = parseInt(parts[1]);
    return new TimeOfDay(hour, minute);
  }

  get hourStr() { return this.Hour.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false }); }
  get minuteStr() { return this.Minute.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false }); }

  get asString() { return `${this.hourStr}:${this.minuteStr}`; }
  get asRaw() { return { Hour: this.Hour, Minute: this.Minute } as ITimeOfDay; }
}


export class ScheduleTrigger implements IScheduleTrigger {
  Days: number[] = [];
  Time: TimeOfDay = new TimeOfDay(0, 0);

  constructor(source: IScheduleTrigger | undefined = undefined) {
    if(source !== undefined) {
      this.Days = source.Days;
      this.Time = new TimeOfDay(source.Time.Hour, source.Time.Minute);
    }
  }

  get DayNames() : DayName[] { return this.Days.map(day => DayNames[day] || "<unknown>"); }

  public WithDays(days: DayName[]) {
    const newTrigger = new ScheduleTrigger(this);
    newTrigger.Days = days.map(day => { return getDayIdx(day); });
    return newTrigger;
  }

  public WithTime(time: TimeOfDay) {
    const newTrigger = new ScheduleTrigger(this);
    newTrigger.Time = time;
    return newTrigger;
  }
}
