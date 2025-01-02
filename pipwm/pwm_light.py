import time
from pwm_driver import PwmDriver
from threading import Thread, Lock, Event

thread_sleep_time = 0.01 # 100Hz

class PwmLight:
    def __init__(self, pin: int, name: str, system_id: str, driver: PwmDriver):
        self._terminate = False
        self._pin = pin
        self._name = name
        self._system_id = system_id
        self._brightness = 0
        self._target_brightness = 0.0
        self._transition_steps = 0.0
        self._driver = driver
        self._thread = Thread(target=self._run)
        self._lock = Lock()
        self._event = Event()

        driver.config_pwm(pin)

        self._thread.start()
    
    def get_status(self) -> tuple[float, bool]:
        with self._lock:
            brightness = self._target_brightness
        return brightness, brightness > 0

    def get_name(self) -> str:
        return self._name

    def set_name(self, name: str):
        self._name = name

    def get_pin(self) -> int:
        return self._pin

    def get_id(self) -> str:
        return f"{self._system_id}::{self._pin}"

    def set_brightness(self, brightness: float, transition: float):
        brightness = self._clamp_brightness(brightness)
        if transition <= 0:
            with self._lock:
                self._target_brightness = brightness
                self._transition_steps = 0
        else:
            with self._lock:
                self._target_brightness = brightness
                diff = brightness - self._brightness
                self._transition_steps = diff * thread_sleep_time / transition

        self._event.set()

    def terminate(self):
        self._terminate = True
        self._event.set()
        self._thread.join()

    def _run(self):
        while not self._terminate:
            with self._lock:
                is_fading = self._brightness != self._target_brightness
                if is_fading:
                    if self._transition_steps == 0:
                        self._brightness = self._target_brightness
                    else:
                        self._brightness = self._clamp_brightness(self._brightness + self._transition_steps)
                        if self._transition_steps < 0 and self._brightness <= self._target_brightness:
                            self._brightness = self._target_brightness
                        elif self._transition_steps > 0 and self._brightness >= self._target_brightness:
                            self._brightness = self._target_brightness

                    self._driver.set_duty_cycle(self._pin, self._brightness)

            if is_fading:
                time.sleep(thread_sleep_time)
            else:
                self._event.wait()
                self._event.clear()

    def _clamp_brightness(self, brightness: float) -> float:
        if brightness < 0:
            return 0
        if brightness > 1.0:
            return 1.0
        return brightness