import './Popup_DeviceConfiguration.css'
import SvgRouteEntry from './image/route_entry.svg';
import SvgRouteColon from './image/route_colon.svg';
import SvgRouteMapsTo from './image/route_maps_to.svg';

import { IDevice, IDeviceProvidedEvent, IDeviceRoute } from "./Device";
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';

function Route(route: IDeviceRoute, idx: number, routingOptions: IDeviceProvidedEvent[])
{
  return(
    <div key={idx} className='route'>
      <SvgRouteEntry />
      <select className='routeSourceEvent' defaultValue={route.SourceEvent}>
        <option>&lt;Please select&gt;</option>
        {routingOptions.map((option) => <option key={option.Name} value={option.Name}>{option.Name}</option>)}
      </select> 
      
      <SvgRouteMapsTo /><span className='routeTargetAddress'>{route.TargetAddress}</span>
      <SvgRouteColon /><span className='routeTargetFunctionality'>{route.TargetFunctionality}</span>
    </div>
  )
}

function RoutingOptions(prop: {device: IDevice }) {
  const device = prop.device;
  const routingOptions = device.RoutingOptions;
  if(routingOptions == undefined)
    return (<div>Error: No routing options available.</div>);

  return (
    <div className='routingOptions'>
      {device.Routing.map((route, idx) => Route(route, idx, routingOptions.ProvidedEvents))}
      <label className='addNew'>Add New</label>
    </div>
  );
}

export function PopUp_DeviceConfiguration(_device: IDevice | null) {
  const [device, setDevice] = useState<IDevice|null>(null);

  useEffect(() => {
    setDevice(cloneDeep(_device));
  }, [_device]);

  if(device === null)
    return (<div>Error: No device selected.</div>);

  return (
    <div className='Popup.DeviceConfiguration'>
      <div className='title'>{device.Vendor} / {device.Model} – {device.Description}</div>
      <div className='subtitle'>(Address: {device.Address})</div>
      <div className='content'>
        <label className='label'>Friendly Name:</label>
        <input className='friendlyname' type='text' defaultValue={device.Name} onChange={(e) => device.Name = e.target.value} />
        <div className='routing'>
          <label className='label'>Routing:</label>
          <RoutingOptions device={device} />
        </div>
      </div>
    </div>
  );
}
