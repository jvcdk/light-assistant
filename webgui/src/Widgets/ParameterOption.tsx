import ParamKnob from '../image/param_knob_2.svg';
import { useEffect, useRef, useState } from "react";
import { IParamEnum, IParamFloat, IParamInfo, IParamInt } from "../Data/JsonTypes";
import './ParameterOption.css';
import { tryParseFloat, tryParseInt } from '../Utils/NumberUtils';

const MouseDialScaling = 5;

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
  }, [props.value, param.Default]);

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
  const [value, setValue] = useState(tryParseFloat(props.value, param.Default));
  const inputRef = useRef<HTMLInputElement>(null);

  const stepSize = (param.Max - param.Min) / 100;
  const units = props.param.Units;

  useEffect(() => {
    setValue(tryParseFloat(props.value, param.Default));
  }, [props.value, param.Default]);

  function update(newValStr: string) {
    const newVal = tryParseFloat(newValStr, value);
    updateValue(newVal);
    return newVal;
  }

  function updateValue(value: number) {
    value = Math.max(param.Min, Math.min(param.Max, value));
    props.onChange(value.toString());
  }

  function handleMouseDown(event: React.MouseEvent) {
    if(inputRef.current === null)
      return;

    event.preventDefault();

    const startValue = update(inputRef.current.value);
    const startY = event.clientY;

    function onMouseMove(moveEvent: MouseEvent) {
      const deltaY = Math.round((startY - moveEvent.clientY) / MouseDialScaling);
      let newValue = startValue + deltaY * stepSize;
      newValue = Math.round(newValue / stepSize) * stepSize;
      updateValue(newValue);
      if(inputRef.current !== null)
        inputRef.current.value = newValue.toString();
      }

    function onMouseUp() {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    }

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }

  return (
    <>
      <label className="Param Float">{param.Name}{units && ` [${units}]`}:</label>
      <span className="Param Float">
        <input ref={inputRef} type="text" defaultValue={value} onBlur={e => update(e.target.value)} />
        <span onMouseDown={handleMouseDown}><ParamKnob /></span>
      </span>
    </>
  );
}

function ParamOptionInt(props: ParamOptionProps) {
  const param = props.param as IParamInt;
  const [value, setValue] = useState(tryParseInt(props.value, param.Default));
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    setValue(tryParseInt(props.value, param.Default));
  }, [props, param.Default]);

  function update(newValStr: string) {
    const newVal = tryParseInt(newValStr, value);
    updateValue(newVal);
    return newVal;
  }

  function updateValue(value: number) {
    value = Math.max(param.Min, Math.min(param.Max, value));
    props.onChange(value.toString());
  }

  const units = props.param.Units;

  function handleMouseDown(event: React.MouseEvent) {
    if(inputRef.current === null)
      return;

    event.preventDefault();

    const startValue = update(inputRef.current.value);
    const startY = event.clientY;

    function onMouseMove(moveEvent: MouseEvent) {
      const deltaY = Math.round((startY - moveEvent.clientY) / MouseDialScaling);
      let newValue = startValue + deltaY;
      newValue = Math.round(newValue);
      updateValue(newValue);
      if(inputRef.current !== null)
        inputRef.current.value = newValue.toString();
    }

    function onMouseUp() {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    }

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }

  return (
    <>
      <label className="Param Int">{param.Name}{units && ` [${units}]`}:</label>
      <span className="Param Int">
        <input ref={inputRef} type="text" defaultValue={value} onBlur={e => update(e.target.value)} />
        <span onMouseDown={handleMouseDown}><ParamKnob /></span>
      </span>
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
