#!/usr/bin/env bash
set -euo pipefail

requests=10
url="http://localhost:7080/api/v1/companies"
method="GET"
data=''

usage() {
  cat <<EOF
Usage: $0 [-n REQUESTS] [-u URL] [-m METHOD] 
  -n   Number of requests to send (default: ${requests})
  -u   Target URL to probe (default: ${url})
  -m   HTTP method: GET or POST (default: ${method})
  
Examples:
  $0 -n 20                            # GET to default URL
  $0 -n 10 -m GET -u "/api/v1/coffeeshops"    # GET to specific endpoint
  $0 -n 5 -m POST -u "/api/v1/companies"      # POST to default endpoint
EOF
}

while getopts ":n:u:m:h" opt; do
  case "$opt" in
    n) requests="$OPTARG" ;;
    u) url="http://localhost:7080${OPTARG}" ;;  # ← Автоматически добавляем base URL
    m) method="$OPTARG" ;;
    h) usage; exit 0 ;;
    *) usage; exit 1 ;;
  esac
done

declare -A counts=()

echo "Testing $requests $method requests to: $url"
echo "=============================================="

for i in $(seq 1 "$requests"); do
  if [[ "$method" == "POST" ]]; then
    # Для POST используем стандартные тестовые данные
    instance=$(curl -s -D - -X POST \
      -H "Content-Type: application/json" \
      -d '{"name":"Test Company"}' \
      -o /dev/null "$url" | \
      awk -F': ' '/^X-Instance-Name:/ {gsub(/\r/,"",$2); print $2; exit}')
  else
    # Для GET просто делаем запрос
    instance=$(curl -s -D - -o /dev/null "$url" | \
      awk -F': ' '/^X-Instance-Name:/ {gsub(/\r/,"",$2); print $2; exit}')
  fi
  
  if [[ -z "${instance:-}" ]]; then
    echo "Warning: no X-Instance-Name header on request #$i" >&2
    continue
  fi
  counts["$instance"]=$(( ${counts["$instance"]:-0} + 1 ))
  echo -n "."
done

echo ""
echo ""
echo "Results:"
echo "========"
for k in "${!counts[@]}"; do
  printf "%-10s %d\n" "$k" "${counts[$k]}"
done

# Показываем ожидаемое распределение
echo ""
echo "Expected distribution:"
if [[ "$method" == "POST" ]]; then
  echo "100% → main (all POST go to master)"
else
  echo "~50% → main (weight=2)"
  echo "~25% → ro1 (weight=1)"  
  echo "~25% → ro2 (weight=1)"
fi
