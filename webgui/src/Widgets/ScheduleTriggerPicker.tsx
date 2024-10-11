import { useState } from "react";
import { ScheduleTrigger, TimeOfDay } from "../Data/ScheduleTrigger";
import { DaySelector } from "./DaySelector";
import Popup_SelectTime from '../Popups/SelectTime';
import { RenderTimeOfDay } from './RenderTimeOfDay';

export interface ScheduleTriggerPickerProps {
  OnChildOpenChanged: (isOpen: boolean) => void;
}
export function ScheduleTriggerPicker(props: ScheduleTriggerPickerProps) {
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
        <RenderTimeOfDay Time={data.Time} />
      </div>
      <Popup_SelectTime
        open={isClockOpen}
        onClose={() => setClockOpen(false)}
        initialTime={data.Time}
        onTimeChange={handleTimeChange}
      />
      <DaySelector onChange={(days) => setData({...data, Days: days})} days={data.Days} />
    </div>
  );
}
