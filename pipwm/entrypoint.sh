#!/bin/sh

exec .venv/bin/python3 main.py --config /app/data/pipwm.yaml "$@"
