import './Popup_DeviceConfiguration.css'
import SvgRouteEntry from './image/route_entry.svg';
import SvgRouteColon from './image/route_colon.svg';
import SvgRouteMapsTo from './image/route_maps_to.svg';

import { IDevice, IDeviceProvidedEvent, IDeviceRoute } from "./Device";
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';

interface IRouteConfiguration {
  SourceEvent: string | undefined;
  TargetAddress: string | undefined;
  TargetFunctionality: string | undefined;
  SourceType: string | undefined;
}

export type GetTargetRoutingOptionsType = (eventType: string | undefined) => string[] | undefined;
export type GetTargetFunctionalityOptionsType = (eventType: string | undefined, targetAddress: string | undefined) => string[] | undefined;
export type TargetAddressToNameType = (targetName: string) => string;

export interface IRoutingOptionsCallbacks {
  GetTargetRoutingOptions: GetTargetRoutingOptionsType;
  GetTargetFunctionalityOptions : GetTargetFunctionalityOptionsType;
  TargetAddressToName: TargetAddressToNameType;
} 

function TargetRoutingOptions(prop: {targetAddress: string | undefined, routeTargetOptions: string[] | undefined, cb: IRoutingOptionsCallbacks, onChange: (value: string) => void}) {
  if(prop.routeTargetOptions === undefined)
    return null;

  if(prop.routeTargetOptions.length === 0)
    return (<div>No target devices available.</div>);

  return (<select className='routeTarget' defaultValue={prop.targetAddress} onChange={e => prop.onChange(e.target.value)}>
    <option key="__unselected__" value={undefined}>&lt;Please select&gt;</option>
    {prop.routeTargetOptions.map(targetAddress => <option key={targetAddress} value={targetAddress}>{prop.cb.TargetAddressToName(targetAddress)}</option>)}
  </select>);
}

function TargetFunctionalityOptions(prop: {targetFunctionality: string | undefined, targetFunctionalityOptions: string[] | undefined, onChange: (value: string) => void}) {
  if(prop.targetFunctionality === undefined || prop.targetFunctionalityOptions === undefined)
    return null;

  if(prop.targetFunctionalityOptions.length === 0)
    return (<div>No target functionality available.</div>);

  return (<select className='routeTargetFunctionality' defaultValue={prop.targetFunctionality} onChange={(e) => prop.onChange(e.target.value) }>
    <option key="__unselected__" value={undefined}>&lt;Please select&gt;</option>
    {prop.targetFunctionalityOptions.map(targetFunc => <option key={targetFunc} value={targetFunc}>{targetFunc}</option>)}    
  </select>)
}

function Route(prop: {route: IDeviceRoute, idx: number, routingOptions: IDeviceProvidedEvent[], cb: IRoutingOptionsCallbacks}) {
  function GetSourceType(sourceEvent: string) : string | undefined {
    const matchingRouteOption = prop.routingOptions.find((el) => el.Name == sourceEvent);
    const result = matchingRouteOption?.EventType;
    return result;
  }

  function UpdateSourceEvent(sourceEvent: string) {
    setRouteConfig({
      ...routeConfig,
      SourceEvent: sourceEvent,
      SourceType: GetSourceType(sourceEvent)
    });
  }

  function UpdateTarget(targetAddress: string) {
    setRouteConfig({
      ...routeConfig,
      TargetAddress: targetAddress
    });
  }

  function UpdateFunctionality(targetFunc: string) {
    setRouteConfig({
      ...routeConfig,
      TargetFunctionality: targetFunc
    });
  }

  const [routeConfig, setRouteConfig] = useState<IRouteConfiguration>({
    ...prop.route,
    SourceType: GetSourceType(prop.route.SourceEvent)
  });
  const routeTargetOptions = prop.cb.GetTargetRoutingOptions(routeConfig.SourceType);
  const showRouteTargetIcon = routeTargetOptions != undefined;
  const targetFunctionalityOptions = prop.cb.GetTargetFunctionalityOptions(routeConfig.SourceType, routeConfig.TargetAddress);
  const showTargetFunctinalityIcon = targetFunctionalityOptions != undefined;

  return(
    <div key={prop.idx} className='route'>
      <SvgRouteEntry />
      <select className='routeSourceEvent' defaultValue={routeConfig.SourceEvent} onChange={(e) => { UpdateSourceEvent(e.target.value); }}>
        <option value={undefined}>&lt;Please select&gt;</option>
        {prop.routingOptions.map((optionSourceEvent) => <option key={optionSourceEvent.Name} value={optionSourceEvent.Name}>{optionSourceEvent.Name}</option>)}
      </select>      

      {showRouteTargetIcon ? <SvgRouteMapsTo /> : null}
      <TargetRoutingOptions targetAddress={routeConfig.TargetAddress} routeTargetOptions={routeTargetOptions} cb={prop.cb} onChange={UpdateTarget} />

      {showTargetFunctinalityIcon ? <SvgRouteColon /> : null}
      <TargetFunctionalityOptions targetFunctionality={routeConfig.TargetFunctionality} targetFunctionalityOptions={targetFunctionalityOptions} onChange={UpdateFunctionality} />
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
      {device.Routing.map((route, idx) => <Route key={idx} route={route} idx={idx} routingOptions={routingOptions.ProvidedEvents} cb={prop.cb} />)}
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
