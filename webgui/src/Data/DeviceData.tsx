import { IDevice, IDeviceConsumableAction, IDeviceRoute, IDeviceRoutingOptions, IDeviceStatus, IDeviceScheduleEntry } from './JsonTypes';

export type FindDeviceDataType = (address: string, issueWarningNotFound?: boolean) => DeviceData | undefined;
export class DeviceData {
  Device: IDevice;
  Status: IDeviceStatus | undefined;
  Routing: IDeviceRoute[] = [];
  RoutingOptions: IDeviceRoutingOptions | undefined;
  Schedule: IDeviceScheduleEntry[] = [];
  ConsumableActions: IDeviceConsumableAction[] = [];

  constructor(device: IDevice) {
    this.Device = device;
  }
}
