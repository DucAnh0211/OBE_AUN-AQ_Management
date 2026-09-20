#!/bin/sh
set -eu

run_sql() {
    file_path="$1"
    echo "Applying ${file_path}"
    psql \
        --username "$POSTGRES_USER" \
        --dbname "$POSTGRES_DB" \
        --set ON_ERROR_STOP=1 \
        --file "$file_path"
}

run_sql /sql/001_curriculum.sql
run_sql /sql/seeds/002_seed_plo_clo_v1.sql
run_sql /sql/004_curriculum_crud.sql
run_sql /sql/seeds/004_seed_plo_clo_v2.sql
run_sql /sql/003_verify_curriculum.sql
run_sql /sql/006_verify_plo_clo_v2.sql

echo "PostgreSQL curriculum bootstrap completed successfully."
