import ParamKnob from '../image/param_knob_2.svg';
import { useCallback, useEffect, useRef } from "react";
import { IParamEnum, IParamFloat, IParamInfo, IParamInt } from "../Data/JsonTypes";
import './ParameterOption.css';
import { tryParseFloat, tryParseInt } from '../Utils/NumberUtils';

const MouseDialScaling = 5;

export interface ParamOptionProps {
  param: IParamInfo;
  value: string | undefined;
  onChange: (value: string | undefined) => void;
  onPreview: (value: string) => void;
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
  const { param, value, onChange, onPreview } = props;

  const paramTyped = param as IParamFloat | IParamInt;
  const parseFn = isFloat ? tryParseFloat : tryParseInt;
  const defaultValue = value || paramTyped.Default.toString();
  const inputRef = useRef<HTMLInputElement>(null);
  const spanRef = useRef<HTMLSpanElement>(null);

  const getValue = useCallback(() => {
    const fallback = parseFn(value, paramTyped.Default);
    const parsed = parseFn(inputRef.current?.value, fallback);
    return Math.max(paramTyped.Min, Math.min(paramTyped.Max, parsed));
  }, [paramTyped, parseFn, value]);

  const updateParent = useCallback(() => {
    const newValue = getValue().toString();
    onChange(newValue);
    if(inputRef.current)
      inputRef.current.value = newValue;
  }, [getValue, onChange]);

  const handleMove = useCallback((startValue: number, startY: number, stepSize: number, clientY: number) => {
    const deltaY = Math.round((startY - clientY) / MouseDialScaling);
    let newValue = startValue + deltaY * stepSize;
    newValue = Math.round(newValue / stepSize) * stepSize;
    newValue = Math.max(paramTyped.Min, Math.min(paramTyped.Max, newValue));
    const newValueStr = newValue.toString();
    if(inputRef.current)
      inputRef.current.value = newValueStr;
    onPreview(newValueStr);
  }, [onPreview, paramTyped]);

  useEffect(() => {
    const stepSize = isFloat ? (paramTyped.Max - paramTyped.Min) / 100 : 1;

    function handleMouseDown(event: MouseEvent) {
      if (inputRef.current === null)
        return;

      event.preventDefault();

      const startValue = getValue();
      const startY = event.clientY;

      function onMouseMove(moveEvent: MouseEvent) {
        handleMove(startValue, startY, stepSize, moveEvent.clientY);
      }

      function onMouseUp() {
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        updateParent();
        onPreview('None');
      }

      document.addEventListener('mousemove', onMouseMove);
      document.addEventListener('mouseup', onMouseUp);
    }

    function handleTouchStart(event: TouchEvent) {
      if(inputRef.current === null || spanRef.current === null)
        return;

      event.preventDefault();

      const startValue = getValue();
      const startY = event.touches[0].clientY;

      const thisSpan = spanRef.current;
      function onTouchEnd() {
        thisSpan.ontouchmove = null;
        thisSpan.ontouchend = null;
        updateParent();
        onPreview('None');
      }

      spanRef.current.ontouchmove = (e) => handleMove(startValue, startY, stepSize, e.touches[0].clientY);
      spanRef.current.ontouchend = () => onTouchEnd();
    }

    if(spanRef.current !== null) {
      spanRef.current.onmousedown = (e) => handleMouseDown(e);
      spanRef.current.ontouchstart = (e) => handleTouchStart(e);
    }
  }, [getValue, handleMove, isFloat, onPreview, paramTyped, updateParent]);

  const units = props.param.Units;
  const classType = isFloat ? 'Float' : 'Int';
  return (
    <>
      <label className={`Param ${classType}`}>{param.Name}{units && ` [${units}]`}:</label>
      <span className={`Param ${classType}`}>
        <input ref={inputRef} type="text" defaultValue={defaultValue} onBlur={updateParent} />
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
