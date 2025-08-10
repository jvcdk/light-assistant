#!/bin/bash

[[ -f /data/lighttpd.conf ]] || (
    echo "Creating default lighttpd configuration..."
    cat /etc/lighttpd/lighttpd.conf | sed \
        -e 's|server.document-root.*|server.document-root = "/app/webgui"|' \
        -e 's|server.port.*|server.port = 8080|' \
        -e 's|server.pid-file.*|server.pid-file = "/tmp/lighttpd.pid"|' \
        -e 's|server.errorlog.*|server.errorlog = "/data/lighttpd.log"|' \
        > /data/lighttpd.conf
)
lighttpd -f /data/lighttpd.conf # Start lighttpd server as daemon

[[ -f /data/light-assistant.conf ]] || (
    echo "Creating default light-assistant configuration..."
    ./light-assistant/light-assistant --config /data/light-assistant.conf --save-config
)

./light-assistant/light-assistant --config /data/light-assistant.conf
