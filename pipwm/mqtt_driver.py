import ast
from typing import Callable, List
import paho.mqtt.client as mqtt
from utils import eprint

class MqttDriver:
    def __init__(self, base_id, host_address, host_port):
        self._client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, base_id)
        self._client.on_connect = self.on_connect
        self._client.on_disconnect = self.on_disconnect
        self._client.on_message = self.on_message
        self._host_address = host_address
        self._host_port = host_port
        self._base_id = base_id
        self._callback = lambda topic, payload: List[str], dict[str, str]

    def connect(self):
        self._client.connect(self._host_address, self._host_port)
    
    def disconnect(self):
        self._client.disconnect()

    def start(self):
        self._client.loop_start()

    def stop(self):
        self._client.loop_stop()

    def on_connect(self, client, userdata, flags, reason_code, properties):
        if reason_code == 0:
            print(f"MQTT connection successful. Subscribing to {self._base_id}/+")
            self._client.subscribe(f"{self._base_id}/#")

        if reason_code > 0:
            print(f"MQTT connection failed with result code {reason_code}")
            # error processing

    def on_disconnect(self, client, userdata, flags, reason_code, properties):
        if reason_code == "Success":
            pass

        if reason_code > 0:
            print(f"MQTT disconnect failed with result code {reason_code}")

    def on_message(self, client, userdata, msg: mqtt.MQTTMessage):
        if self._callback is None:
            return

        try:
            payload = ast.literal_eval(msg.payload.decode("utf-8"))
        except Exception as e:
            eprint(f"Error parsing payload: {e}")
            return


        topics = msg.topic.split("/")
        if topics[0] == self._base_id:
            self._callback(topics[1:], payload)
        else:
            print(f"Unmatched topic received: {msg.topic}")

    def publish(self, topic: str, payload: str):
        topic = f"{self._base_id}/{topic}"
        self._client.publish(topic, payload, retain=True)

    def set_callback(self, callback: Callable[[List[str], dict[str, str]], None]):
        self._callback = callback
