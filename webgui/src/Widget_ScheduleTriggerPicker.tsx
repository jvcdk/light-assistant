import { useState } from "react";
import { ScheduleTrigger, TimeOfDay } from "./ScheduleTrigger";
import { Widget_DaySelector } from "./Widget_DaySelector";
import Popup_CircularClock from './Popup_CircularClock';
import { Widget_RenderTimeOfDay } from './Widget_RenderTimeOfDay';

export function Widget_ScheduleTriggerPicker(props: {OnChildOpenChanged: (isOpen: boolean) => void}) {
  const [data, setData] = useState(new ScheduleTrigger());
  const [isClockOpen, _setClockOpen] = useState(false);
  function setClockOpen(open: boolean) {
    _setClockOpen(open);
    props.OnChildOpenChanged(open);
  }

  const handleTimeChange = (time: TimeOfDay) => {
    setData({ ...data, Time: time });
  };

  return (
    <div className='Schedule'>
      <div className="Time" onClick={() => setClockOpen(true)}>
        <Widget_RenderTimeOfDay Time={data.Time} />
      </div>
      <Popup_CircularClock
        open={isClockOpen}
        onClose={() => setClockOpen(false)}
        initialTime={data.Time}
        onTimeChange={handleTimeChange}
      />
      <Widget_DaySelector onChange={(days) => setData({...data, Days: days})} days={data.Days} />
    </div>
  );
}
