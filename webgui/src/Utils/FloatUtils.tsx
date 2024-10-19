
export function tryParseFloat(value: string | undefined, defaultValue: number): number {
  if (value === undefined)
    return defaultValue;
  const result = parseFloat(value);
  if (isNaN(result))
    return defaultValue;
  return result;
}
