import { IDevice, IDeviceConsumableTrigger, IDeviceRoute, IDeviceRoutingOptions, IDeviceStatus, IDeviceScheduleEntry } from './JsonTypes';

export type FindDeviceData1Type = (address: string, issueWarningNotFound?: boolean) => DeviceData1 | undefined;
export class DeviceData1 {
  Device: IDevice;
  Status: IDeviceStatus | undefined;
  Routing: IDeviceRoute[] = [];
  RoutingOptions: IDeviceRoutingOptions | undefined;
  Schedule: IDeviceScheduleEntry[] = [];
  ConsumableTriggers: IDeviceConsumableTrigger[] = [];

  constructor(device: IDevice) {
    this.Device = device;
  }
}

export type FindDeviceData2Type = (address: string, issueWarningNotFound?: boolean) => DeviceData2 | undefined;
export class DeviceData2 {
  Device: IDevice;
  ConsumableTriggers: IDeviceConsumableTrigger[] = [];

  constructor(device: IDevice) {
    this.Device = device;
  }
}
