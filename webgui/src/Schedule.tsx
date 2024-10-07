import { useState } from 'react';
import './Schedule.css'
import Popup from 'reactjs-popup';
import { PopUp_ScheduleRuleEdit } from './Popup_ScheduleRuleEdit';

export function Schedule() {
  const [popupOpen, setPopupOpen] = useState(false);

  const openPopup = () => {
    setPopupOpen(true);
  }
  const closeModal = () => setPopupOpen(false);


  return (
    <div className='Schedule' onClick={openPopup}>
      hej
      <Popup open={popupOpen} onClose={closeModal} modal closeOnDocumentClick={false}>
        <PopUp_ScheduleRuleEdit /> 
      </Popup>
    </div>
  );
}
