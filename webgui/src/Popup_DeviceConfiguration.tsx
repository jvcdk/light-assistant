import './Popup_DeviceConfiguration.css'
import SvgRouteEntry from './image/route_entry.svg';
import SvgRouteColon from './image/route_colon.svg';
import SvgRouteMapsTo from './image/route_maps_to.svg';

import { IDevice, IDeviceProvidedEvent, IDeviceRoute } from "./Device";
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';

interface IRouteConfiguration extends IDeviceRoute {
  SourceType: string | undefined;
}

type GetRoutingOptionsType = (eventType: string) => string[];

function Route(route: IDeviceRoute, idx: number, routingOptions: IDeviceProvidedEvent[], getRoutingOptions: GetRoutingOptionsType)
{
  function getSourceType(sourceEvent: string) : string | undefined {
    const matchingRouteOption = routingOptions.find((el) => el.Name == sourceEvent);
    const result = matchingRouteOption?.EventType;
    return result;
  }

  function updateSourceEvent(sourceEvent: string) {
    setRouteConfig({
      ...routeConfig,
      SourceEvent: sourceEvent,
      SourceType: getSourceType(sourceEvent)
    });
  }

  const [routeConfig, setRouteConfig] = useState<IRouteConfiguration>({
    ...route,
    SourceType: getSourceType(route.SourceEvent)
  });

  useEffect(() => {
    console.log(routeConfig);
  }, [routeConfig])

  return(
    <div key={idx} className='route'>
      <SvgRouteEntry />
      <select className='routeSourceEvent' defaultValue={routeConfig.SourceEvent} onChange={(e) => { updateSourceEvent(e.target.value); }}>
        <option>&lt;Please select&gt;</option>
        {routingOptions.map((option) => <option key={option.Name} value={option.Name}>{option.Name}</option>)}
      </select>      
      <SvgRouteMapsTo /><span className='routeTargetAddress'>{routeConfig.TargetAddress}</span>
      <SvgRouteColon /><span className='routeTargetFunctionality'>{routeConfig.TargetFunctionality}</span>
    </div>
  )
}

function RoutingOptions(prop: {device: IDevice, getRoutingOptions: GetRoutingOptionsType }) {
  const device = prop.device;
  const routingOptions = device.RoutingOptions;
  if(routingOptions == undefined)
    return (<div>Error: No routing options available.</div>);

  return (
    <div className='routingOptions'>
      {device.Routing.map((route, idx) => Route(route, idx, routingOptions.ProvidedEvents, prop.getRoutingOptions))}
      <label className='addNew'>Add New</label>
    </div>
  );
}

export function PopUp_DeviceConfiguration(_device: IDevice | null, getRoutingOptions: GetRoutingOptionsType) {
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
          <RoutingOptions device={device} getRoutingOptions={getRoutingOptions} />
        </div>
      </div>
    </div>
  );
}
