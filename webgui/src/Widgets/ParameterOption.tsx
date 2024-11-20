import { useEffect } from "react";
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

  useEffect(() => {
    if(props.value === undefined)
      props.onChange(param.Default);
  }, [props, param.Default]);

  return (
    <span className="Param Enum">
      <label>{param.Name}</label>
      <select onChange={e => props.onChange(e?.target.value)} value={props.value}>
        {param.Values.map(value => <option key={value} value={value}>{value}</option>)}
      </select>
    </span>
  );
}

function ParamOptionFloat(props: ParamOptionProps) {
  const param = props.param as IParamFloat;

  useEffect(() => {
    if(props.value === undefined)
      props.onChange(param.Default.toString());
  }, [props, param.Default]);

  function update(value: string) {
    const newVal = parseFloat(value);
    if(!isNaN(newVal))
      props.onChange(value);
  }

  const value = tryParseFloat(props.value, param.Default);
  return (
    <span className="Param Float">
      <label>{param.Name}</label>
      <input type="number" min={param.Min} max={param.Max} value={value} onChange={e => update(e.target.value)} />
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
