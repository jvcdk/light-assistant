# Pi PWM

Pi PWM is a little Python based service that exposes the PWM driver output as an MQTT service. It probably belongs in it's own repo, but for now it is placed here under light-assistant (which is the context for which I have developed and used it).

Please see the SystemD service file [pipwm.service](../systemd/pipwm.service) on how to run this in a container (mounting / specifying config file, and providing system id, access to `/sys/...`).

The default configuration is to connect to mqtt server `mosquitto`, because this is what it is named with the provided SystemD files. If you prefer to run things on localhost, you need to update the configuration accordingly.

## Raspberry Pi configuration

The PiPwm service assumes that your Raspberry Pi has pwm-overlay enabled:

* Edit the file `/boot/firmware/config.txt`:
  * Add the line `dtoverlay=pwm-2chan`.
* Reboot.
* The PWM drivers are not available via sysfs interface at `/sys/class/pwm/pwmchip0`.
