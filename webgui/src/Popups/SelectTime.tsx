import { CircularClock, ClockMode } from '../Widgets/CircularClock';
import Popup from 'reactjs-popup';
import './SelectTime.css';
import { TimeOfDay } from '../Data/ScheduleTrigger';
import { useState } from 'react';
import { State } from "../Utils/State";

interface Popup_SelectTimeProps {
  isOpen: State<boolean>;
  initialTime: TimeOfDay;
  onTimeChange: (time: TimeOfDay) => void;
}

function Popup_SelectTime(props: Popup_SelectTimeProps) {
  const { isOpen, initialTime, onTimeChange } = props;
  const [selectedTime, setSelectedTime] = useState(initialTime);

  return (
    <Popup open={isOpen.val} onClose={() => isOpen.val = false} closeOnDocumentClick={false} modal>
      <div className="SelectTime">
        <CircularClock
          initialTime={initialTime}
          onTimeChange={setSelectedTime}
          mode={ClockMode.Hour}
        />
        <div className='Buttons'>
          <input type='button' className='Cancel' onClick={() => isOpen.val = false} value='Cancel' />
          <input type='button' className='Ok' onClick={() => {isOpen.val = false; onTimeChange(selectedTime)}} value='OK' />
        </div>
      </div>
    </Popup>
  );
}

export default Popup_SelectTime;
