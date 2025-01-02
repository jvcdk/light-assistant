import './DeviceScheduleOptions.css'
import { useEffect, useState } from "react";
import { DeviceData } from "../Data/DeviceData";
import { IDeviceConsumableAction, IDeviceScheduleEntry, PreviewMode } from "../Data/JsonTypes";
import { ParamOption } from './ParameterOption';
import { ScheduleTrigger } from '../Data/ScheduleTrigger';
import { DaySelector } from './DaySelector';
import { RenderTimeOfDay } from './RenderTimeOfDay';
import Popup_SelectTime from '../Popups/SelectTime';
import { Action } from '../Utils/Action';
import { State } from '../Utils/State';
import { GetParamDefault } from '../Utils/IParamInfoUtils';

class DeviceScheduleEntry implements IDeviceScheduleEntry {
  Key: number = 0;
  EventType: string | undefined;
  Trigger: ScheduleTrigger = new ScheduleTrigger();
  Parameters: object = {};

  constructor(source: IDeviceScheduleEntry | undefined = undefined) {
    if (source !== undefined) {
      this.EventType = source.EventType;
      this.Parameters = source.Parameters;
      this.Trigger = new ScheduleTrigger(source.Trigger);
      this.Key = source.Key;
    }
  }

  public WithEventType(eventType: string) {
    const newEntry = new DeviceScheduleEntry(this);
    newEntry.EventType = eventType;
    return newEntry;
  }

  public WithTrigger(trigger: ScheduleTrigger) {
    const newEntry = new DeviceScheduleEntry(this);
    newEntry.Trigger = trigger;
    return newEntry;
  }

  public WithParameters(parameters: Map<string, string>) {
    const newEntry = new DeviceScheduleEntry(this);
    newEntry.Parameters = Object.fromEntries(parameters);
    return newEntry;
  }

  get ParametersTyped() {
    return new Map<string, string>(Object.entries(this.Parameters));
  }

  get asRaw() {
    return {
      Key: this.Key,
      EventType: this.EventType,
      Parameters: this.Parameters,
      Trigger: this.Trigger,
    } as IDeviceScheduleEntry;
  }
}

interface ActionOptionsProps {
  info: IDeviceConsumableAction;
  values: Map<string, string>;
  onChange: (value: Map<string, string>) => void;
  onPreview: (value: string, previewMode: PreviewMode) => void;
}

function ActionOptions(props: ActionOptionsProps) {
  const params = props.info.Parameters;
  const values = props.values;

  function onChange(name: string, value: string | undefined) {
    const newValues = new Map(values);
    if (value)
      newValues.set(name, value);
    else
      newValues.delete(name);
    props.onChange(newValues)
  }

  function OnPreview(value: string, previewMode: PreviewMode) {
    if (previewMode == 'None')
      return;

    if (value == 'None')
      props.onPreview('', 'None');
    else
      props.onPreview(value, previewMode);
  }

  return (
    <div className='ActionOptions'>
      {params.map(param => <ParamOption
        key={param.Name}
        param={param}
        value={values.get(param.Name)}
        onPreview={(value) => OnPreview(value, param.PreviewMode)}
        onChange={val => onChange(param.Name, val)} />)}
    </div>
  );
}

function GetTrigger(consumableTriggers: IDeviceConsumableAction[], eventType: string | undefined) {
  if (eventType === undefined)
    return undefined;
  return consumableTriggers.find(el => el.EventType == eventType);
}

interface IScheduleEntryProps {
  entry: DeviceScheduleEntry;
  consumableTriggers: IDeviceConsumableAction[];
  onEntryChanged: (entry: DeviceScheduleEntry) => void;
  onTriggerChanged: (trigger: ScheduleTrigger) => void;
  onChildOpenChanged: Action<boolean>;
  onActionOptionsPreview: (value: string, previewMode: PreviewMode) => void;
}

function ScheduleEntry(props: IScheduleEntryProps) {
  const { entry, consumableTriggers, onChildOpenChanged, onEntryChanged, onTriggerChanged } = props;
  const clockOpen = new State(useState(false));
  clockOpen.addListener("ScheduleEntry", onChildOpenChanged);
  const [triggerInfo, setTriggerInfo] = useState<IDeviceConsumableAction | undefined>(GetTrigger(consumableTriggers, entry.EventType));

  function SetEventType(eventType: string) {
    const newEntry = entry.WithEventType(eventType);
    onEntryChanged(newEntry);
  }

  useEffect(() => {
    const ConsumableActions = GetTrigger(consumableTriggers, entry.EventType);
    setTriggerInfo(ConsumableActions);

    if (ConsumableActions === undefined)
      return;

    const newParams = new Map<string, string>();
    ConsumableActions.Parameters.forEach(param => {
      const value = entry.ParametersTyped.get(param.Name);
      if (value !== undefined)
        newParams.set(param.Name, value);
      else
        newParams.set(param.Name, GetParamDefault(param));
    });

    const paramsChanged = Array.from(newParams).some(([key, value]) => entry.ParametersTyped.get(key) !== value);
    if (paramsChanged)
      onEntryChanged(entry.WithParameters(newParams));
  }, [consumableTriggers, entry, entry.EventType, entry.ParametersTyped, onEntryChanged]);

  const trigger = entry.Trigger;
  return (
    <div key={entry.Key} className='ScheduleEntry FlexHori'>
      <div>
        <select className='TriggerDeviceAction' value={entry.EventType} onChange={(e) => SetEventType(e.target.value)}>
          <option value="">&lt;Please select&gt;</option>
          {consumableTriggers.map((action) => <option key={action.EventType} value={action.EventType}>{action.EventType}</option>)}
        </select>
        {triggerInfo && <ActionOptions onPreview={props.onActionOptionsPreview} info={triggerInfo} values={entry.ParametersTyped} onChange={val => onEntryChanged(entry.WithParameters(val))} />}
      </div>
      <div className='Spacer'></div>
      {triggerInfo && <DaySelector onChange={(days) => onTriggerChanged(trigger.WithDays(days))} days={trigger.DayNames} />}
      {triggerInfo && <RenderTimeOfDay Time={trigger.Time} OnTimeClick={() => clockOpen.val = true} />}
      <Popup_SelectTime
        isOpen={clockOpen}
        initialTime={trigger.Time}
        onTimeChange={(time) => onTriggerChanged(trigger.WithTime(time))}
      />
    </div>
  )
}

function ScheduleIsActive(schedule: DeviceScheduleEntry, consumableTriggers: IDeviceConsumableAction[]) {
  return consumableTriggers.find(trigger => trigger.EventType == schedule.EventType) !== undefined;
}

function FindFirstAvailableKey(scheduleWithKey: DeviceScheduleEntry[]): number {
  let key = 1;
  while (scheduleWithKey.find(el => el.Key == key))
    key++;
  return key;
}


export interface DeviceScheduleOptionsProps {
  devData: DeviceData;
  setSchedule: (schedule: IDeviceScheduleEntry[]) => void;
  onChildOpenChanged: Action<boolean>;
  onActionOptionsPreview: (value: string, previewMode: PreviewMode) => void;
}

let newDeviceScheduleEntry = new DeviceScheduleEntry();

export function DeviceScheduleOptions(prop: DeviceScheduleOptionsProps) {
  const { devData, setSchedule, onChildOpenChanged } = prop;
  const consumableTriggers = devData.ConsumableActions;

  const scheduleWithKey = devData.Schedule.map(schedule => {
    if (schedule instanceof DeviceScheduleEntry)
      return schedule;
    return new DeviceScheduleEntry(schedule);
  });
  scheduleWithKey.push(newDeviceScheduleEntry);

  function updateSchedule(entry: DeviceScheduleEntry) {
    const idx = scheduleWithKey.findIndex(el => el.Key == entry.Key);
    if (idx < 0)
      return;

    if (idx == scheduleWithKey.length - 1)
      newDeviceScheduleEntry = new DeviceScheduleEntry();

    if (entry.Key < 1)
      entry.Key = FindFirstAvailableKey(scheduleWithKey);

    scheduleWithKey[idx] = entry;
    const result = scheduleWithKey
      .filter(entry => ScheduleIsActive(entry, consumableTriggers))
      .map(entry => entry.asRaw);
    setSchedule(result);
  }

  if (consumableTriggers.length == 0)
    return (<div>Device does not provide schedulable functions.</div>);

  return (
    <div className='ScheduleOptions'>
      {scheduleWithKey.map((entry) => <ScheduleEntry
        onActionOptionsPreview={prop.onActionOptionsPreview}
        onEntryChanged={updateSchedule}
        onTriggerChanged={(trigger) => updateSchedule(entry.WithTrigger(trigger))}
        key={entry.Key} entry={entry}
        consumableTriggers={consumableTriggers} onChildOpenChanged={onChildOpenChanged} />)}
    </div>
  );
}

