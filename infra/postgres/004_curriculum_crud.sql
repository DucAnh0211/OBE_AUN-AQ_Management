-- API support for CTDT, curriculum versions and courses.
-- This migration is idempotent and does not alter PLO/CLO/mapping tables.
BEGIN;

ALTER TABLE curriculum.programs
    ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS updated_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS archived_at timestamptz;

ALTER TABLE curriculum.program_versions
    ADD COLUMN IF NOT EXISTS source_version_id bigint
        REFERENCES curriculum.program_versions(id),
    ADD COLUMN IF NOT EXISTS is_current boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS published_at timestamptz,
    ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS updated_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS archived_at timestamptz;

UPDATE curriculum.program_versions
SET published_at = COALESCE(published_at, created_at)
WHERE dataset_status = 'finalized';

WITH current_versions AS (
    SELECT DISTINCT ON (program_id) id
    FROM curriculum.program_versions
    WHERE dataset_status = 'finalized' AND archived_at IS NULL
    ORDER BY program_id, published_at DESC NULLS LAST, id DESC
)
UPDATE curriculum.program_versions pv
SET is_current = EXISTS (
    SELECT 1 FROM current_versions current_version WHERE current_version.id = pv.id
);

CREATE UNIQUE INDEX IF NOT EXISTS program_versions_current_uq
    ON curriculum.program_versions(program_id)
    WHERE is_current AND archived_at IS NULL;

DO $migration$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'program_versions_dataset_status_ck'
          AND conrelid = 'curriculum.program_versions'::regclass
    ) THEN
        ALTER TABLE curriculum.program_versions
            ADD CONSTRAINT program_versions_dataset_status_ck
            CHECK (dataset_status IN ('draft', 'finalized', 'archived'));
    END IF;
END
$migration$;

ALTER TABLE curriculum.courses
    ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS updated_at timestamptz NOT NULL DEFAULT now();

ALTER TABLE curriculum.program_courses
    ALTER COLUMN source_row DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS course_code_snapshot text,
    ADD COLUMN IF NOT EXISTS course_name_snapshot text,
    ADD COLUMN IF NOT EXISTS display_order integer,
    ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS updated_at timestamptz NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS archived_at timestamptz;

UPDATE curriculum.program_courses pc
SET course_code_snapshot = COALESCE(
        NULLIF(BTRIM(c.institutional_code), ''),
        CASE
            WHEN pc.quality_flags ? 'course_code_missing_or_placeholder' THEN NULL
            ELSE NULLIF(BTRIM(pc.course_code_source_value), '')
        END
    ),
    course_name_snapshot = COALESCE(pc.course_name_snapshot, c.name)
FROM curriculum.courses c
WHERE c.id = pc.course_id
  AND pc.course_name_snapshot IS NULL;

WITH ranked AS (
    SELECT id,
           ROW_NUMBER() OVER (
               PARTITION BY program_version_id
               ORDER BY source_row NULLS LAST, id
           )::integer AS position
    FROM curriculum.program_courses
)
UPDATE curriculum.program_courses pc
SET display_order = ranked.position
FROM ranked
WHERE ranked.id = pc.id AND pc.display_order IS NULL;

ALTER TABLE curriculum.program_courses
    ALTER COLUMN course_name_snapshot SET NOT NULL,
    ALTER COLUMN display_order SET NOT NULL;

DO $migration$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'program_courses_display_order_ck'
          AND conrelid = 'curriculum.program_courses'::regclass
    ) THEN
        ALTER TABLE curriculum.program_courses
            ADD CONSTRAINT program_courses_display_order_ck
            CHECK (display_order > 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'program_courses_dataset_status_ck'
          AND conrelid = 'curriculum.program_courses'::regclass
    ) THEN
        ALTER TABLE curriculum.program_courses
            ADD CONSTRAINT program_courses_dataset_status_ck
            CHECK (dataset_status IN ('draft', 'finalized', 'archived'));
    END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS program_courses_active_course_uq
    ON curriculum.program_courses(program_version_id, course_id)
    WHERE archived_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS program_courses_active_code_uq
    ON curriculum.program_courses(
        program_version_id,
        UPPER(BTRIM(course_code_snapshot))
    )
    WHERE course_code_snapshot IS NOT NULL AND archived_at IS NULL;

CREATE INDEX IF NOT EXISTS programs_active_lookup_ix
    ON curriculum.programs(UPPER(code), UPPER(name))
    WHERE archived_at IS NULL;

CREATE INDEX IF NOT EXISTS program_versions_program_lookup_ix
    ON curriculum.program_versions(program_id, created_at DESC);

CREATE INDEX IF NOT EXISTS program_courses_version_lookup_ix
    ON curriculum.program_courses(program_version_id, display_order, id);

COMMIT;
