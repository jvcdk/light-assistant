import { useState } from "react";
import { ScheduleTrigger } from "./ScheduleTrigger";
import { Widget_DaySelector } from "./Widget_DaySelector";

export function Widget_ScheduleTriggerPicker() {
  const [data, setData] = useState(new ScheduleTrigger());



  return (
    <div className='Schedule'>
      <Widget_DaySelector onChange={(days) => setData({...data, Days: days})} days={data.Days} />
    </div>
  );
}
