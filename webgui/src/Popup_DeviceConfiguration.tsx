import './Popup_DeviceConfiguration.css'
import { IDevice } from "./Device";

function RoutingOptions(prop: {device: IDevice, routingOptions: string | null}) {
  if(prop.routingOptions == null)
    return (<div className='routingOptions'>Loading from server...</div>);

  //const routing = device.Routing;
  return (
    <div className='routingOptions'>
      {prop.routingOptions}
    </div>
  );
}

export function PopUp_DeviceConfiguration(device: IDevice | null) {
  if(device === null)
    return ("Error: No device selected.");

  const routingOptions = null;
  return (
    <div className='Popup.DeviceConfiguration'>
      <div className='title'>{device.Name} [{device.Address}]</div>
      <div className='content'>
        <label className='label'>Friendly Name:</label>
        <input className='friendlyname' type='text' value={device.Name} onChange={(e) => device.Name = e.target.value} />
        <div className='routing'>
          <label className='label'>Routing:</label>
          <RoutingOptions device={device} routingOptions={routingOptions} />
        </div>
      </div>
    
    </div>
  );
}
