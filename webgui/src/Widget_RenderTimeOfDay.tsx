import './Widget_RenderTimeOfDay.css';
import { TimeOfDay } from "./ScheduleTrigger";
import { ClockMode } from './CircularClock';

export function Widget_RenderTimeOfDay(props: {Time: TimeOfDay, OnHourClick?: () => void, OnMinutesClick?: () => void, selected?: ClockMode}) {
  const hourStr = props.Time.Hour.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false });
  const minuteStr = props.Time.Minute.toLocaleString(undefined, { minimumIntegerDigits: 2, useGrouping: false });

  const hourSelected = props.selected == ClockMode.Hour ? ' selected' : '';
  const minuteSelected = props.selected == ClockMode.Minute ? ' selected' : '';

  return (
    <span className='Time'>
      <span className={`Hour${hourSelected}`} onClick={props.OnHourClick}>{hourStr}</span>:<span className={`Minute${minuteSelected}`} onClick={props.OnMinutesClick}>{minuteStr}</span>
    </span>
  );
}
