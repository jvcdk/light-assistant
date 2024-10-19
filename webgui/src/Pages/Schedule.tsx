import { useCallback, useEffect, useReducer, useRef, useState } from 'react';
import './Schedule.css'
import { ScheduleRule, ScheduleRuleEdit } from '../Popups/ScheduleRuleEdit';
import { useWebSocketContext } from '../WebSocketContext';
import { IDevice, IScheduleTriggerOptions, IServerToClientMessage } from '../Data/JsonTypes';
import { DeviceData2, FindDeviceData2Type } from '../Data/DeviceData';

export function Schedule() {
  const [, forceUpdate] = useReducer(x => x + 1, 0);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { sendJsonMessage, lastJsonMessage } = useWebSocketContext();
  const [_deviceData, setDeviceData] = useState<DeviceData2[]>([]);
  const deviceData = useRef(_deviceData); // TODO: Could we do without this one?
  const [selectedRule, setSelectedRule] = useState<ScheduleRule>(new ScheduleRule());
  const [popupOpen, setPopupOpen] = useState(false);

  const openPopup = () => setPopupOpen(true);
  const closePopup = () => setPopupOpen(false);

  function addRule() {
    setSelectedRule(new ScheduleRule());
    openPopup();
  }

  const FindDeviceData: FindDeviceData2Type = useCallback((address: string, issueWarningNotFound: boolean = true): DeviceData2 | undefined => {
    const result = deviceData.current.find(entry => entry.Device.Address === address);
    if(result == undefined && issueWarningNotFound)
      console.log(`Warning: Searched for device ${address} but did not find it.`)

    return result;
  }, [deviceData]);

  useEffect(() => {
    function handleDeviceList(deviceList: IDevice[]) {
      deviceData.current = deviceList.map(entry => {
        let result = FindDeviceData(entry.Address, false);
        if(result)
          result.Device = entry;
        else
          result = new DeviceData2(entry);

        return result;
      });
      setDeviceData(deviceData.current);
    }

    function handleScheduleTriggerOptions(scheduleTriggerOptions: IScheduleTriggerOptions) {
      const devData = FindDeviceData(scheduleTriggerOptions.Address);
      if (devData) {
        devData.ConsumableTriggers = scheduleTriggerOptions.ConsumableTriggers;
        forceUpdate();
      }
    }
    
    
    if (lastJsonMessage == undefined)
      return

    try {
      const message = lastJsonMessage as IServerToClientMessage;
      if (message.Devices)
        handleDeviceList(message.Devices);
      if (message.ScheduleTriggerOptions)
        handleScheduleTriggerOptions(message.ScheduleTriggerOptions);
    } catch (error) {
      console.log(`Error message: ${error}`)
    }

  }, [FindDeviceData, lastJsonMessage]);

  return (
    <div className='Schedule'>
      <div>No schedule rules defined.</div>
      <input type='button' value='Add Rule' onClick={addRule} />
      <ScheduleRuleEdit isOpen={popupOpen} onClose={closePopup} rule={selectedRule} devData={deviceData.current} /> 
    </div>
  );
}
