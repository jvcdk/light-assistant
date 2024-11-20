import './DeviceRoutingOptions.css'
import SvgRouteEntry from '../image/route_entry.svg';
import SvgRouteColon from '../image/route_colon.svg';
import SvgRouteMapsTo from '../image/route_maps_to.svg';
import { useState } from "react";
import { DeviceData } from "../Data/DeviceData";
import { IDeviceProvidedEvent, IDeviceRoute } from "../Data/JsonTypes";

export interface IDeviceRouteWithKey extends IDeviceRoute {
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

export function Route(prop: {route: IDeviceRouteWithKey, idx: number, routingOptions: IDeviceProvidedEvent[], cb: IRoutingOptionsCallbacks, onChange: (value: IDeviceRouteWithKey) => void}) {
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
  const showTargetFunctionalityIcon = targetFunctionalityOptions != undefined;

  return(
    <div key={prop.idx} className='Route'>
      <SvgRouteEntry />
      <select className='RouteSourceEvent' defaultValue={prop.route.SourceEvent} onChange={(e) => { UpdateSourceEvent(e.target.value); }}>
        <option value="">&lt;Please select&gt;</option>
        {prop.routingOptions.map((optionSourceEvent) => <option key={optionSourceEvent.Name} value={optionSourceEvent.Name}>{optionSourceEvent.Name}</option>)}
      </select>

      {showRouteTargetIcon ? <SvgRouteMapsTo /> : null}
      <TargetRoutingOptions targetAddress={prop.route.TargetAddress} routeTargetOptions={routeTargetOptions} cb={prop.cb} onChange={UpdateTarget} />

      {showTargetFunctionalityIcon ? <SvgRouteColon /> : null}
      <TargetFunctionalityOptions targetFunctionality={prop.route.TargetFunctionality} targetFunctionalityOptions={targetFunctionalityOptions} onChange={UpdateFunctionality} />
    </div>
  )
}

let routingKey: number = 1;
export function CreateEmptyRoutingWithKey() : IDeviceRouteWithKey {
  return {
    SourceEvent: "",
    TargetAddress: "",
    TargetFunctionality: "",
    key: routingKey++,
  } as IDeviceRouteWithKey;
}

export function DeviceRoutingOptions(prop: { devData: DeviceData; cb: IRoutingOptionsCallbacks; setDeviceRoute: (route: IDeviceRoute[]) => void; }) {
  const devData = prop.devData;
  const routingOptions = devData.RoutingOptions;

  const routingWithKey = devData.Routing.map(route => {
    return {
      ...route,
      key: routingKey++,
    } as IDeviceRouteWithKey;
  });
  routingWithKey.push(CreateEmptyRoutingWithKey());

  const [deviceRoute, setDeviceRoute] = useState<IDeviceRouteWithKey[]>(routingWithKey);

  if (routingOptions == undefined)
    return (<div>Error: No routing options available.</div>);

  const providedEvents = routingOptions?.ProvidedEvents;
  if (providedEvents.length == 0)
    return (<div>Device does not provide any events.</div>);

  function updateDeviceRoute(newRoute: IDeviceRouteWithKey) {
    let result = deviceRoute.slice();
    const entry = result.find(el => el.key == newRoute.key);
    if (entry == undefined) {
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
      {deviceRoute.map((route, idx) => <Route key={route.key} route={route} idx={idx} routingOptions={providedEvents} cb={prop.cb} onChange={newRoute => updateDeviceRoute(newRoute)} />)}
    </div>
  );
}
