import ParamKnob from '../image/param_knob_2.svg';
import { useCallback, useEffect, useRef, useState } from "react";
import { IParamEnum, IParamFloat, IParamInfo, IParamInt, PreviewMode } from "../Data/JsonTypes";
import './ParameterOption.css';
import { tryParseFloat, tryParseInt } from '../Utils/NumberUtils';

const MouseDialScaling = 5;

export interface ParamOptionProps {
  param: IParamInfo;
  value: string | undefined;
  previewMode: PreviewMode;
  onChange: (value: string | undefined) => void;
  onPreview: (value: string, previewMode: PreviewMode) => void;
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

function ParamOptionNumber(props: ParamOptionProps, isFloat: boolean) {
  const param = props.param as IParamFloat | IParamInt;
  const parseFn = isFloat ? tryParseFloat : tryParseInt;
  const [value, setValue] = useState(parseFn(props.value, param.Default));
  const inputRef = useRef<HTMLInputElement>(null);
  const spanRef = useRef<HTMLSpanElement>(null);
  const sendPreview = props.previewMode != "None";

  useEffect(() => {
    if(props.value === undefined) {
      props.onChange(param.Default.toString());
      return;
    }

    setValue(parseFn(props.value, param.Default));
  }, [props, param.Default, parseFn]);

  const updateValue = useCallback((value: number) => {
    value = Math.max(param.Min, Math.min(param.Max, value));
    const valStr = value.toString();value.toString()
    props.onChange(valStr);
    if(sendPreview)
      props.onPreview(valStr, props.previewMode);
  }, [param, props, sendPreview]);

  const update = useCallback((newValStr: string) => {
    const newVal = parseFn(newValStr, value);
    updateValue(newVal);
    return newVal;
  }, [value, updateValue, parseFn]);

  const handleMove = useCallback((startValue: number, startY: number, stepSize: number, clientY: number) => {
    const deltaY = Math.round((startY - clientY) / MouseDialScaling);
    let newValue = startValue + deltaY * stepSize;
    newValue = Math.round(newValue / stepSize) * stepSize;
    updateValue(newValue);
    if(inputRef.current !== null)
      inputRef.current.value = newValue.toString();
  }, [updateValue]);

  useEffect(() => {
    const stepSize = isFloat ? (param.Max - param.Min) / 100 : 1;

    function handleMouseDown(event: MouseEvent) {
      if (inputRef.current === null)
        return;

      event.preventDefault();

      const startValue = update(inputRef.current.value);
      const startY = event.clientY;

      function onMouseMove(moveEvent: MouseEvent) {
        handleMove(startValue, startY, stepSize, moveEvent.clientY);
      }

      function onMouseUp() {
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        if(sendPreview)
          props.onPreview("", "None");
      }

      document.addEventListener('mousemove', onMouseMove);
      document.addEventListener('mouseup', onMouseUp);
    }

    function handleTouchStart(event: TouchEvent) {
      if(inputRef.current === null || spanRef.current === null)
        return;

      event.preventDefault();

      const startValue = update(inputRef.current.value);
      const startY = event.touches[0].clientY;

      const thisSpan = spanRef.current;
      function onTouchEnd() {
        thisSpan.ontouchmove = null;
        thisSpan.ontouchend = null;
        if(sendPreview)
          props.onPreview("", "None");
      }

      spanRef.current.ontouchmove = (e) => handleMove(startValue, startY, stepSize, e.touches[0].clientY);
      spanRef.current.ontouchend = () => onTouchEnd();
    }

    if(spanRef.current !== null) {
      spanRef.current.onmousedown = (e) => handleMouseDown(e);
      spanRef.current.ontouchstart = (e) => handleTouchStart(e);
    }
  }, [param, value, props, update, updateValue, isFloat, handleMove, sendPreview]);

  const units = props.param.Units;
  const classType = isFloat ? 'Float' : 'Int';
  return (
    <>
      <label className={`Param ${classType}`}>{param.Name}{units && ` [${units}]`}:</label>
      <span className={`Param ${classType}`}>
        <input ref={inputRef} type="text" defaultValue={value} onBlur={e => update(e.target.value)} />
        <span ref={spanRef}><ParamKnob /></span>
      </span>
    </>
  );
}

function ParamOptionFloat(props: ParamOptionProps) {
  return ParamOptionNumber(props, /* isFloat */ true);
}

function ParamOptionInt(props: ParamOptionProps) {
  return ParamOptionNumber(props, /* isFloat */ false);
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
