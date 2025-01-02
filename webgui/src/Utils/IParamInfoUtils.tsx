import { IParamEnum, IParamFloat, IParamInfo, IParamInt } from "../Data/JsonTypes";

export function GetParamDefault(param: IParamInfo): string {
  switch (param.Type) {
    case 'enum':
      return (param as IParamEnum).Default;

    case 'float':
    case 'brightness':
    case 'int':
      return (param as IParamFloat | IParamInt).Default.toString();
    default:
      console.error('Unknown param type', param);
      return '';
  }
}