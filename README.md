# Light-assistant

Light-assistant is a lightweight home automation tool focused on easy and intuitive control of lights. The name is a pun on both its simplicy usage and its main purpose.

The project is provided as-is; it is a project I wrote for my own use at home, but if it can help you out as well, I am glad :)

NOTE: There is currently no user/password protection implemented. Only run this on *trusted* home networks!

## License

This project is licensed under the [BSD 3-Clause License](./LICENSE).

## Building

To build the main project, run:

```
docker build . -t light-assistant:latest
```

## Child Project: pipwm

[`pipwm`](./pipwm/README.md) is a PWM controller for Raspberry Pi, publishing PWM controller data on an MQTT bus.

Build pipwm from the `pipwm` directory:

```
docker build . -t pipwm:latest
```

## Systemd Service Files

Helpful systemd service files are available in the [`systemd`](./systemd/README.md) directory.
