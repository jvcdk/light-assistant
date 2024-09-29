import { useState } from "react";
import { ScheduleTrigger } from "./ScheduleTrigger";

export function ScheduleTriggerPicker() {
  const [data, setData] = useState(new ScheduleTrigger());



  return (
    <div className='Schedule'>
      {data.Enabled ? 'Enabled' : 'Disabled'}
    </div>
  );
}
