export function safeGetValue(values: string[], idx: number) {
    return values.length > idx ? values[idx] : undefined;
}
