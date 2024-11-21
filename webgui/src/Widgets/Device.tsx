import './Device.css'
import { DeviceData, FindDeviceDataType } from '../Data/DeviceData';
import { IDeviceRoute, IDeviceScheduleEntry } from '../Data/JsonTypes';
import { ScheduleTrigger } from '../Data/ScheduleTrigger';

function DeviceBattery(prop: { battery: number | undefined })
{
  if(prop.battery == undefined)
    return null;

    return (
      <div className='Battery'>{prop.battery}</div>
    )
}

function DeviceLinkQuality(prop: { lq: number | undefined })
{
  if(prop.lq == undefined)
    return null;

    return (
      <div className='LinkQuality'>{prop.lq}</div>
    )
}

function DeviceBrightness(prop: { brightness: number | undefined })
{
  if(prop.brightness == undefined)
    return null;

    return (
      <div className='Brightness'>{prop.brightness}</div>
    )
}

function DeviceOnState(prop: { onState: boolean | undefined })
{
  if(prop.onState == undefined)
    return null;

    return (
      <div className='State'>{prop.onState ? "On" : "Off"}</div>
    )
}

function Route(route: IDeviceRoute, idx: number, findDevice: FindDeviceDataType)
{
  const targetName = findDevice(route.TargetAddress)?.Device.Name || route.TargetAddress;
  return (
    <div key={idx} className='Route'>{route.SourceEvent} -&gt; {targetName}</div>
  )
}

function DeviceRouting(prop: { routing: IDeviceRoute[], findDevice: FindDeviceDataType })
{
  const routing = prop.routing || [];
    return (
      <div className='Routing'>
        {routing.map((route, idx) => Route(route, idx, prop.findDevice))}
      </div>
    )
}

function RenderDays(props: {days: number[]}) {
  const days = props.days;
  const daysIndices = [0, 1, 2, 3, 4, 5, 6];
  return (
    <div className='Days'>{daysIndices.map(idx => {
      const dayType = idx <= 4 ? 'Weekday' : 'Weekend';
      const isSet = days.includes(idx) ? 'DaySet' : '';
      return <div key={idx} className={`DayEntry ${dayType} ${isSet}`}></div>
    })}</div>
  )
}

function DeviceScheduleEntry(prop: { entry: IDeviceScheduleEntry }) {
  const entry = prop.entry;
  const trigger = new ScheduleTrigger(entry.Trigger);
  return (
    <div className='ScheduleEntry FlexHori'>
      <RenderDays days={trigger.Days} />
      <div className='Time'>{trigger.Time.asString}</div>
      <div className='EventType'>{entry.EventType}</div>
    </div>
  )
}

function DeviceSchedule(prop: { schedule: IDeviceScheduleEntry[] })
{
  const schedule = prop.schedule || [];
  return (
    <div className='Schedule'>
      {schedule.map((entry, idx) => <DeviceScheduleEntry key={idx} entry={entry} />)}
    </div>
  )
}

export function Device(devData: DeviceData, openPopup: () => void, findDevice: FindDeviceDataType) {
  const device = devData.Device;
  const status = devData.Status;
  const routing = devData.Routing;
  return (
    <div onClick={openPopup} className='Device' key={device.Address}>
      <div className='NameAddress'>
        <div className='Name'>{device.Name}</div>
        <div className='Address'>{device.Address}</div>
      </div>
      <div className='Status'>
        <DeviceBattery battery={status?.Battery} />
        <DeviceLinkQuality lq={status?.LinkQuality} />
        <DeviceBrightness brightness={status?.Brightness} />
        <DeviceOnState onState={status?.State} />
      </div>
      <DeviceRouting routing={routing} findDevice={findDevice} />
      <DeviceSchedule schedule={devData.Schedule} />
    </div>
  );
}
