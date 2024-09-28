import time
from typing import List, Callable

from data_utils import create_light_status_payload, create_light_identity_payload
from mqtt_driver import MqttDriver
from utils import eprint
from pwm_driver import PwmDriver
from pwm_light import PwmLight

def find_light_by_name(lights: List[PwmLight], name: str) -> PwmLight | None:
    for light in lights:
        if light.get_name() == name:
            return light
        if light.get_id() == name:
            return light
    return None

class Controller:
    def __init__(self, system_id: str, pwm_driver: PwmDriver, mqtt_driver: MqttDriver, pwms: dict[int, str], resolution: int, device_names_updated_callback: Callable[[List[PwmLight]], None]):
        self._system_id = system_id
        self._device_names_updated_callback = device_names_updated_callback
        self._mqtt_driver = mqtt_driver
        self._mqtt_driver.set_callback(self.handle_message)
        self._resolution = resolution
        self.running = False
        self._init_lights(pwms, pwm_driver)

    def handle_message(self, topic: List[str], payload: dict):
        if len(topic) == 0:
            eprint("Empty topic received.")
            return

        if len(topic) == 1:
            eprint("Missing command in topic.")
            return

        deviceId = topic[0]
        command = topic[1]

        if command == "set":
            self._handle_set_command(deviceId, topic[2:], payload)
        elif command == "rename":
            self._handle_rename_command(deviceId, topic[2:], payload)
        elif command == "status" or command == "identity":
            pass # Ignore own commands
        else:
            eprint(f"Unrecognized command: {command} - {topic} - {payload}")

    def start(self):
        if self.running:
            print("Controller already running")
            return

        print("Controller starting...")
        self._mqtt_driver.connect()
        self._mqtt_driver.start()
        self.running = True

        self._identify_all_lights()
        self._report_all_light_status()

    def stop(self):
        if self.running:
            print("Controller stopping...")
            self.running = False
            self._mqtt_driver.stop()
            self._mqtt_driver.disconnect()
            for light in self._lights:
                light.terminate()

    def run(self):
        while self.running:
            time.sleep(1)
    
    def _init_lights(self, pwms: dict[int, str], pwm_driver: PwmDriver):
        self._lights: List[PwmLight] = []
        for pin, name in pwms.items():
            light = PwmLight(pin, name, self._system_id, pwm_driver)
            self._lights.append(light)

    def _identify_all_lights(self):
        for light in self._lights:
            self._identify_light(light)

    def _identify_light(self, light: PwmLight):
        payload = create_light_identity_payload(light.get_name())
        self._mqtt_driver.publish(f"{light.get_id()}/identity", payload)

    def _report_all_light_status(self):
        for light in self._lights:
            self._report_light_status(light)

    def _report_light_status(self, light: PwmLight):
        (brightness, state) = light.get_status()
        payload = create_light_status_payload(int(brightness * self._resolution), state)
        self._mqtt_driver.publish(f"{light.get_id()}/status", payload)

    def _handle_rename_command(self, deviceId: str, topic: List[str], payload: dict):
        if len(topic) != 0:
            eprint("Invalid rename command")
            return

        device = find_light_by_name(self._lights, deviceId)
        if device is None:
            eprint(f"Device not found: {deviceId}")
            return

        if "to" in payload:
            device.set_name(payload["to"])
            self._device_names_updated_callback(self._lights)
            self._identify_light(device)
        else:
            eprint(f"Missing name in payload: {payload}")
            return

    def _handle_set_command(self, deviceId: str, topic: List[str], payload: dict):
        if len(topic) != 0:
            eprint("Invalid set command")
            return

        device = find_light_by_name(self._lights, deviceId)
        if device is None:
            eprint(f"Device not found: {deviceId}")
            return

        if not "brightness" in payload:
            eprint(f"Missing brightness or state in payload: {payload}")
            return
        brightness = int(payload["brightness"])

        transition = 0.25
        if "transition" in payload:
            transition = float(payload["transition"])
            if transition < 0:
                transition = 0

        device.set_brightness(brightness / self._resolution, transition)
        self._report_light_status(device)
