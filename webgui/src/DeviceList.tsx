import { useEffect, useState } from "react";
import { useWebSocketContext } from "./WebSocketContext";
import { Device, IDevice } from "./Device";

export function DeviceList() {
  const { sendMessage, lastMessage } = useWebSocketContext();
  const [devices, setDevices] = useState<IDevice[]>([]); // Use useState to manage devices

    useEffect(() => {
      if(lastMessage == undefined)
        return

      try {
        const data = JSON.parse(lastMessage.data)
        if(data["Devices"])
          setDevices(data["Devices"]);
      } catch (error) {
        console.log(`Could not parse data '${lastMessage.data}'`)
        console.log(`Error message: ${error}`)
      }
    }, [lastMessage]);

  return (
    <div className='DeviceList'>
      {devices.map(device => Device(device))}
    </div>
  );
}
