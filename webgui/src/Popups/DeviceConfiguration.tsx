import './DeviceConfiguration.css'

import { IDeviceRoute, IDeviceScheduleEntry, PreviewMode } from '../Data/JsonTypes';
import cloneDeep from 'lodash/cloneDeep';
import { useEffect, useState } from 'react';
import { DeviceData } from '../Data/DeviceData';
import Popup from 'reactjs-popup';
import { IRoutingOptionsCallbacks, DeviceRoutingOptions } from '../Widgets/DeviceRoutingOptions';
import { DeviceScheduleOptions } from '../Widgets/DeviceScheduleOptions';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import { DeviceGeneralOptions } from '../Widgets/DeviceGeneralOptions';

export interface DeviceConfigurationProps {
  isOpen: boolean;
  devData: DeviceData | null;
  routingCallbacks: IRoutingOptionsCallbacks;
  onClose: (devData: DeviceData | null) => void;
  onOptionsPreview: (devData: DeviceData | null, value: string, previewMode: PreviewMode) => void;
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

  function OnOptionsPreview(value: string, previewMode: PreviewMode) {
    props.onOptionsPreview(devData, value, previewMode);
  }

  function OnServiceOptionChange(data: string[]) {
    setDevData({
      ...devData,
      ServiceOptions: {
        ...devData!.ServiceOptions,
        Values: data,
      }
    } as DeviceData);
  }

  if(devData === null)
    return ("");

  const device = devData.Device;
  const serviceOptions = devData.ServiceOptions;
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
            <DeviceGeneralOptions
              device={device}
              onOptionsPreview={OnOptionsPreview}
              serviceOptions={serviceOptions} 
              onChange={OnServiceOptionChange} />
          </TabPanel>
          <TabPanel className='Routing'>
            <DeviceRoutingOptions
              devData={devData}
              cb={props.routingCallbacks}
              setDeviceRoute={UpdateDeviceRoute} />
          </TabPanel>
          <TabPanel className='Schedule'>
            <DeviceScheduleOptions
              onActionOptionsPreview={OnOptionsPreview}
              devData={devData}
              setSchedule={UpdateDeviceSchedule}
              onChildOpenChanged={(val) => setCloseOnEscape(!val)} />
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
