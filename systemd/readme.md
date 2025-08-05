These systemd service files may help you set up light-assistant (and required services) on your Linux system. They use Docker to install and run the dependencies Mosquitto and Zigbee2Mqtt. Technically you could replace Mosquitto with another mqtt server, but Zigbee2Mqtt is a dependency.

I put the services on the same named Docker network: `z2m`.

Place the files in `/etc/systemd/system/` folder (or where-ever your SystemD expects sercice files) and enable and start them.

## Zigbee2Mqtt

You may want to run Zigbee2Mqtt manually the first time with a port forwarding for 8080 such that you can perform the one-time setup.

The hostname for the mqtt server will be `mosquitto` (as set by the `--name` parameter for the `docker` command).

## Mosquitto

You may want to manually create this configuration file in `/var/lib/mosquitto/mosquitto.conf`:

```
allow_anonymous true
listener 1833 0.0.0.0
```

The `allow_anonymous` is safe as long as the services are on the closed `docker` network. Otherwise, you need to add authentication.

The `listener` command is needed for Mosquitto to listen on all interfaces (not just `localhost`). This is needed as it is containerized by `docker` (and thus `localhost` does not make any sense).
