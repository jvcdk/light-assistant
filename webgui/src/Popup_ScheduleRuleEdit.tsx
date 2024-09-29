import './Popup_ScheduleRuleEdit.css'
import { ScheduleTriggerPicker } from './ScheduleTriggerPicker';

export function PopUp_ScheduleRuleEdit(prop: {str: string}) {
  return (
    <div className='Popup_ScheduleRuleEdit'>
      {prop.str}
      <ScheduleTriggerPicker />
    </div>
  );
}
