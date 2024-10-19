import './RenderTimeOfDay.css';
import { TimeOfDay } from "../Data/ScheduleTrigger";
import { ClockMode } from './CircularClock';

interface RenderTimeOfDayProps {
  Time: TimeOfDay;
  OnTimeClick?: () => void;
  OnHourClick?: () => void;
  OnMinutesClick?: () => void;
  selected?: ClockMode;
}

export function RenderTimeOfDay(props: RenderTimeOfDayProps) {
  const { Time, OnTimeClick, OnHourClick, OnMinutesClick, selected } = props;
  const hourStr = Time.hour.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false });
  const minuteStr = Time.minute.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false });

  const hourSelected = selected == ClockMode.Hour ? ' selected' : '';
  const minuteSelected = selected == ClockMode.Minute ? ' selected' : '';
  const timeClass = OnTimeClick ? ' clickable' : '';

  return (
    <span className={'Time' + timeClass} onClick={OnTimeClick}>
      <span className={`Hour${hourSelected}`} onClick={OnHourClick}>{hourStr}</span>:<span className={`Minute${minuteSelected}`} onClick={OnMinutesClick}>{minuteStr}</span>
    </span>
  );
}
