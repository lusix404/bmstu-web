#!/usr/bin/env bash
set -euo pipefail

requests=120
url="http://localhost:7080/api/v1/companies"

usage() {
  cat <<EOF
Usage: $0 [-n REQUESTS] [-u URL]
  -n   Number of requests to send (default: ${requests})
  -u   Target URL to probe (default: ${url})
Example:
  $0 -n 200 -u http://localhost:7080/api/v1/companies
EOF
}

while getopts ":n:u:h" opt; do
  case "$opt" in
    n) requests="$OPTARG" ;;
    u) url="$OPTARG" ;;
    h) usage; exit 0 ;;
    *) usage; exit 1 ;;
  esac
done

declare -A counts=()

for i in $(seq 1 "$requests"); do
  instance=$(curl -s -D - -o /dev/null "$url" | awk -F': ' '/^X-Instance-Name:/ {gsub(/\r/,"",$2); print $2; exit}')
  if [[ -z "${instance:-}" ]]; then
    echo "Warning: no X-Instance-Name header on request #$i" >&2
    continue
  fi
  counts["$instance"]=$(( ${counts["$instance"]:-0} + 1 ))
done

echo "Results for $requests requests to $url"
for k in "${!counts[@]}"; do
  printf "%-10s %d\n" "$k" "${counts[$k]}"
done
