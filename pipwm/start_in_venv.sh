#!bash
set -eux

cd "$(dirname "$0")"

if [[ ! -d .venv ]]; then
  python3 -m venv .venv
fi
. ./.venv/bin/activate
pip install -r requirements
./main.py "$@"

