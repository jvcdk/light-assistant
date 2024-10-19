/**
 * Should match JsonServerToClientMessage
 */
export interface IServerToClientMessage {
  Devices: IDevice[];
  DeviceStatus: IDeviceStatus | undefined;
  Routing: IDeviceRouting;
  RoutingOptions: IDeviceRoutingOptions | undefined;
  ScheduleTriggerOptions: IScheduleTriggerOptions | undefined;
  OpenNetworkStatus : IOpenNetworkStatus | undefined;
}


/**
 * Should match JsonDevice
 */
export interface IDevice {
  Name: string;
  Address: string;
  Vendor: string;
  Model: string;
  Description: string;
  BatteryPowered: boolean;
}

/**
 * Should match JsonDeviceStatus
 */
export interface IDeviceStatus {
  Address: string;
  LinkQuality: number;
  Battery: number;
  Brightness: number;
  State: boolean | undefined;
}

/**
 * Should match JsonDeviceRouting
 */
export interface IDeviceRouting {
  Address: string;
  Routing: IDeviceRoute[];
}

/**
 * Should match JsonDeviceRoute
 */
export interface IDeviceRoute {
  SourceEvent: string;
  TargetAddress: string;
  TargetFunctionality: string;
}

/**
 * Should match JsonDeviceRoutingOptions
 */
export interface IDeviceRoutingOptions {
  Address: string;
  ProvidedEvents: IDeviceProvidedEvent[];
  ConsumableEvents: IDeviceConsumableEvent[];
}

/**
 * Should match JsonDeviceConsumableEvent
 */
export interface IDeviceConsumableEvent {
  EventType: string;
  TargetName: string;
}

/**
 * Should match JsonDeviceProvidedEvent
 */
export interface IDeviceProvidedEvent {
  EventType: string;
  Name: string;
}

/**
 * Should match JsonDeviceSchedule
 */
export interface IDeviceSchedule {
  Address: string;
  Schedule: IDeviceScheduleEntry[];
}

/**
 * Should match JsonDeviceScheduleEntry
 */
export interface IDeviceScheduleEntry {
  EventType: string | undefined; // EventType is the action to be taken
  Parameters: Map<string, string>;
  Trigger: IScheduleTrigger;
}

/**
 * Should match JsonScheduleTrigger
 */
export interface ITimeOfDay {
  hour: number;
  minute: number;
}

/**
 * Should match JsonScheduleTrigger
 */
export interface IScheduleTrigger {
  Days: number[];
  Time: ITimeOfDay;
}

/**
 * Should match JsonScheduleTriggerOptions
 */
export interface IScheduleTriggerOptions {
  Address: string;
  ConsumableTriggers: IDeviceConsumableTrigger[];
}

/**
 * Should match JsonDeviceConsumableTrigger
 */
export interface IDeviceConsumableTrigger {
  EventType: string;
  Parameters: IParamInfo[];
}

/**
 * Should match JsonParamInfo
 */
export interface IParamInfo {
  Name: string;
  Type: string;
}

/**
 * Should match JsonParamEnum
 */
export interface IParamEnum extends IParamInfo {
  Values: string[];
  Default: string;
}

/**
 * Should match JsonParamFloat
 */
export interface IParamFloat extends IParamInfo {
  Min: number;
  Max: number;
  Default: number;
}

/**
 * Should match JsonParamBrightness
 */
export interface IParamBrightness extends IParamFloat {
}

/**
 * Should match JsonOpenNetworkStatus
 */
export interface IOpenNetworkStatus {
  Status: boolean;
  Time: number;
}

/**
 * Should match JsonDeviceConfigurationChange
 */
export class DeviceConfigurationChange {
  Address: string;
  Name: string;
  Route: IDeviceRoute[];

  constructor(address: string, name: string, route: IDeviceRoute[]) {
    this.Address = address;
    this.Name = name;
    this.Route = route;
  }
}

/**
 * Should match JsonIngressMessage
 */
export class ClientToServerMessage {
  DeviceConfigurationChange: DeviceConfigurationChange | undefined;
  RequestOpenNetwork: boolean = false;
}
