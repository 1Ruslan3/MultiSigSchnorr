#!/usr/bin/env bash
set -euo pipefail

echo "Opening psql inside multisig-postgres container..."
echo "Use '\\dt' to list tables and '\\q' to exit."
echo

docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
