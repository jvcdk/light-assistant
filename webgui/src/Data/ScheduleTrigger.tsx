export enum DayName {
  Monday = "Mo",
  Tuesday = "Tu",
  Wednesday = "We",
  Thursday = "Th",
  Friday = "Fr",
  Saturday = "Sa",
  Sunday = "Su"
}

export class TimeOfDay {
  Hour: number = 0;
  Minute: number = 0;

  constructor(hour: number, minute: number) {
    this.Hour = hour;
    this.Minute = minute;
  }

  static fromString(simeStr : string) {
    const parts = simeStr.split(':');
    if(parts.length !== 2)
      throw new Error('Invalid time string format');
    const hour = parseInt(parts[0]);
    const minute = parseInt(parts[1]);
    return new TimeOfDay(hour, minute);
  }
}

export class ScheduleTrigger {
  Days: DayName[] = [];
  Time: TimeOfDay = new TimeOfDay(0, 0);
}
