import { IDevice, IDeviceConsumableAction, IDeviceRoute, IDeviceRoutingOptions, IDeviceStatus, IDeviceScheduleEntry, IServiceOptions } from './JsonTypes';

export type FindDeviceDataType = (address: string, issueWarningNotFound?: boolean) => DeviceData | undefined;
export class DeviceData {
  Device: IDevice;
  Status: IDeviceStatus | undefined;
  Routing: IDeviceRoute[] = [];
  RoutingOptions: IDeviceRoutingOptions | undefined;
  Schedule: IDeviceScheduleEntry[] = [];
  ConsumableActions: IDeviceConsumableAction[] = [];
  ServiceOptions: IServiceOptions | undefined;

  constructor(device: IDevice) {
    this.Device = device;
  }
}
