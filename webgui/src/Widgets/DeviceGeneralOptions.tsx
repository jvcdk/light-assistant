import { IDevice, IServiceOptions, PreviewMode } from "../Data/JsonTypes";
import { safeGetValue } from "../Utils/ArrayUtils";
import { ParamOption } from "./ParameterOption";

export interface DeviceGeneralOptionsProps {
  device: IDevice;
  serviceOptions: IServiceOptions | undefined;
  onOptionsPreview: (value: string, previewMode: PreviewMode) => void;
  onChange: (data: string[]) => void;
}


export function DeviceGeneralOptions(prop: DeviceGeneralOptionsProps) {
  const { device, serviceOptions, onOptionsPreview, onChange } = prop;

  function OnValueChange(idx: number, value: string | undefined) {
    if(idx < 0 || idx >= values.length)
      return;
    if(value === undefined)
      return;

    const newValue = values.slice();
    newValue[idx] = value;
    onChange(newValue);
  }

  function OnPreview(value: string, previewMode: PreviewMode) {
    if(previewMode == 'None')
      return;

    if(value == 'None')
      onOptionsPreview('', 'None');
    else
      onOptionsPreview(value, previewMode);
  }

  const params = serviceOptions?.Params || [];
  const values = serviceOptions?.Values || [];
  return (
    <div className='Grid'>
      <label>Vendor:</label><span>{device.Vendor}</span>
      <label>Model:</label><span>{device.Model}</span>
      <label>Description:</label><span>{device.Description}</span>
      <label>Friendly name:</label>
      <input className='FriendlyName' type='text' defaultValue={device.Name} onChange={(e) => device.Name = e.target.value} />
      {params.map((option, idx) =>
        <ParamOption
          key={idx}
          param={option}
          value={safeGetValue(values, idx)}
          onChange={(value) => OnValueChange(idx, value)}
          onPreview={(value) => OnPreview(value, option.PreviewMode)} />)}
    </div>
  );
}
