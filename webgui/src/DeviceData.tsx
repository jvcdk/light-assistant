import { IDevice, IDeviceRoute, IDeviceRoutingOptions, IDeviceStatus } from './JsonTypes';

export type FindDeviceDataType = (address: string, issueWarningNotFound?: boolean) => DeviceData | undefined;
export class DeviceData {
  Device: IDevice;
  Status: IDeviceStatus | undefined;
  Routing: IDeviceRoute[] = [];
  RoutingOptions: IDeviceRoutingOptions | undefined;

  constructor(device: IDevice) {
    this.Device = device;
  }
}
