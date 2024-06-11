import './DeviceList.css'
import Popup from 'reactjs-popup';
import { useCallback, useEffect, useRef, useState } from "react";
import { useWebSocketContext } from "./WebSocketContext";
import { Device } from "./Device";
import { ClientToServerMessage, DeviceConfigurationChange, IDevice, IDeviceRouting, IDeviceRoutingOptions, IDeviceStatus } from './JsonTypes';
import { GetTargetRoutingOptionsType, IRoutingOptionsCallbacks, TargetAddressToNameType, PopUp_DeviceConfiguration, GetTargetFunctionalityOptionsType } from './Popup_DeviceConfiguration';

export function DeviceList() {
  const { sendMessage, lastMessage } = useWebSocketContext();
  const [_devices, setDevices] = useState<IDevice[]>([]);
  const devices = useRef(_devices);

  const [selectedDevice, setSelectedDevice] = useState<IDevice | null>(null);
  const [popupOpen, setPopupOpen] = useState(false);

  const openPopup = (device: IDevice) => {
    setSelectedDevice(device);
    setPopupOpen(true);
  }
  const closeModal = () => setPopupOpen(false);

  type FindDeviceType = (address: string, issueWarningNotFound?: boolean) => IDevice | undefined;
  const FindDevice: FindDeviceType = useCallback((address: string, issueWarningNotFound: boolean = true): IDevice | undefined => {
    const result = devices.current.find(device => device.Address === address);
    if(result == undefined && issueWarningNotFound)
      console.log(`Warning: Searched for device ${address} but did not find it.`)

    return result;
  }, [devices]);

  const GetRoutingOptions: GetTargetRoutingOptionsType = useCallback((eventType: string | undefined) : string[] | undefined => {
    if(eventType == undefined)
      return undefined;

    const eligibleDevices = _devices.filter(dev => dev.RoutingOptions?.ConsumableEvents.some(ev => ev.EventType == eventType));
    return eligibleDevices.map(dev => dev.Address);
  }, [_devices]);

  const GetTargetFunctionalityOptions: GetTargetFunctionalityOptionsType = useCallback((eventType: string | undefined, targetAddress: string | undefined) => {
    const paramsOk = targetAddress && eventType;
    if(!paramsOk)
      return undefined;

    const device = FindDevice(targetAddress);
    if(device == undefined)
      return undefined;

    const consumableEvents = device.RoutingOptions?.ConsumableEvents;
    if(consumableEvents == undefined)
      return undefined;

    return consumableEvents.filter(ev => ev.EventType == eventType).map(ev => ev.TargetName);
  }, [FindDevice]);

  const TargetAddressToName: TargetAddressToNameType = useCallback((targetName: string) => {
    let device = null;
    if(targetName)
      device = FindDevice(targetName);

    return device?.Name || "<unknown>";
  }, [FindDevice]);

  useEffect(() => {
    function handleDeviceStatus(deviceStatus: IDeviceStatus) {
      const device = FindDevice(deviceStatus.Address);
      if (device)
        device.Status = deviceStatus;
    }

    function handleDeviceRouting(deviceRouting: IDeviceRouting) {
      const device = FindDevice(deviceRouting.Address);
      if (device)
        device.Routing = deviceRouting.Routing || [];
    }

    function handleDeviceList(deviceList: IDevice[]) {
      CopyAdditionalDevInfoFromExistingDevices();
      devices.current = deviceList;
      setDevices(devices.current);

      function CopyAdditionalDevInfoFromExistingDevices() {
        deviceList.forEach(newDevice => {
          const existingDevice = FindDevice(newDevice.Address, false);
          if (existingDevice) {
            newDevice.Status = existingDevice.Status;
            newDevice.Routing = existingDevice.Routing;
            newDevice.RoutingOptions = existingDevice.RoutingOptions;
          }
        });
      }
    }

    function handleDeviceRoutingOptions(deviceRoutingOptions: IDeviceRoutingOptions) {
      const device = FindDevice(deviceRoutingOptions.Address);
      if (device)
        device.RoutingOptions = deviceRoutingOptions;
    }

    if (lastMessage == undefined)
      return

    try {
      const data = JSON.parse(lastMessage.data)
      if (data["Devices"])
        handleDeviceList(data["Devices"]);
      if (data["DeviceStatus"])
        handleDeviceStatus(data["DeviceStatus"]);
      if (data["Routing"])
        handleDeviceRouting(data["Routing"]);
      if(data["RoutingOptions"])
        handleDeviceRoutingOptions(data["RoutingOptions"]);
    } catch (error) {
      console.log(`Error: Could not parse data '${lastMessage.data}'`)
      console.log(`Error message: ${error}`)
    }
  }, [FindDevice, lastMessage]);

  const cb = {
    GetTargetRoutingOptions: GetRoutingOptions,
    GetTargetFunctionalityOptions: GetTargetFunctionalityOptions,
    TargetAddressToName: TargetAddressToName,
  } as IRoutingOptionsCallbacks;

  function OnDeviceConfigurationUpdate(device: IDevice | null) {
    setPopupOpen(false);
    if(device == null)
      return;

    const msg = new ClientToServerMessage();
    msg.DeviceConfigurationChange = new DeviceConfigurationChange(device.Address, device.Name, device.Routing);
    sendMessage(JSON.stringify(msg));
  }

  return (
    <div className='DeviceList'>
      <div className='Heading'>
        <div className='NameAddress'>Name / Address</div>
        <div className='VendorModel'>Vendor / Model</div>
        <div className='Description'>Description</div>
        <div className='Status'>Status</div>
        <div className='Routing'>Routing</div>
      </div>
      {devices.current.map(device => Device(device, () => openPopup(device), FindDevice))}
      <Popup open={popupOpen} onClose={closeModal} modal closeOnDocumentClick={false}>
        <PopUp_DeviceConfiguration device={selectedDevice} cb={cb} cbOnClose={OnDeviceConfigurationUpdate} /> 
      </Popup>
    </div>
  );
}
