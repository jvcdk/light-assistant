import './Popup_DeviceConfiguration.css'
import SvgRouteEntry from './image/route_entry.svg';
import SvgRouteColon from './image/route_colon.svg';
import SvgRouteMapsTo from './image/route_maps_to.svg';

import { IDevice, IDeviceProvidedEvent, IDeviceRoute } from './JsonTypes';
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';

interface IDeviceRouteWithKey extends IDeviceRoute {
  key: number;
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

  return (<select className='routeTarget' defaultValue={prop.cb.TargetAddressToName(prop.targetAddress || "")} onChange={e => prop.onChange(e.target.value)}>
    <option key="__unselected__" value="">&lt;Please select&gt;</option>
    {prop.routeTargetOptions.map(targetAddress => <option key={targetAddress} value={targetAddress}>{prop.cb.TargetAddressToName(targetAddress)}</option>)}
  </select>);
}

function TargetFunctionalityOptions(prop: {targetFunctionality: string | undefined, targetFunctionalityOptions: string[] | undefined, onChange: (value: string) => void}) {
  if(prop.targetFunctionality === undefined || prop.targetFunctionalityOptions === undefined)
    return null;

  if(prop.targetFunctionalityOptions.length === 0)
    return (<div>No target functionality available.</div>);

  return (<select className='routeTargetFunctionality' defaultValue={prop.targetFunctionality} onChange={(e) => prop.onChange(e.target.value) }>
    <option key="__unselected__" value="">&lt;Please select&gt;</option>
    {prop.targetFunctionalityOptions.map(targetFunc => <option key={targetFunc} value={targetFunc}>{targetFunc}</option>)}    
  </select>)
}

function Route(prop: {route: IDeviceRouteWithKey, idx: number, routingOptions: IDeviceProvidedEvent[], cb: IRoutingOptionsCallbacks, onChange: (value: IDeviceRouteWithKey) => void}) {
  function GetSourceType(sourceEvent: string) : string | undefined {
    const matchingRouteOption = prop.routingOptions.find((el) => el.Name == sourceEvent);
    const result = matchingRouteOption?.EventType;
    return result;
  }

  function UpdateSourceEvent(sourceEvent: string) {
    prop.onChange({
      ...prop.route,
      SourceEvent: sourceEvent,
    });
  }

  function UpdateTarget(targetAddress: string) {
    prop.onChange({
      ...prop.route,
      TargetAddress: targetAddress
    });
  }

  function UpdateFunctionality(targetFunc: string) {
    prop.onChange({
      ...prop.route,
      TargetFunctionality: targetFunc
    });
  }

  const sourceType = GetSourceType(prop.route.SourceEvent)
  const routeTargetOptions = prop.cb.GetTargetRoutingOptions(sourceType);
  const showRouteTargetIcon = routeTargetOptions != undefined;
  const targetFunctionalityOptions = prop.cb.GetTargetFunctionalityOptions(sourceType, prop.route.TargetAddress);
  const showTargetFunctinalityIcon = targetFunctionalityOptions != undefined;

  return(
    <div key={prop.idx} className='route'>
      <SvgRouteEntry />
      <select className='routeSourceEvent' defaultValue={prop.route.SourceEvent} onChange={(e) => { UpdateSourceEvent(e.target.value); }}>
        <option value="">&lt;Please select&gt;</option>
        {prop.routingOptions.map((optionSourceEvent) => <option key={optionSourceEvent.Name} value={optionSourceEvent.Name}>{optionSourceEvent.Name}</option>)}
      </select>      

      {showRouteTargetIcon ? <SvgRouteMapsTo /> : null}
      <TargetRoutingOptions targetAddress={prop.route.TargetAddress} routeTargetOptions={routeTargetOptions} cb={prop.cb} onChange={UpdateTarget} />

      {showTargetFunctinalityIcon ? <SvgRouteColon /> : null}
      <TargetFunctionalityOptions targetFunctionality={prop.route.TargetFunctionality} targetFunctionalityOptions={targetFunctionalityOptions} onChange={UpdateFunctionality} />
    </div>
  )
}

let routingKey: number = 1;
function CreateEmptyRoutingWithKey() : IDeviceRouteWithKey {
  return {
    SourceEvent: "",
    TargetAddress: "",
    TargetFunctionality: "",
    key: routingKey++,
  } as IDeviceRouteWithKey;
}

function RoutingOptions(prop: {device: IDevice, cb: IRoutingOptionsCallbacks, setDeviceRoute: (route: IDeviceRoute[]) => void }) {
  const device = prop.device;
  const routingOptions = device.RoutingOptions;

  const routingWithKey = device.Routing.map(route => { return {
    ...route,
    key: routingKey++,
  } as IDeviceRouteWithKey})
  routingWithKey.push(CreateEmptyRoutingWithKey());

  const [deviceRoute, setDeviceRoute] = useState<IDeviceRouteWithKey[]>(routingWithKey);

  if(routingOptions == undefined)
    return (<div>Error: No routing options available.</div>);

  const providedEvents = routingOptions?.ProvidedEvents;
  if(providedEvents.length == 0)
    return (<div>Device does not provide any events.</div>);

  function updateDeviceRoute(newRoute: IDeviceRouteWithKey) {
    let result = deviceRoute.slice();
    const entry = result.find(el => el.key == newRoute.key);
    if(entry == undefined) {
      console.log("Error: Did not find routing entry.");
      return;
    }

    Object.assign(entry, newRoute);
    result = result.filter(route => providedEvents.find(el => el.Name == route.SourceEvent) != undefined);  
    result.push(CreateEmptyRoutingWithKey());
 
    // We need to both update a local state and a parent (prop.) state.
    // The local state ensures that the drop-down boxes do not close as new (status) data arrives (from the server) about the device.
    // The parent state ensures that, well, the parent is up-to-date and can be sent to server when user presses Apply.
    setDeviceRoute(result);
    prop.setDeviceRoute(result);
  }

  return (
    <div className='routingOptions'>
      {deviceRoute.map((route, idx) => 
        <Route key={route.key} route={route} idx={idx} routingOptions={providedEvents} cb={prop.cb} onChange={newRoute => updateDeviceRoute(newRoute)} />)}
    </div>
  );
}

export type CloseDeviceConfigurationType = (device: IDevice | null) => void;
export function PopUp_DeviceConfiguration(prop: {device: IDevice | null, cb: IRoutingOptionsCallbacks, cbOnClose: CloseDeviceConfigurationType}) {
  const [device, setDevice] = useState<IDevice|null>(null);

  useEffect(() => {
    setDevice(cloneDeep(prop.device));
  }, [prop.device]);

  function UpdateDeviceRoute(route: IDeviceRoute[]) {
    setDevice({
      ...device,
      Routing: route,
    } as IDevice);
  }

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
          <RoutingOptions device={device} cb={prop.cb} setDeviceRoute={UpdateDeviceRoute} />
        </div>
      <div className='buttons'>
        <input className='cancel' type='button' onClick={() => prop.cbOnClose(null)} value="Cancel" />
        <input className='ok' type='button' onClick={() => prop.cbOnClose(device)} value="Apply" />
      </div>
      </div>
    </div>
  );
}
