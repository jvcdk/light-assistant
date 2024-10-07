import { CircularClock, ClockMode } from './CircularClock';
import Popup from 'reactjs-popup';
import './Popup_CircularClock.css'; // Create this CSS file for styling
import { TimeOfDay } from './ScheduleTrigger';
import { useState } from 'react';

interface PopupCircularClockProps {
  open: boolean;
  onClose: () => void;
  initialTime: TimeOfDay;
  onTimeChange: (time: TimeOfDay) => void;
}

function Popup_CircularClock(props: PopupCircularClockProps) {
  const [selectedTime, setSelectedTime] = useState(props.initialTime);

  return (
    <Popup open={props.open} onClose={() => props.onClose()} closeOnDocumentClick={false} modal>
      <div className="PopupCircularClock">
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

export default Popup_CircularClock;