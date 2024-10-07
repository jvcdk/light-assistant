import './Widget_DaySelector.css'
import { DayName } from "./ScheduleTrigger";

function ToggleDay(days: DayName[], day: DayName) {
    if(days.includes(day))
        return days.filter(d => d !== day);
    return [...days, day];
}

function ToggleAll(days: DayName[]) {
    if(days.length > 3)
        return [];
    return Object.values(DayName);
}

function DayElement(prop: {day: DayName, selected: boolean, onClick: () => void}) {
    return <div className={prop.selected ? 'Day selected' : 'Day'} onClick={prop.onClick}>{prop.day}</div>;
}

export type OnChangeType = (days: DayName[]) => void;
export function Widget_DaySelector(prop: {onChange: OnChangeType, days: DayName[]}) {
  return (
    <div className='DaySelector'>
        <div>
            <DayElement day={DayName.Monday} selected={prop.days.includes(DayName.Monday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Monday))} />
            <DayElement day={DayName.Tuesday} selected={prop.days.includes(DayName.Tuesday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Tuesday))} />
            <DayElement day={DayName.Wednesday} selected={prop.days.includes(DayName.Wednesday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Wednesday))} />
            <DayElement day={DayName.Thursday} selected={prop.days.includes(DayName.Thursday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Thursday))} />
            <DayElement day={DayName.Friday} selected={prop.days.includes(DayName.Friday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Friday))} />
        </div>
        <div>
            <DayElement day={DayName.Saturday} selected={prop.days.includes(DayName.Saturday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Saturday))} />
            <DayElement day={DayName.Sunday} selected={prop.days.includes(DayName.Sunday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Sunday))} />
            <div className='ToggleAll' onClick={() => prop.onChange(ToggleAll(prop.days))}>Toggle All</div>
        </div>
        
    </div>
  );
}
