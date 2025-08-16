# Pi PWM

Pi PWM is a little Python based service that exposes the PWM driver output as an MQTT service. It probably belongs in it's own repo, but for now it is placed here under light-assistant (which is the context for which I have developed and used it).

Please see the SystemD service file [pipwm.service](../systemd/pipwm.service) on how to run this in a container (mounting / specifying config file, and providing system id, access to /sys/...).
