import Popup from 'reactjs-popup';
import './ScheduleRuleEdit.css'
import { ScheduleTriggerPicker } from '../Widgets/ScheduleTriggerPicker';
import { useState } from 'react';

export interface ScheduleRuleEditProps {
  isOpen: boolean;
  onClose: () => void;
}
export function ScheduleRuleEdit(props: ScheduleRuleEditProps) {
  const [childIsOpen, setChildIsOpen] = useState(false);

  return (
    <Popup closeOnEscape={!childIsOpen} open={props.isOpen} onClose={props.onClose} modal closeOnDocumentClick={false}>
      <div className='ScheduleRuleEdit'>
        <ScheduleTriggerPicker OnChildOpenChanged={setChildIsOpen} />
      </div>
    </Popup>
  );
}
