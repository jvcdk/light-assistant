import './DeviceConfiguration.css'

import { IDeviceRoute, IDeviceScheduleEntry, PreviewMode } from '../Data/JsonTypes';
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';
import { DeviceData } from '../Data/DeviceData';
import Popup from 'reactjs-popup';
import { IRoutingOptionsCallbacks, DeviceRoutingOptions } from '../Widgets/DeviceRoutingOptions';
import { DeviceScheduleOptions } from '../Widgets/DeviceScheduleOptions';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';

export interface DeviceConfigurationProps {
  isOpen: boolean;
  devData: DeviceData | null;
  routingCallbacks: IRoutingOptionsCallbacks;
  onClose: (devData: DeviceData | null) => void;
  onActionOptionsPreview: (devData: DeviceData | null, value: string, previewMode: PreviewMode) => void;
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

  function OnActionOptionsPreview(value: string, previewMode: PreviewMode) {
    props.onActionOptionsPreview(devData, value, previewMode);
  }

  if(devData === null)
    return ("");

  const device = devData.Device;
  return (
    <Popup closeOnEscape={closeOnEscape} open={props.isOpen} onClose={() => props.onClose(null)} modal closeOnDocumentClick={false}>
      <div className='DeviceConfiguration'>
      <div className='Title'>{device.Name}</div>
        <div className='SubTitle'>{device.Address}</div>
        <Tabs>
          <TabList>
            <Tab>General</Tab>
            <Tab>Routing</Tab>
            <Tab>Schedule</Tab>
          </TabList>
          <TabPanel>
            <div className='Grid'>
              <label>Vendor:</label><span>{device.Vendor}</span>
              <label>Model:</label><span>{device.Model}</span>
              <label>Description:</label><span>{device.Description}</span>
              <label>Friendly name:</label>
              <input className='FriendlyName' type='text' defaultValue={device.Name} onChange={(e) => device.Name = e.target.value} />
            </div>
          </TabPanel>
          <TabPanel className='Routing'>
            <DeviceRoutingOptions devData={devData} cb={props.routingCallbacks} setDeviceRoute={UpdateDeviceRoute} />
          </TabPanel>
          <TabPanel className='Schedule'>
            <DeviceScheduleOptions onActionOptionsPreview={OnActionOptionsPreview} devData={devData} setSchedule={UpdateDeviceSchedule} onChildOpenChanged={(val) => setCloseOnEscape(!val)} />
          </TabPanel>
        </Tabs>
        <div className='Buttons'>
          <input className='Cancel' type='button' onClick={() => props.onClose(null)} value="Cancel" />
          <input className='Ok' type='button' onClick={() => props.onClose(devData)} value="Apply" />
        </div>
      </div>
    </Popup>
  );
}
