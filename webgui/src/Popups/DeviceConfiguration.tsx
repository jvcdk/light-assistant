import './DeviceConfiguration.css'
import SvgRouteEntry from '../image/route_entry.svg';
import SvgRouteColon from '../image/route_colon.svg';
import SvgRouteMapsTo from '../image/route_maps_to.svg';

import { IDeviceProvidedEvent, IDeviceRoute } from '../Data/JsonTypes';
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';
import { DeviceData } from '../Data/DeviceData';
import Popup from 'reactjs-popup';

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

  return (<select className='RouteTarget' defaultValue={prop.targetAddress || ""} onChange={e => prop.onChange(e.target.value)}>
    <option key="__unselected__" value="">&lt;Please select&gt;</option>
    {prop.routeTargetOptions.map(targetAddress => <option key={targetAddress} value={targetAddress}>{prop.cb.TargetAddressToName(targetAddress)}</option>)}
  </select>);
}

function TargetFunctionalityOptions(prop: {targetFunctionality: string | undefined, targetFunctionalityOptions: string[] | undefined, onChange: (value: string) => void}) {
  if(prop.targetFunctionality === undefined || prop.targetFunctionalityOptions === undefined)
    return null;

  if(prop.targetFunctionalityOptions.length === 0)
    return (<div>No target functionality available.</div>);

  return (<select className='RouteTargetFunctionality' defaultValue={prop.targetFunctionality} onChange={(e) => prop.onChange(e.target.value) }>
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
    <div key={prop.idx} className='Route'>
      <SvgRouteEntry />
      <select className='RouteSourceEvent' defaultValue={prop.route.SourceEvent} onChange={(e) => { UpdateSourceEvent(e.target.value); }}>
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

function RoutingOptions(prop: {devData: DeviceData, cb: IRoutingOptionsCallbacks, setDeviceRoute: (route: IDeviceRoute[]) => void }) {
  const devData = prop.devData;
  const routingOptions = devData.RoutingOptions;

  const routingWithKey = devData.Routing.map(route => { return {
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
    <div className='RoutingOptions'>
      {deviceRoute.map((route, idx) => 
        <Route key={route.key} route={route} idx={idx} routingOptions={providedEvents} cb={prop.cb} onChange={newRoute => updateDeviceRoute(newRoute)} />)}
    </div>
  );
}

export interface DeviceConfigurationProps {
  isOpen: boolean;
  devData: DeviceData | null;
  routingCallbacks: IRoutingOptionsCallbacks;
  onClose: (devData: DeviceData | null) => void;
}
export function DeviceConfiguration(props: DeviceConfigurationProps) {
  const [devData, setDevData] = useState<DeviceData|null>(null);

  useEffect(() => {
    setDevData(cloneDeep(props.devData));
  }, [props.devData]);

  function UpdateDeviceRoute(route: IDeviceRoute[]) {
    setDevData({
      ...devData,
      Routing: route,
    } as DeviceData);
  }

  if(devData === null)
    return ("");

  const device = devData.Device;
  return (
    <Popup open={props.isOpen} onClose={() => props.onClose(null)} modal closeOnDocumentClick={false}>
      <div className='DeviceConfiguration'>
        <div className='Title'>{device.Vendor} – {device.Model}</div>
        <div className='SubTitle'>{device.Description}</div>
        <div className='SubTitle'>Address: {device.Address}</div>
        <div className='Content'>
          <label className='Label'>Friendly name:</label>
          <input className='FriendlyName' type='text' defaultValue={device.Name} onChange={(e) => device.Name = e.target.value} />
          <div className='Routing'>
            <label className='Label'>Routing:</label>
            <RoutingOptions devData={devData} cb={props.routingCallbacks} setDeviceRoute={UpdateDeviceRoute} />
          </div>
        <div className='Buttons'>
          <input className='Cancel' type='button' onClick={() => props.onClose(null)} value="Cancel" />
          <input className='Ok' type='button' onClick={() => props.onClose(devData)} value="Apply" />
        </div>
        </div>
      </div>
    </Popup>
  );
}
