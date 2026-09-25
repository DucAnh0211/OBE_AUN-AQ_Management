#!/bin/sh
set -eu

echo "Applying repeatable runtime migrations"

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/007_accreditation.sql

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/008_verify_accreditation.sql

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/009_accreditation_vi.sql

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/010_verify_accreditation_vi.sql

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/011_security.sql

psql \
  --host db \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --file /sql/012_verify_security.sql

echo "Runtime migrations completed successfully"
