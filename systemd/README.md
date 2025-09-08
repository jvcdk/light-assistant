These systemd service files may help you set up light-assistant (and required services) on your Linux system. They use Docker to install and run the dependencies Mosquitto and Zigbee2Mqtt. Technically you could replace Mosquitto with another mqtt server, but Zigbee2Mqtt is a dependency.

I put the services on the same named Docker network: `z2m`.

Steps:
 * Place the files in `/etc/systemd/system/` folder (or where-ever your SystemD expects sercice files)
 * Go through the files and adapt to your needs:
   * The containers are started with user:group ID 65534:65534. This usually maps to nobody:nogroup.
   * Config folders are located in /var/lib/... . Create these with same user:group ownership as above.
   * Port: Light Assistant listens on port 80 (for web) and 8081 (for websocket).
   * Also, see notes below.
 * Enable and start the services.

## Zigbee2Mqtt

You want to run Zigbee2Mqtt manually the first time with a port forwarding for 8080 such that you can perform the one-time setup.

The hostname for the mqtt server will be `mosquitto` (as set by the `--name` parameter for the `docker` command).

You need to edit the systemd file to map your USB device in (usually `/dev/ttyACM0` or `/dev/ttyUSB0`).

## Mosquitto

You may want to manually create this configuration file in `/var/lib/mosquitto/mosquitto.conf`:

```
allow_anonymous true
listener 1883 0.0.0.0
```

The `allow_anonymous` is safe as long as the services are on the closed `docker` network. Otherwise, you need to add authentication.

The `listener` command is needed for Mosquitto to listen on all interfaces (not just `localhost`). This is needed as it is containerized by `docker` (and thus `localhost` does not make any sense).

## PiPwm

You need to map in the GPIO interface for your PWM. The directory you mount may not contain any symlinks. For example, on my machine, the path `/sys/class/pwm/` contains a symlink `pwmchip0 -> ../../devices/platform/axi/1000120000.pcie/1f00098000.pwm/pwm/pwmchip0`. Mounting `/sys/class/pwm/` will fail (it wilil create a RO file system within the container). You need to mount `/sys/class/pwm/pwmchip0/`.

You might need to change the groupid of your container. In the provided file, I use ID 993, which is my `gpio` group, and which as write access to `/sys/class/pwm/pwmchip0/`. If your gpio ownership is different, you need to update the service file.

You need to give your machine a name by setting the environment variable `PIPWM_MACHINE_ID`.
