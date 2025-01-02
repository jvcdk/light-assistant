/**
 * Should match JsonServerToClientMessage
 */
export interface IServerToClientMessage {
  Devices: IDevice[];
  DeviceStatus: IDeviceStatus | undefined;
  Routing: IDeviceRouting;
  RoutingOptions: IDeviceRoutingOptions | undefined;
  Schedule: IDeviceSchedule | undefined;
  ScheduleActionOptions: IScheduleActionOptions | undefined;
  ServiceOptions: IServiceOptions | undefined;
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
 * Should match JsonDeviceScheduleEntry
 */
export interface IDeviceScheduleEntry {
  Key: number;
  EventType: string | undefined; // EventType is the action to be taken
  Parameters: object;
  Trigger: IScheduleTrigger;
}

/**
 * Should match JsonDeviceSchedule
 */
export interface IDeviceSchedule {
  Address: string;
  Schedule: IDeviceScheduleEntry[];
}

/**
 * Should match JsonScheduleAction
 */
export interface IScheduleTrigger {
  Days: number[];
  Time: ITimeOfDay;
}

/**
 * Should match JsonTimeOfDay
 */
export interface ITimeOfDay {
  Hour: number;
  Minute: number;
}

/**
 * Should match JsonScheduleActionOptions
 */
export interface IScheduleActionOptions {
  Address: string;
  ConsumableActions: IDeviceConsumableAction[];
}

/**
 * Should match JsonDeviceConsumableAction
 */
export interface IDeviceConsumableAction {
  EventType: string;
  Parameters: IParamInfo[];
}

/**
 * Should match JsonServiceOptions
 */
export interface IServiceOptions {
  Address: string;
  Params: IParamInfo[];
  Values: string[];
}

export type PreviewMode = 'None' | 'Raw' | 'Normalized';

/**
 * Should match JsonParamInfo
 */
export interface IParamInfo {
  Name: string;
  Type: string;
  Units: string;
  PreviewMode: PreviewMode;
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
 * Should match JsonParamInt
 */
export interface IParamInt extends IParamInfo {
  Min: number;
  Max: number;
  Default: number;
}

/**
 * Should match JsonOpenNetworkStatus
 */
export interface IOpenNetworkStatus {
  Status: boolean;
  Time: number;
}

/**
 * Should match JsonServiceOptionValue
 */
export interface IServiceOptionValue {
  Name: string;
  Value: string;
}

/**
 * Should match JsonDeviceConfigurationChange
 */
export class DeviceConfigurationChange {
  Address: string;
  Name: string;
  Route: IDeviceRoute[];
  Schedule: IDeviceScheduleEntry[];
  ServiceOptionValues: IServiceOptionValue[];

  constructor(address: string, name: string, route: IDeviceRoute[], schedule: IDeviceScheduleEntry[], serviceOptionValues: IServiceOptionValue[]) {
    this.Address = address;
    this.Name = name;
    this.Route = route;
    this.Schedule = schedule;
    this.ServiceOptionValues = serviceOptionValues;
  }
}

/**
 * Should match JsonDeviceOptionPreview
 */
export class DeviceOptionPreview {
  Address: string;
  Value: string;
  PreviewMode: PreviewMode;

  constructor(address: string, value: string, previewMode: PreviewMode) {
    this.Address = address;
    this.Value = value;
    this.PreviewMode = previewMode;
  }
}
/**
 * Should match JsonIngressMessage
 */
export class ClientToServerMessage {
  DeviceConfigurationChange: DeviceConfigurationChange | undefined;
  RequestOpenNetwork: boolean = false;
  DeviceOptionPreview: DeviceOptionPreview | undefined;
}
