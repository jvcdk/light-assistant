/**
 * Should match JsonServerToClientMessage
 */
export interface IServerToClientMessage {
  Devices: IDevice[];
  DeviceStatus: IDeviceStatus | undefined;
  Routing: IDeviceRouting;
  RoutingOptions: IDeviceRoutingOptions | undefined;
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
