import './DeviceList.css'

import { useEffect, useState } from "react";
import { useWebSocketContext } from "./WebSocketContext";
import { Device, IDevice, IDeviceStatus } from "./Device";

export function DeviceList() {
  const { sendMessage, lastMessage } = useWebSocketContext();
  const [devices, setDevices] = useState<IDevice[]>([]);  
    useEffect(() => {
      function handleDeviceStatus(deviceStatus: IDeviceStatus) {
        const device = devices.find(device => device.Address === deviceStatus.Address);
        if(device)
          device.Status = deviceStatus;
      }
  
      if(lastMessage == undefined)
        return

      try {
        const data = JSON.parse(lastMessage.data)
        if(data["Devices"])
          setDevices(data["Devices"]);
        if(data["DeviceStatus"])
          handleDeviceStatus(data["DeviceStatus"]);
      } catch (error) {
        console.log(`Could not parse data '${lastMessage.data}'`)
        console.log(`Error message: ${error}`)
      }
    }, [devices, lastMessage]);

  return (
    <div className='DeviceList'>
      <div className='Heading'>
        <div className='NameAddress'>Name / Address</div>
        <div className='VendorModel'>Vendor / Model</div>
        <div className='Description'>Description</div>
        <div className='Status'>Status</div>
      </div>
      {devices.map(device => Device(device))}
    </div>
  );
}

