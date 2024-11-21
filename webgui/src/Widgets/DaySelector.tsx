import './DaySelector.css'
import { DayName } from "../Data/ScheduleTrigger";
import { Action } from '../Utils/Action';

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

function DayElement(prop: {day: DayName, isWeekDay: boolean; selected: boolean, onClick: () => void}) {
    const className = prop.isWeekDay ? 'Weekday' : 'Weekend';
    return <div className={`Day ${className} ` + (prop.selected && 'selected')} onClick={prop.onClick}>{prop.day.charAt(0)}</div>;
}

export function DaySelector(prop: {onChange: Action<DayName[]>, days: DayName[]}) {
  return (
    <div className='DaySelector'>
        <div className='FlexHori'>
            <DayElement isWeekDay={true} day={DayName.Monday} selected={prop.days.includes(DayName.Monday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Monday))} />
            <DayElement isWeekDay={true} day={DayName.Tuesday} selected={prop.days.includes(DayName.Tuesday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Tuesday))} />
            <DayElement isWeekDay={true} day={DayName.Wednesday} selected={prop.days.includes(DayName.Wednesday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Wednesday))} />
            <DayElement isWeekDay={true} day={DayName.Thursday} selected={prop.days.includes(DayName.Thursday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Thursday))} />
            <DayElement isWeekDay={true} day={DayName.Friday} selected={prop.days.includes(DayName.Friday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Friday))} />
        </div>
        <div className='FlexHori'>
            <DayElement isWeekDay={false} day={DayName.Saturday} selected={prop.days.includes(DayName.Saturday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Saturday))} />
            <DayElement isWeekDay={false} day={DayName.Sunday} selected={prop.days.includes(DayName.Sunday)} onClick={() => prop.onChange(ToggleDay(prop.days, DayName.Sunday))} />
            <div className='Spacer'></div>
            <div className='ToggleAll' onClick={() => prop.onChange(ToggleAll(prop.days))}>All</div>
        </div>
    </div>
  );
}
