import './DeviceList.css'
import Popup from 'reactjs-popup';
import { useEffect, useState } from "react";
import { useWebSocketContext } from "./WebSocketContext";
import { Device, IDevice, IDeviceRouting, IDeviceStatus } from "./Device";

export function DeviceList() {
  const { sendMessage, lastMessage } = useWebSocketContext();
  const [devices, setDevices] = useState<IDevice[]>([]);

  const [selectedDevice, setSelectedDevice] = useState<IDevice | null>(null);
  const [popupOpen, setPopupOpen] = useState(false);

  const openPopup = (device: IDevice) => {
    console.log(device);
    setSelectedDevice(device);
    setPopupOpen(true);
  }
  const closeModal = () => setPopupOpen(false);

  useEffect(() => {
    function handleDeviceStatus(deviceStatus: IDeviceStatus) {
      const device = devices.find(device => device.Address === deviceStatus.Address);
      if(device)
        device.Status = deviceStatus;
    }

    function handleDeviceRouting(deviceRouting: IDeviceRouting) {
      const device = devices.find(device => device.Address === deviceRouting.Address);
      if(device)
        device.Routing = deviceRouting.Routing;
    }

    if(lastMessage == undefined)
      return

    try {
      const data = JSON.parse(lastMessage.data)
      if(data["Devices"])
        setDevices(data["Devices"]);
      if(data["DeviceStatus"])
        handleDeviceStatus(data["DeviceStatus"]);
      if(data["Routing"])
        handleDeviceRouting(data["Routing"]);
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
        <div className='Routing'>Routing</div>
      </div>
      {devices.map(device => Device(device, () => openPopup(device)))}
      <Popup open={popupOpen} onClose={closeModal} modal>
        {selectedDevice && selectedDevice.Name}
      </Popup>
    </div>
  );
}

