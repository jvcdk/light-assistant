import './Device.css'

/**
 * Should match JsonDevice
 */
export interface IDevice {
  Name : string;
  Address : string;
  Vendor : string;
  Model : string;
  Description : string;
  BatteryPowered: boolean;

  /**
   * Additional device info that is sent seperately (not part of JsonDevice)
   * but sent in their own message types and attached client side to this
   * device object.
   */
  Status: IDeviceStatus | undefined;
  Routing: string[]
}

/**
 * Should match JsonDeviceStatus
 */
export interface IDeviceStatus {
  Address : string;
  LinkQuality: number;
  Battery: number;
  Brightness : number;
  State : boolean | undefined;
}

/**
 * Should match JsonDeviceRouting
 */
export interface IDeviceRouting {
  Address : string;
  Routing: string[];
}

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

function Route(route: string, idx: number)
{
  return (
    <div key={idx} className='Route'>{route}</div>
  )
}

function DeviceRouting(prop: { routing: string[] })
{
  const routing = prop.routing || [];
    return (
      <div className='Routing'>
        {routing.map((route, idx) => Route(route, idx))}
      </div>
    )
}

export function Device(device: IDevice) {
  const status = device.Status;
  const routing = device.Routing;
  return (
    <div className='Device' key={device.Address}>
      <div className='NameAddress'>
        <div className='Name'>{device.Name}</div>
        <div className='Address'>{device.Address}</div>
      </div>
      <div className='VendorModel'>
        <div className='Vendor'>{device.Vendor}</div>
        <div className='Model'>{device.Model}</div>
      </div>
      <div className='Description'>{device.Description}</div>
      <div className='Status'>
        <DeviceBattery battery={status?.Battery} />
        <DeviceLinkQuality lq={status?.LinkQuality} />
        <DeviceBrightness brightness={status?.Brightness} />
        <DeviceOnState onState={status?.State} />
      </div>
      <DeviceRouting routing={routing}></DeviceRouting>
    </div>
  );
}
