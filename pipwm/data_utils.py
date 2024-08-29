def create_light_status_payload(brightness: int, state: bool):
    stateStr = "ON" if state else "OFF"
    return f'{{"brightness":{brightness}, "state":"{stateStr}"}}'

def create_light_identity_payload(name: str):
    return f'{{"name":"{name}"}}'
