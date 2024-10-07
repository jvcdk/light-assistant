import './Popup_ScheduleRuleEdit.css'
import { Widget_ScheduleTriggerPicker } from './Widget_ScheduleTriggerPicker';

export function PopUp_ScheduleRuleEdit(props: {OnChildOpenChanged: (isOpen: boolean) => void}) {
  return (
    <div className='Popup_ScheduleRuleEdit'>
      <Widget_ScheduleTriggerPicker OnChildOpenChanged={props.OnChildOpenChanged} />
    </div>
  );
}
