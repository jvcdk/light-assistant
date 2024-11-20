import './DeviceList.css'
import { useCallback, useEffect, useReducer, useRef, useState } from "react";
import { useWebSocketContext } from "../WebSocketContext";
import { Device } from "../Widgets/Device";
import { ClientToServerMessage, DeviceConfigurationChange, IDevice, IDeviceRouting, IDeviceRoutingOptions, IDeviceStatus, IScheduleActionOptions, IServerToClientMessage } from '../Data/JsonTypes';
import { DeviceData, FindDeviceDataType } from '../Data/DeviceData';
import { GetTargetFunctionalityOptionsType, GetTargetRoutingOptionsType, IRoutingOptionsCallbacks, TargetAddressToNameType } from '../Widgets/DeviceRoutingOptions';
import { DeviceConfiguration } from '../Popups/DeviceConfiguration';

export function DeviceList() {
  const [, forceUpdate] = useReducer(x => x + 1, 0);
  const { sendJsonMessage, lastJsonMessage } = useWebSocketContext();
  const [_deviceData, setDeviceData] = useState<DeviceData[]>([]);
  const deviceData = useRef(_deviceData);

  const [selectedDeviceData, setSelectedDeviceData] = useState<DeviceData | null>(null);
  const [popupOpen, setPopupOpen] = useState(false);

  const openPopup = (device: DeviceData) => {
    setSelectedDeviceData(device);
    setPopupOpen(true);
  }
  const FindDeviceData: FindDeviceDataType = useCallback((address: string, issueWarningNotFound: boolean = true): DeviceData | undefined => {
    const result = deviceData.current.find(entry => entry.Device.Address === address);
    if(result == undefined && issueWarningNotFound)
      console.log(`Warning: Searched for device ${address} but did not find it.`)

    return result;
  }, [deviceData]);

  const GetRoutingOptions: GetTargetRoutingOptionsType = useCallback((eventType: string | undefined) : string[] | undefined => {
    if(eventType == undefined)
      return undefined;

    const eligibleDevices = _deviceData.filter(entry => entry.RoutingOptions?.ConsumableEvents.some(ev => ev.EventType == eventType));
    return eligibleDevices.map(entry => entry.Device.Address);
  }, [_deviceData]);

  const GetTargetFunctionalityOptions: GetTargetFunctionalityOptionsType = useCallback((eventType: string | undefined, targetAddress: string | undefined) => {
    const paramsOk = targetAddress && eventType;
    if(!paramsOk)
      return undefined;

    const devData = FindDeviceData(targetAddress);
    if(devData == undefined)
      return undefined;

    const consumableEvents = devData.RoutingOptions?.ConsumableEvents;
    if(consumableEvents == undefined)
      return undefined;

    return consumableEvents.filter(ev => ev.EventType == eventType).map(ev => ev.TargetName);
  }, [FindDeviceData]);

  const TargetAddressToName: TargetAddressToNameType = useCallback((targetName: string) => {
    let devData = null;
    if(targetName)
      devData = FindDeviceData(targetName);

    return devData?.Device.Name || "<unknown>";
  }, [FindDeviceData]);

  useEffect(() => {
    function handleDeviceStatus(deviceStatus: IDeviceStatus) {
      const devData = FindDeviceData(deviceStatus.Address);
      if (devData) {
        devData.Status = deviceStatus;
        forceUpdate();
      }
    }

    function handleDeviceRouting(deviceRouting: IDeviceRouting) {
      const devData = FindDeviceData(deviceRouting.Address);
      if (devData) {
        devData.Routing = deviceRouting.Routing || [];
        forceUpdate();
      }
    }

    function handleDeviceList(deviceList: IDevice[]) {
      deviceData.current = deviceList.map(entry => {
        let result = FindDeviceData(entry.Address, false);
        if(result)
          result.Device = entry;
        else
          result = new DeviceData(entry);

        return result;
      });
      setDeviceData(deviceData.current);
    }

    function handleDeviceRoutingOptions(deviceRoutingOptions: IDeviceRoutingOptions) {
      const devData = FindDeviceData(deviceRoutingOptions.Address);
      if (devData)
        devData.RoutingOptions = deviceRoutingOptions;
    }

    function handleScheduleActionOptions(scheduleActionOptions: IScheduleActionOptions) {
      const devData = FindDeviceData(scheduleActionOptions.Address);
      if (devData) {
        devData.ConsumableActions = scheduleActionOptions.ConsumableActions;
        forceUpdate();
      }
    }

    if (lastJsonMessage == undefined)
      return

    try {
      const message = lastJsonMessage as IServerToClientMessage;
      if (message.Devices)
        handleDeviceList(message.Devices);
      if (message.DeviceStatus)
        handleDeviceStatus(message.DeviceStatus);
      if (message.Routing)
        handleDeviceRouting(message.Routing);
      if(message.RoutingOptions)
        handleDeviceRoutingOptions(message.RoutingOptions);
      if (message.ScheduleActionOptions)
        handleScheduleActionOptions(message.ScheduleActionOptions);
    } catch (error) {
      console.log(`Error message: ${error}`)
    }
  }, [FindDeviceData, lastJsonMessage]);

  const routingCallbacks = {
    GetTargetRoutingOptions: GetRoutingOptions,
    GetTargetFunctionalityOptions: GetTargetFunctionalityOptions,
    TargetAddressToName: TargetAddressToName,
  } as IRoutingOptionsCallbacks;

  function OnDeviceConfigurationUpdate(devData: DeviceData | null) {
    setSelectedDeviceData(null);
    setPopupOpen(false);
    if(devData == null)
      return;

    const device = devData.Device;
    const msg = new ClientToServerMessage();
    msg.DeviceConfigurationChange = new DeviceConfigurationChange(device.Address, device.Name, devData.Routing, devData.Schedule);
    sendJsonMessage(msg);
  }

  return (
    <div className='DeviceList'>
      <div className='Heading'>
        <div className='NameAddress'>Name / Address</div>
        <div className='Status'>Status</div>
        <div className='Routing'>Routing</div>
        <div className='Schedule'>Schedule</div>
      </div>
      {deviceData.current.map(device => Device(device, () => openPopup(device), FindDeviceData))}
      <DeviceConfiguration isOpen={popupOpen} devData={selectedDeviceData} routingCallbacks={routingCallbacks} onClose={OnDeviceConfigurationUpdate} /> 
    </div>
  );
}
