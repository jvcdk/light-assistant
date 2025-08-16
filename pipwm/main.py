#!/usr/bin/env python3
import sys
from typing import List
from controller import Controller
from pwm_light import PwmLight
from pwm_driver import PwmDriver
from mqtt_driver import MqttDriver
import time
import signal
import argparse
import yaml
import os

# Global constant for default configuration
DEFAULT_CONFIG = {
    'controller': {
        'resolution': 32767
    },
    'pwm_controller': {
        'base_path': '/sys/class/pwm/pwmchip0/',
        'pwms': {
            0: 'pwm0',
            1: 'pwm1'
        },
        'period': 2**18*20 # 18 bit resolution x 20ns period
    },
    'mqtt': {
        'base_id': 'pipwm',
        'address': 'mosquitto',
        'port': 1883
    }
}

CTRL_C_PRESSED = False
LAST_CTRL_C_TIME = time.time() - 1

def signal_handler(signum, frame):
    global LAST_CTRL_C_TIME
    current_time = time.time()
    
    if (current_time - LAST_CTRL_C_TIME) <= 1:
        print("\nCtrl+C pressed twice within 1 second. Forcing shutdown...")
        exit(1)
    
    LAST_CTRL_C_TIME = current_time
    print("\nCtrl+C pressed. Press Ctrl+C again within 1 second to force shutdown.")
    print("Stopping controller...")
    controller.stop()

def load_config(config_file) -> dict:
    if not os.path.exists(config_file):
        print(f"Configuration file '{config_file}' not found. Using default values.")
        return DEFAULT_CONFIG

    with open(config_file, 'r') as file:
        try:
            config = yaml.safe_load(file)
            # Merge the loaded config with DEFAULT_CONFIG, giving priority to loaded config
            merged_config = DEFAULT_CONFIG.copy()
            merged_config.update(config)

            # Ensure MQTT port is an integer
            if 'mqtt' in merged_config and isinstance(merged_config['mqtt'], dict) and 'port' in merged_config['mqtt']:
                merged_config['mqtt']['port'] = int(merged_config['mqtt']['port'])

            return merged_config
        except yaml.YAMLError as e:
            print(f"Error parsing YAML configuration: {e}")
            return DEFAULT_CONFIG.copy()

def save_updated_config(config_file: str, config: dict, lights: List[PwmLight]):
    config['pwm_controller']['pwms'] = {light.get_pin(): light.get_name() for light in lights}
    with open(config_file, 'w') as file:
        yaml.dump(config, file, default_flow_style=False)

def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="PWM Controller")
    parser.add_argument("-c", "--config", default="pipwm.yaml",
                        help="Path to the configuration file (default: pipwm.yaml)")
    parser.add_argument("--write-default-config", metavar="FILENAME",
                        help="Write default configuration to the specified file and exit")
    args = parser.parse_args()

    if args.write_default_config:
        with open(args.write_default_config, 'w') as f:
            yaml.dump(DEFAULT_CONFIG, f, default_flow_style=False)
        print(f"Default configuration written to {args.write_default_config}")
        exit(0)
    return args

def get_system_id() -> str:
    system_id_path = "/etc/machine-id"
    if os.path.exists(system_id_path):
        with open(system_id_path, 'r') as f:
            return f.read().strip()

    system_id_path = "/var/lib/dbus/machine-id"
    if os.path.exists(system_id_path):
        with open(system_id_path, 'r') as f:
            return f.read().strip()

    print("System ID not found. Please generate one and store it /etc/machine-id or /var/lib/dbus/machine-id.")
    sys.exit(1)

if __name__ == "__main__":
    args = parse_arguments()
    system_id = get_system_id()

    # Set up signal handler for graceful shutdown
    signal.signal(signal.SIGINT, signal_handler)

    config = load_config(args.config)
    
    # Instantiate PwmDriver
    pwm_config = config['pwm_controller']
    pwm_driver = PwmDriver(
        pwm_config['base_path'],
        pwm_config['period']
    )
    
    # Instantiate MqttDriver
    mqtt_config = config['mqtt']
    mqtt_driver = MqttDriver(
        mqtt_config['base_id'],
        mqtt_config['address'],
        mqtt_config['port']
    )
    
    resolution = int(config['controller']['resolution'])
    if resolution < 1:
        print("Resolution must be greater than 0. Using default resolution of 16383.")
        resolution = 16383

    # Create Controller with the drivers
    controller = Controller(system_id, pwm_driver, mqtt_driver, pwm_config.get('pwms', {}), resolution, lambda lights: save_updated_config(args.config, config, lights))

    try:
        controller.start()
        print("Controller started. Press Ctrl+C to stop.")
        controller.run()
    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        controller.stop()
        print("Controller stopped")
