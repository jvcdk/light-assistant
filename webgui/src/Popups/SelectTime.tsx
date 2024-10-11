import { CircularClock, ClockMode } from '../Widgets/CircularClock';
import Popup from 'reactjs-popup';
import './SelectTime.css';
import { TimeOfDay } from '../Data/ScheduleTrigger';
import { useState } from 'react';

interface Popup_SelectTimeProps {
  open: boolean;
  onClose: () => void;
  initialTime: TimeOfDay;
  onTimeChange: (time: TimeOfDay) => void;
}

function Popup_SelectTime(props: Popup_SelectTimeProps) {
  const [selectedTime, setSelectedTime] = useState(props.initialTime);

  return (
    <Popup open={props.open} onClose={() => props.onClose()} closeOnDocumentClick={false} modal>
      <div className="SelectTime">
        <CircularClock
          initialTime={props.initialTime}
          onTimeChange={setSelectedTime}
          mode={ClockMode.Hour}
        />
        <div className='Buttons'>
          <input type='button' className='Cancel' onClick={() => props.onClose()} value='Cancel' />
          <input type='button' className='Ok' onClick={() => {props.onClose(); props.onTimeChange(selectedTime)}} value='OK' />
        </div>
      </div>
    </Popup>
  );
}

export default Popup_SelectTime;