#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/deploy/docker-compose.postgres.yml"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "Docker Compose file was not found: $COMPOSE_FILE" >&2
  exit 1
fi

echo "Starting PostgreSQL for demo..."
docker compose -f "$COMPOSE_FILE" up -d

echo
echo "Applying migrations..."
"$PROJECT_ROOT/scripts/apply-migrations.sh"

echo
echo "Demo infrastructure is ready."
echo "Now run API in one terminal:"
echo "./scripts/run-api.sh"
echo
echo "Then run Web in another terminal:"
echo "./scripts/run-web.sh"
echo
echo "Open:"
echo "http://localhost:5080/system-overview"
