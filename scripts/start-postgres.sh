#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/deploy/docker-compose.postgres.yml"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "Docker Compose file was not found: $COMPOSE_FILE" >&2
  exit 1
fi

echo "Starting PostgreSQL container..."
docker compose -f "$COMPOSE_FILE" up -d

echo
echo "Current PostgreSQL container:"
docker ps --filter "name=multisig-postgres"

echo
echo "PostgreSQL startup command completed."
echo "Use './scripts/open-psql.sh' to open psql."
