#!/usr/bin/env bash
set -euo pipefail

REMOVE_VOLUMES="${1:-}"

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/deploy/docker-compose.postgres.yml"

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "Docker Compose file was not found: $COMPOSE_FILE" >&2
  exit 1
fi

if [[ "$REMOVE_VOLUMES" == "--remove-volumes" ]]; then
  echo "Stopping PostgreSQL and removing Docker volumes..."
  echo "This will delete PostgreSQL data."
  docker compose -f "$COMPOSE_FILE" down -v
else
  echo "Stopping PostgreSQL container..."
  docker compose -f "$COMPOSE_FILE" down
fi

echo "PostgreSQL stop command completed."
