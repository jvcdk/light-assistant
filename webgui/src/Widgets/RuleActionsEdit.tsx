import { DeviceData2 } from "../Data/DeviceData";
import { IDevice } from "../Data/JsonTypes";

export class TriggerAction {
  EventType: string = "";
  Parameters: [] = [];
}

export interface SelectActionsProps {
  selectedDevice: string;
  devData: DeviceData2[];
  onChange: (value: string) => void;
}
function SelectAction(props: SelectActionsProps) {
  const devData = props.devData;
  const device = devData.find(dev => dev.Device.Address === props.selectedDevice);
  const actions = device?.ConsumableTriggers;

  if (actions == undefined || actions.length === 0)
    return (<div></div>);

  return (
    <div>
      <select className='TriggerDeviceAction' onChange={(e) => props.onChange(e.target.value)}>
        <option value="">&lt;Please select&gt;</option>
        {actions.map((act) => <option key={act.EventType} value={act.EventType}>{act.EventType}</option>)}
      </select>
    </div>
  );
}

export interface SelectDeviceProps {
  selectedDevice: string;
  devices: IDevice[];
  onChange: (value: string) => void;
}
function SelectDevice(props: SelectDeviceProps) {
  const selectedDevice = props.selectedDevice;
  const devices = props.devices;
  return <select className='TriggerDeviceAddress' defaultValue={selectedDevice} onChange={(e) => props.onChange(e.target.value)}>
    <option value="">&lt;Please select&gt;</option>
    {devices.map((dev) => <option key={dev.Address} value={dev.Address}>{dev.Name}</option>)}
  </select>;
}


export interface RuleActionsEditPropsProps {
  action: TriggerAction;
  selectedDevice: string;
  devData: DeviceData2[];
  onDeviceChange: (value: string) => void;
  onActionChange: (value: TriggerAction) => void;
}
export function RuleActionsEdit(props: RuleActionsEditPropsProps) {
  const devData = props.devData;
  const selectedDevice = props.selectedDevice;

  const devices = devData.filter(dev => dev.ConsumableTriggers.length > 0)
    .map(dev => dev.Device);

  function UpdateAction(action: string) {
    props.onActionChange({
      ...props.action,
      EventType: action,
    });
  }

  if (Object.entries(devices).length === 0)
    return (<div>No devices available (with Schedule Trigger options).</div>);

  return (<div>
    <SelectDevice selectedDevice={selectedDevice} devices={devices} onChange={props.onDeviceChange} />
    <SelectAction selectedDevice={selectedDevice} devData={devData} onChange={UpdateAction} />
  </div>);
}
