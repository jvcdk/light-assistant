import { useState } from 'react';
import './Schedule.css'
import { ScheduleRuleEdit } from '../Popups/ScheduleRuleEdit';

export function Schedule() {
  const [popupOpen, setPopupOpen] = useState(false);

  const OpenPopup = () => setPopupOpen(true);
  const ClosePopup = () => setPopupOpen(false);

  return (
    <div className='Schedule' onClick={OpenPopup}>
      No schedule rules defined.
      <ScheduleRuleEdit isOpen={popupOpen} onClose={ClosePopup} /> 
    </div>
  );
}
