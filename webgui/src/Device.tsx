import './Device.css'

/**
 * Should match JsonDevice
 */
export interface IDevice {
  Name : string;
  Address : string;
  Vendor : string;
  Model : string;
  Description : string;
}

export function Device(device: IDevice) {
  return (
    <div className='Device' key={device.Address}>
      <div className='NameAddress'>
        <div className='Name'>{device.Name}</div>
        <div className='Address'>{device.Address}</div>
      </div>
      <div className='VendorModel'>
        <div className='Vendor'>{device.Vendor}</div>
        <div className='Model'>{device.Model}</div>
      </div>
      <div className='Description'>{device.Description}</div>
    </div>
  );
}
