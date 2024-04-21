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

export type GetTargetRoutingOptionsType = (eventType: string | undefined) => string[] | undefined;
export type MapTargetAddressToNameType = (targetName: string) => string;

export interface IRoutingOptionsCallbacks {
  GetTargetRoutingOptions: GetTargetRoutingOptionsType;
  MapTargetAddressToName: MapTargetAddressToNameType;
} 


function TargetRoutingOptionsEntry(prop: {targetAddress: string, cb: IRoutingOptionsCallbacks}) {
  const name = prop.cb.MapTargetAddressToName(prop.targetAddress);
  console.log(`addr: ${prop.targetAddress} -> ${name}`);
  return (<option key={prop.targetAddress} value={name}>{name}</option>);
}

function TargetRoutingOptions(prop: {TargetAddress: string, routeTargetOptions: string[] | undefined, cb: IRoutingOptionsCallbacks}) {
  if(prop.routeTargetOptions === undefined)
    return null;

  if(prop.routeTargetOptions.length === 0)
    return (<div>No target devices available.</div>);

  return (<div>
    <select className='routeTarget' defaultValue={prop.TargetAddress} onChange={(e) => { console.log(e); }}>
      <option key="__unselected__" value={undefined}>&lt;Please select&gt;</option>
      {prop.routeTargetOptions.map(targetAddress => <TargetRoutingOptionsEntry targetAddress={targetAddress} cb={prop.cb} />)}
    </select>
  </div>);
}

function Route(prop: {route: IDeviceRoute, idx: number, routingOptions: IDeviceProvidedEvent[], cb: IRoutingOptionsCallbacks}) {
  function getSourceType(sourceEvent: string) : string | undefined {
    const matchingRouteOption = prop.routingOptions.find((el) => el.Name == sourceEvent);
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
    ...prop.route,
    SourceType: getSourceType(prop.route.SourceEvent)
  });
  const routeTargetOptions = prop.cb.GetTargetRoutingOptions(routeConfig.SourceType);
  const showRouteTargetIcon = routeTargetOptions != undefined && routeTargetOptions.length > 0;

  return(
    <div key={prop.idx} className='route'>
      <SvgRouteEntry />
      <select className='routeSourceEvent' defaultValue={routeConfig.SourceEvent} onChange={(e) => { updateSourceEvent(e.target.value); }}>
        <option value={undefined}>&lt;Please select&gt;</option>
        {prop.routingOptions.map((optionSourceEvent) => <option key={optionSourceEvent.Name} value={optionSourceEvent.Name}>{optionSourceEvent.Name}</option>)}
      </select>      

      {showRouteTargetIcon ? <SvgRouteMapsTo /> : null}
      <TargetRoutingOptions TargetAddress={routeConfig.TargetAddress} routeTargetOptions={routeTargetOptions} cb={prop.cb} />

      <SvgRouteColon /><span className='routeTargetFunctionality'>{routeConfig.TargetFunctionality}</span>
    </div>
  )
}

function RoutingOptions(prop: {device: IDevice, cb: IRoutingOptionsCallbacks }) {
  const device = prop.device;
  const routingOptions = device.RoutingOptions;
  if(routingOptions == undefined)
    return (<div>Error: No routing options available.</div>);

  return (
    <div className='routingOptions'>
      {device.Routing.map((route, idx) => <Route route={route} idx={idx} routingOptions={routingOptions.ProvidedEvents} cb={prop.cb} />)}
      <label className='addNew'>Add New</label>
    </div>
  );
}

export function PopUp_DeviceConfiguration(prop: {device: IDevice | null, cb: IRoutingOptionsCallbacks}) {
  const [device, setDevice] = useState<IDevice|null>(null);

  useEffect(() => {
    setDevice(cloneDeep(prop.device));
  }, [prop.device]);

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
          <RoutingOptions device={device} cb={prop.cb} />
        </div>
      </div>
    </div>
  );
}
