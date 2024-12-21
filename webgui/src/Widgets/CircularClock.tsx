import React, { useState, useEffect, useCallback } from 'react';
import { TimeOfDay } from '../Data/ScheduleTrigger';
import { RenderTimeOfDay } from './RenderTimeOfDay';
import './CircularClock.css';
import { State } from '../Utils/State';

export enum ClockMode {
  Hour,
  Minute
}

function ClockDigits(props: { Mode: ClockMode, SelectedDigit: number }) {
  let nElements = 60/5;
  let stepSize = 5;
  let angularStepSize = 6;
  let innerBoundary = 60; // No inner boundary for minutes
  if(props.Mode == ClockMode.Hour) {
    nElements = 24;
    stepSize = 1;
    angularStepSize = 30;
    innerBoundary = 12;
  }

  const selfSize = 15; // Units: em. Should match the CSS value in CircularClock.css
  return Array.from({ length: nElements }).map((_, index) => {
    index = index * stepSize;

    const isInner = index >= innerBoundary;
    const radius = isInner ? selfSize*0.3125 : selfSize*0.4125;
    let className;
    if(props.Mode == ClockMode.Hour)
      className = isInner ? 'HoursPm' : 'HoursAm';
    else
      className = 'Minutes';
    const classSelected = index === props.SelectedDigit ? ' selected' : '';

    const angle = (index * angularStepSize) * (Math.PI / 180);
    const x = radius * Math.sin(angle);
    const y = -radius * Math.cos(angle);
    return (
      <div className={`ClockDigit ${className}${classSelected}`} key={index} style={{ position: 'absolute', left: `50%`, top: `50%`, transform: `translate(-50%, -50%) translate(${x}em, ${y}em)` }}>
        {index}
      </div>
    );
  });
}

export interface CircularClockProps {
  initialTime: TimeOfDay;
  onTimeChange: (time: TimeOfDay) => void;
  mode: ClockMode
}

export function CircularClock(props: CircularClockProps) {
  const selectedTime = new State(useState(props.initialTime.Clone()));
  selectedTime.addListener("CircularClock", props.onTimeChange);
  const [isDragging, setIsDragging] = useState(false);
  const [mode, setMode] = useState(props.mode);

  const updateTime = useCallback((circularClock: HTMLElement, clientX: number, clientY: number) => {
    const rect = circularClock.getBoundingClientRect();
    const centerX = rect.width / 2;
    const centerY = rect.height / 2;
    const mouseX = clientX - rect.left - centerX;
    const mouseY = clientY - rect.top - centerY;
    const radius = Math.sqrt(mouseX * mouseX + mouseY * mouseY);
    if(radius < Math.abs(centerX) * 0.25 || radius > Math.abs(centerX) * 1.1)
      return; // Ignore clicks inside or outside the clock
    const angle = Math.atan2(mouseY, mouseX);
    const degrees = angle * (180 / Math.PI) + 90; // Offset for 12 o'clock
    const normalizedDegrees = (degrees + 360) % 360;
  
    if (mode == ClockMode.Hour) {
      let hour = Math.round((normalizedDegrees / 30) % 12);
      if(radius < centerX * 0.75)
        hour += 12;
      selectedTime.val.Hour = hour;
    } else {
      const minute = Math.round((normalizedDegrees / 6) % 60);
      selectedTime.val.Minute = minute;
    }
  }, [mode, selectedTime.val]);

  const handleMouseEvent = useCallback((e: MouseEvent) => {
    const circularClock = (e.target as HTMLElement).closest('.CircularClock') as HTMLElement;
    if(!circularClock)
      return;

    updateTime(circularClock, e.clientX, e.clientY);
  }, [updateTime]);

  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); // Prevent default text selection
    setIsDragging(true);
    handleMouseEvent(e.nativeEvent as MouseEvent);
  }, [handleMouseEvent]);

  const handleMouseMove = useCallback((e: MouseEvent) => {
    if (!isDragging)
      return;

    handleMouseEvent(e);
  }, [isDragging, handleMouseEvent]);

  const handleMouseUp = useCallback(() => {
    if(isDragging)
      setMode(ClockMode.Minute);
    setIsDragging(false);
  }, [isDragging]);

  useEffect(() => {
    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
  }, [handleMouseMove, handleMouseUp, isDragging]);

  const isModeHour = mode == ClockMode.Hour;
  const handClass = (isModeHour && selectedTime.val.Hour >= 12) ? 'Hand Inner' : 'Hand Outer';
  const handAngle = (isModeHour ? selectedTime.val.Hour * 30 : selectedTime.val.Minute * 6) + 90;
  const selectedDigit = isModeHour ? selectedTime.val.Hour : selectedTime.val.Minute;

  return (
    <div>
      <div className="CircularClock" onMouseDown={handleMouseDown}>
        <div className={handClass} style={{ transform: `translate(-50%, -50%) rotate(${handAngle}deg)` }} />
        <ClockDigits Mode={mode} SelectedDigit={selectedDigit} />
        <span className='CenterTime'>
          <RenderTimeOfDay selected={mode} Time={selectedTime.val} OnMinutesClick={() => setMode(ClockMode.Minute)} OnHourClick={() => setMode(ClockMode.Hour)} />
        </span>
      </div>
    </div>
  );
}
