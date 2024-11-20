import './DeviceScheduleOptions.css'
import { useEffect, useReducer, useState } from "react";
import { DeviceData } from "../Data/DeviceData";
import { IDeviceConsumableTrigger, IDeviceScheduleEntry } from "../Data/JsonTypes";
import { ParamOption } from './ParameterOption';
import { ScheduleTrigger } from '../Data/ScheduleTrigger';
import { DaySelector } from './DaySelector';
import { RenderTimeOfDay } from './RenderTimeOfDay';
import { State } from "../Utils/State";
import Popup_SelectTime from '../Popups/SelectTime';
import { Action } from '../Utils/Action';
import { Listener, ListenerData } from '../Utils/ListenerPattern';

class DeviceScheduleEntryWithKeyData extends ListenerData<IDeviceScheduleEntry> {
  private static _key: number = 1;
  key: number = DeviceScheduleEntryWithKeyData._key++;

  EventType: string | undefined;
  Parameters: Map<string, string> = new Map();
  Trigger: ScheduleTrigger = new ScheduleTrigger();
}

class DeviceScheduleEntryWithKey extends Listener<IDeviceScheduleEntry, DeviceScheduleEntryWithKeyData> implements IDeviceScheduleEntry {
  constructor(source: IDeviceScheduleEntry | undefined = undefined) {
    const data = new DeviceScheduleEntryWithKeyData();
    super(data);
    if(source !== undefined) {
      this.EventType = source.EventType;
      this.Parameters = source.Parameters;
      this.Trigger = new ScheduleTrigger(source.Trigger);

      if(source instanceof DeviceScheduleEntryWithKey)
        data.key = source.key;
    }
  }

  get EventType () { return this._data.EventType; }
  set EventType (value: string | undefined) { this._data.EventType = value; this.notifyListeners(); }

  get Parameters () { return this._data.Parameters; }
  set Parameters (value: Map<string, string>) { this._data.Parameters = value; this.notifyListeners(); }

  get Trigger () { return this._data.Trigger; }
  set Trigger (value: ScheduleTrigger) {
    this._data.Trigger = value;
    value.addListener("DeviceScheduleEntryWithKey", (val) => {
      if(val instanceof ScheduleTrigger)
        this.Trigger = val;
      else
        this.Trigger = new ScheduleTrigger(val);
    });
    this.notifyListeners();
  }

  get key () { return this._data.key; }
}

interface ActionOptionsProps {
  info: IDeviceConsumableTrigger;
  values: Map<string, string>;
  onChange: (value: Map<string, string>) => void;
}

function ActionOptions(props: ActionOptionsProps) {
  const params = props.info.Parameters;
  const values = props.values;

  function onChange(name: string, value: string | undefined) {
    const newValues = new Map(values);
    if(value)
      newValues.set(name, value);
    else
      newValues.delete(name);
    props.onChange(newValues);
  }

  return (
    <div>
      {params.map(param => <ParamOption
          key={param.Name}
          param={param}
          value={values.get(param.Name)}
          onChange={val => onChange(param.Name, val)} />)}
    </div>
  );
}

interface IScheduleEntryProps {
  entry: DeviceScheduleEntryWithKey;
  consumableTriggers: IDeviceConsumableTrigger[];
  onChildOpenChanged: Action<boolean>;
}

function ScheduleEntry(props: IScheduleEntryProps) {
  const { entry, consumableTriggers, onChildOpenChanged } = props;
  const clockOpen = new State(useState(false));
  clockOpen.addListener("ScheduleEntry", onChildOpenChanged);
  const [, forceUpdate] = useReducer(x => x + 1, 0);

  useEffect(() => {
    entry.Trigger.addListener("ScheduleEntry", forceUpdate);
  }, [entry.Trigger]);

  const triggerInfo = entry.EventType ? consumableTriggers.find(el => el.EventType == entry.EventType) : undefined;
  const trigger = entry.Trigger;
  return(
    <div key={entry.key} className='ScheduleEntry FlexHori'>
      <div>
        <select className='TriggerDeviceAction' value={entry.EventType} onChange={(e) => entry.EventType = e.target.value}>
          <option value="">&lt;Please select&gt;</option>
          {consumableTriggers.map((action) => <option key={action.EventType} value={action.EventType}>{action.EventType}</option>)}
        </select>
        {triggerInfo && <ActionOptions info={triggerInfo} values={entry.Parameters} onChange={val => entry.Parameters = val} />}
      </div>
      {triggerInfo && <DaySelector onChange={(days) => trigger.DayNames = days} days={trigger.DayNames} />}
      {triggerInfo && <RenderTimeOfDay Time={trigger.Time} OnTimeClick={() => clockOpen.val = true} /> }
      <Popup_SelectTime
        isOpen={clockOpen}
        initialTime={trigger.Time}
        onTimeChange={(time) => trigger.Time = time}
      />
    </div>
  )
}

function ScheduleIsValid(schedule: DeviceScheduleEntryWithKey, eligibleEvTypes: string[]) {
  return eligibleEvTypes.find(evType => evType == schedule.EventType) !== undefined;
}

export interface DeviceScheduleOptionsProps {
  devData: DeviceData;
  setSchedule: (schedule: IDeviceScheduleEntry[]) => void;
  onChildOpenChanged: Action<boolean>;
}

export function DeviceScheduleOptions(prop: DeviceScheduleOptionsProps) {
  const { devData, setSchedule, onChildOpenChanged  } = prop;
  const consumableTriggers = devData.ConsumableTriggers;
  const eligibleEvTypes = consumableTriggers.map(el => el.EventType);

  const scheduleWithKey = devData.Schedule.map(schedule => new DeviceScheduleEntryWithKey(schedule));
  scheduleWithKey.push(new DeviceScheduleEntryWithKey());
  const [localSchedule, _setLocalSchedule] = useState<DeviceScheduleEntryWithKey[]>(scheduleWithKey);

  const setLocalSchedule = (entry: DeviceScheduleEntryWithKey) => {
    const entryIsValid = ScheduleIsValid(entry, eligibleEvTypes);

    let result: DeviceScheduleEntryWithKey[] = [];
    _setLocalSchedule(prev => {
      result = prev.slice();
      const idx = result.findIndex(el => el.key == entry.key);
      if(idx >= 0) {
        if(entryIsValid)
          result[idx] = entry;
        else
          result.splice(idx, 1);
      }
      const lastElementIsValid = result.length > 0 && ScheduleIsValid(result[result.length - 1], eligibleEvTypes);
      if(lastElementIsValid) {
        const emptyEntry = new DeviceScheduleEntryWithKey();
        emptyEntry.addListener("DeviceScheduleOptions", (val) => setLocalSchedule(val as DeviceScheduleEntryWithKey));
        result.push(emptyEntry);
      }
      return result;
    });
    // Remove last element (should not go up to parent scope)
    result = result.slice(0, result.length - 1);
    setSchedule(result);
  }
  scheduleWithKey.forEach((el) => el.addListener("DeviceScheduleOptions", (val) => setLocalSchedule(val as DeviceScheduleEntryWithKey)));

  if (consumableTriggers.length == 0)
    return (<div>Device does not provide schedulable functions.</div>);

  return (
    <div className='ScheduleOptions'>
      {localSchedule.map((entry) => <ScheduleEntry key={entry.key} entry={entry}
        consumableTriggers={consumableTriggers} onChildOpenChanged={onChildOpenChanged} />)}
    </div>
  );
}
