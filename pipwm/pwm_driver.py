import os
import time

class PwmDriver:
    def __init__(self, base_path, period):
        self.base_path = base_path
        self._period = period

    def set_duty_cycle(self, pin: int, duty_cycle: float):
        path = self._get_path(pin, "duty_cycle")
        dc = int(duty_cycle * self._period)
        with open(path, "w") as f:
            f.write(str(dc))

    def config_pwm(self, pin):
        try:
            self._export(pin)
            time.sleep(0.1)

            dc = self._get_duty_cycle(pin)
            if dc != 0:
                self.set_duty_cycle(pin, 0)
                time.sleep(0.1)

            self._set_period(pin, self._period)
            time.sleep(0.1)

            self._enable(pin)

        except Exception as e:
            print(f"Warning: Failed to configure pwm{pin}. Error: {e}")

    def _export(self, pin):
        pwm_base_path = self._get_path(pin, "")
        if os.path.exists(pwm_base_path):
            return

        export_path = os.path.join(self.base_path, "export")
        with open(export_path, "w") as f:
            f.write(str(pin))

    def _get_path(self, pinId: int, element: str) -> str:
        return os.path.join(self.base_path, f"pwm{pinId}", element)

    def _get_duty_cycle(self, pin: int) -> int:
        path = self._get_path(pin, "duty_cycle")
        with open(path, "r") as f:
            return int(f.read())

    def _set_period(self, pin: int, period: int):
        path = self._get_path(pin, "period")
        with open(path, "w") as f:
            f.write(str(period))

    def _enable(self, pin: int):
        path = self._get_path(pin, "enable")
        with open(path, "w") as f:
            f.write("1")
