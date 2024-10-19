import { useState } from "react";
import { ScheduleTrigger, TimeOfDay } from "../Data/ScheduleTrigger";
import { DaySelector } from "./DaySelector";
import Popup_SelectTime from '../Popups/SelectTime';
import { RenderTimeOfDay } from './RenderTimeOfDay';
import { State } from "../Utils/State";

export interface ScheduleTriggerPickerProps {
  OnChildOpenChanged: (isOpen: boolean) => void;
}
export function ScheduleTriggerPicker(props: ScheduleTriggerPickerProps) {
  const { OnChildOpenChanged } = props;
  const [data, setData] = useState(new ScheduleTrigger());
  const clockOpen = new State(useState(false));
  clockOpen.addListener(OnChildOpenChanged);

  const handleTimeChange = (time: TimeOfDay) => {
    setData({ ...data, Time: time });
  };

  return (
    <div className='Schedule'>
      <div className="Time" onClick={() => clockOpen.val = true }>
        <RenderTimeOfDay Time={data.Time} />
      </div>
      <Popup_SelectTime
        isOpen={clockOpen}
        initialTime={data.Time}
        onTimeChange={handleTimeChange}
      />
      <DaySelector onChange={(days) => setData({...data, Days: days})} days={data.Days} />
    </div>
  );
}
