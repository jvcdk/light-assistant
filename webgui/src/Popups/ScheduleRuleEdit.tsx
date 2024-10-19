import Popup from 'reactjs-popup';
import './ScheduleRuleEdit.css'
import { ScheduleTriggerPicker } from '../Widgets/ScheduleTriggerPicker';
import { useState } from 'react';
import { DeviceData2 } from '../Data/DeviceData';
import { RuleActionsEdit, TriggerAction } from '../Widgets/RuleActionsEdit';
import { ScheduleTrigger } from '../Data/ScheduleTrigger';

export class ScheduleRule {
  Enabled: boolean = false;
  DeviceAddress : string = '';
  Trigger: ScheduleTrigger = new ScheduleTrigger();
  Action: TriggerAction = new TriggerAction();
}

export interface ScheduleRuleEditProps {
  isOpen: boolean;
  onClose: () => void;
  rule: ScheduleRule;
  devData: DeviceData2[];
}

export function ScheduleRuleEdit(props: ScheduleRuleEditProps) {
  const [childIsOpen, setChildIsOpen] = useState(false);
  const [rule, setRule] = useState(props.rule);

  function onDeviceChange(value: string) {
    setRule({
      ...rule,
      DeviceAddress: value,
    });
  }

  function onActionChange(value: TriggerAction) {
    setRule({
      ...rule,
      Action: value,
    });
  }

  return (
    <Popup closeOnEscape={!childIsOpen} open={props.isOpen} onClose={props.onClose} modal closeOnDocumentClick={false}>
      <div className='ScheduleRuleEdit'>
        <ScheduleTriggerPicker OnChildOpenChanged={setChildIsOpen} />
        <RuleActionsEdit selectedDevice={rule.DeviceAddress} devData={props.devData} action={rule.Action} onActionChange={onActionChange} onDeviceChange={onDeviceChange} />
      </div>
    </Popup>
  );
}
