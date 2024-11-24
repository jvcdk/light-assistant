import { useEffect } from "react";
import { IParamEnum, IParamFloat, IParamInfo, IParamInt } from "../Data/JsonTypes";
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
    <>
      <label className="Param Enum">{param.Name}:</label>
      <select className="Param Enum" onChange={e => props.onChange(e?.target.value)} value={props.value}>
        {param.Values.map(value => <option key={value} value={value}>{value}</option>)}
      </select>
    </>
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

  const stepSize = (param.Max - param.Min) / 100;
  const units = props.param.Units;
  return (
    <>
      <label className="Param Float">{param.Name}{units && ` [${units}]`}:</label>
      <input className="Param Float" type="text" min={param.Min} step={stepSize} max={param.Max} value={props.value} onChange={e => update(e.target.value)} />
    </>
  );
}

function ParamOptionInt(props: ParamOptionProps) {
  const param = props.param as IParamInt;

  useEffect(() => {
    if(props.value === undefined)
      props.onChange(param.Default.toString());
  }, [props, param.Default]);

  function update(value: string) {
    const newVal = parseInt(value);
    if(!isNaN(newVal))
      props.onChange(value);
  }

  const units = props.param.Units;
  return (
    <>
      <label className="Param Int">{param.Name}{units && ` [${units}]`}:</label>
      <input className="Param Int" type="text" min={param.Min} max={param.Max} value={props.value} onChange={e => update(e.target.value)} />
    </>
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
    case 'int':
      return <ParamOptionInt {...props} />;
    default:
      return <div>Unknown type: {param.Type}</div>;
  }
}
