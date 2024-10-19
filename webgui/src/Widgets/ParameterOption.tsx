import { IParamEnum, IParamFloat, IParamInfo } from "../Data/JsonTypes";
import { tryParseFloat } from "../Utils/FloatUtils";
import './ParameterOption.css';

export interface ParamOptionProps {
  param: IParamInfo;
  value: string | undefined;
  onChange: (value: string | undefined) => void;
}

function ParamOptionEnum(props: ParamOptionProps) {
  const param = props.param as IParamEnum;
  const value = props.value || param.Default;
  return (
    <span className="Param Enum">
      <label>{param.Name}</label>
      <select onChange={e => props.onChange(e?.target.value)} value={value}>
        {param.Values.map(value => <option key={value} value={value}>{value}</option>)}
      </select>
    </span>
  );
}

function ParamOptionFloat(props: ParamOptionProps) {
  const param = props.param as IParamFloat;
  const value = tryParseFloat(props.value, param.Default);
  return (
    <span className="Param Float">
      <label>{param.Name}</label>
      <input type="number" min={param.Min} max={param.Max} value={value} onChange={e => props.onChange(e.target.value)} />
    </span>
  );
}

export function ParamOption(props: ParamOptionProps) {
  const param = props.param;
  switch (param.Type) {
    case 'enum':
      return <ParamOptionEnum {...props} />;
    case 'float':
    case 'brightness': // For now, just use the float option
      return <ParamOptionFloat {...props} />;
    default:
      return <div>Unknown type: {param.Type}</div>;
  }
}
