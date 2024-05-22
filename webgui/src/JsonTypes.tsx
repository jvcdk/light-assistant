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

  /**
   * Additional device info that is sent seperately (not part of JsonDevice)
   * but sent in their own message types and attached client side to this
   * device object.
   */
  Status: IDeviceStatus | undefined;
  Routing: IDeviceRoute[];
  RoutingOptions: IDeviceRoutingOptions | undefined;
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
