import './Popup_DeviceConfiguration.css'
import { IDevice, IDeviceRoute } from "./Device";

import SvgRouteEntry from './image/route_entry.svg';
import SvgRouteColon from './image/route_colon.svg';
import SvgRouteMapsTo from './image/route_maps_to.svg';

function Route(route: IDeviceRoute, idx: number)
{
  return(
    <div key={idx} className='route'>
      <SvgRouteEntry /><span className='routeSourceEvent'>{route.SourceEvent}</span>
      <SvgRouteMapsTo /><span className='routeTargetAddress'>{route.TargetAddress}</span>
      <SvgRouteColon /><span className='routeTargetFunctionality'>{route.TargetFunctionality}</span>
    </div>
  )
}

function RoutingOptions(prop: {device: IDevice }) {
  const device = prop.device;
  return (
    <div className='routingOptions'>
      {device.Routing.map((route, idx) => Route(route, idx))}
      <label className='addNew'>Add New</label>
    </div>
  );
}

export function PopUp_DeviceConfiguration(device: IDevice | null) {
  if(device === null)
    return ("Error: No device selected.");

  return (
    <div className='Popup.DeviceConfiguration'>
      <div className='title'>{device.Name} [{device.Address}]</div>
      <div className='content'>
        <label className='label'>Friendly Name:</label>
        <input className='friendlyname' type='text' value={device.Name} onChange={(e) => device.Name = e.target.value} />
        <div className='routing'>
          <label className='label'>Routing:</label>
          <RoutingOptions device={device} />
        </div>
      </div>
    
    </div>
  );
}
