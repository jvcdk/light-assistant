import './DeviceConfiguration.css'

import { IDeviceRoute, IDeviceScheduleEntry } from '../Data/JsonTypes';
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';
import { DeviceData } from '../Data/DeviceData';
import Popup from 'reactjs-popup';
import { IRoutingOptionsCallbacks, DeviceRoutingOptions } from '../Widgets/DeviceRoutingOptions';
import { DeviceScheduleOptions } from '../Widgets/DeviceScheduleOptions';

export interface DeviceConfigurationProps {
  isOpen: boolean;
  devData: DeviceData | null;
  routingCallbacks: IRoutingOptionsCallbacks;
  onClose: (devData: DeviceData | null) => void;
}
export function DeviceConfiguration(props: DeviceConfigurationProps) {
  const [devData, setDevData] = useState<DeviceData|null>(null);
  const [closeOnEscape, setCloseOnEscape] = useState(true);

  useEffect(() => {
    setDevData(cloneDeep(props.devData));
  }, [props.devData]);

  function UpdateDeviceRoute(route: IDeviceRoute[]) {
    setDevData({
      ...devData,
      Routing: route,
    } as DeviceData);
  }

  function UpdateDeviceSchedule(schedule: IDeviceScheduleEntry[]) {
    setDevData({
      ...devData,
      Schedule: schedule,
    } as DeviceData);
  }

  if(devData === null)
    return ("");

  const device = devData.Device;
  return (
    <Popup closeOnEscape={closeOnEscape} open={props.isOpen} onClose={() => props.onClose(null)} modal closeOnDocumentClick={false}>
      <div className='DeviceConfiguration'>
        <div className='Title'>{device.Vendor} – {device.Model}</div>
        <div className='SubTitle'>{device.Description}</div>
        <div className='SubTitle'>Address: {device.Address}</div>
        <div className='Content'>
          <label className='Label'>Friendly name:</label>
          <input className='FriendlyName' type='text' defaultValue={device.Name} onChange={(e) => device.Name = e.target.value} />
          <div className='Routing'>
            <label className='Label'>Routing:</label>
            <DeviceRoutingOptions devData={devData} cb={props.routingCallbacks} setDeviceRoute={UpdateDeviceRoute} />
          </div>
          <div className='Schedule'>
            <label className='Label'>Schedule:</label>
            <DeviceScheduleOptions devData={devData} setSchedule={UpdateDeviceSchedule} onChildOpenChanged={(val) => setCloseOnEscape(!val)} />
          </div>
        <div className='Buttons'>
          <input className='Cancel' type='button' onClick={() => props.onClose(null)} value="Cancel" />
          <input className='Ok' type='button' onClick={() => props.onClose(devData)} value="Apply" />
        </div>
        </div>
      </div>
    </Popup>
  );
}
