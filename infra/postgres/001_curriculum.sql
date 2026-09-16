-- PostgreSQL schema for the source-backed PLO/CLO curriculum dataset.
-- Run once on a dedicated database before generating/running the seed script.
CREATE SCHEMA IF NOT EXISTS curriculum;

CREATE TABLE IF NOT EXISTS curriculum.programs (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code text NOT NULL UNIQUE,
    name text NOT NULL
);

CREATE TABLE IF NOT EXISTS curriculum.program_versions (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_id bigint NOT NULL REFERENCES curriculum.programs(id),
    version_code text NOT NULL,
    dataset_status text NOT NULL DEFAULT 'finalized',
    UNIQUE (program_id, version_code)
);

CREATE TABLE IF NOT EXISTS curriculum.source_documents (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    file_name text NOT NULL,
    file_kind text NOT NULL CHECK (file_kind IN ('xlsx', 'docx', 'json')),
    sha256 char(64) NOT NULL,
    object_key text,
    UNIQUE (file_kind, sha256)
);

CREATE TABLE IF NOT EXISTS curriculum.import_batches (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_version_id bigint NOT NULL REFERENCES curriculum.program_versions(id),
    dataset_document_id bigint NOT NULL REFERENCES curriculum.source_documents(id),
    workbook_document_id bigint NOT NULL REFERENCES curriculum.source_documents(id),
    syllabus_document_id bigint NOT NULL REFERENCES curriculum.source_documents(id),
    dataset_sha256 char(64) NOT NULL,
    imported_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (program_version_id, dataset_sha256)
);

CREATE TABLE IF NOT EXISTS curriculum.source_course_rows (
    import_batch_id bigint NOT NULL REFERENCES curriculum.import_batches(id),
    source_row integer NOT NULL,
    raw_data jsonb NOT NULL,
    PRIMARY KEY (import_batch_id, source_row)
);

CREATE TABLE IF NOT EXISTS curriculum.plos (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_version_id bigint NOT NULL REFERENCES curriculum.program_versions(id),
    code text NOT NULL,
    statement text NOT NULL,
    level_code text,
    source_cell text,
    provenance text NOT NULL DEFAULT 'original_curriculum_map',
    UNIQUE (program_version_id, code),
    UNIQUE (id, program_version_id)
);

CREATE TABLE IF NOT EXISTS curriculum.courses (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    institutional_code text,
    name text NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS courses_institutional_code_uq
    ON curriculum.courses(institutional_code) WHERE institutional_code IS NOT NULL;

CREATE TABLE IF NOT EXISTS curriculum.program_courses (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_version_id bigint NOT NULL REFERENCES curriculum.program_versions(id),
    course_id bigint NOT NULL REFERENCES curriculum.courses(id),
    source_row integer NOT NULL,
    course_code_source_value text,
    credits integer NOT NULL CHECK (credits > 0),
    semester_source_value text,
    dataset_status text NOT NULL DEFAULT 'finalized',
    quality_flags jsonb NOT NULL DEFAULT '[]'::jsonb,
    UNIQUE (program_version_id, source_row),
    UNIQUE (id, program_version_id)
);

CREATE TABLE IF NOT EXISTS curriculum.weight_codes (
    code char(1) PRIMARY KEY,
    score integer NOT NULL CHECK (score > 0)
);
CREATE TABLE IF NOT EXISTS curriculum.progression_codes (
    code char(1) PRIMARY KEY,
    rank integer NOT NULL CHECK (rank > 0)
);
INSERT INTO curriculum.weight_codes (code, score) VALUES ('X', 2), ('Y', 1)
    ON CONFLICT (code) DO UPDATE SET score = EXCLUDED.score;
INSERT INTO curriculum.progression_codes (code, rank) VALUES ('I', 1), ('R', 2), ('E', 3)
    ON CONFLICT (code) DO UPDATE SET rank = EXCLUDED.rank;

CREATE TABLE IF NOT EXISTS curriculum.course_plos (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_version_id bigint NOT NULL,
    program_course_id bigint NOT NULL,
    plo_id bigint NOT NULL,
    weight_code char(1) NOT NULL REFERENCES curriculum.weight_codes(code),
    progression_code char(1) NOT NULL REFERENCES curriculum.progression_codes(code),
    provenance text NOT NULL,
    fit text,
    fit_reason text,
    FOREIGN KEY (program_course_id, program_version_id)
        REFERENCES curriculum.program_courses(id, program_version_id),
    FOREIGN KEY (plo_id, program_version_id)
        REFERENCES curriculum.plos(id, program_version_id),
    UNIQUE (program_course_id, plo_id),
    UNIQUE (id, program_course_id)
);

CREATE TABLE IF NOT EXISTS curriculum.clos (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    program_course_id bigint NOT NULL REFERENCES curriculum.program_courses(id),
    code text NOT NULL,
    statement text NOT NULL,
    level_code text,
    provenance text NOT NULL,
    source_reference jsonb,
    assumption_note text,
    dataset_status text NOT NULL DEFAULT 'finalized',
    UNIQUE (program_course_id, code),
    UNIQUE (id, program_course_id)
);

CREATE TABLE IF NOT EXISTS curriculum.clo_plos (
    clo_id bigint NOT NULL,
    program_course_id bigint NOT NULL,
    course_plo_id bigint NOT NULL,
    PRIMARY KEY (clo_id, course_plo_id),
    FOREIGN KEY (clo_id, program_course_id)
        REFERENCES curriculum.clos(id, program_course_id),
    FOREIGN KEY (course_plo_id, program_course_id)
        REFERENCES curriculum.course_plos(id, program_course_id)
);

CREATE OR REPLACE VIEW curriculum.plo_balance AS
WITH scores AS (
    SELECT p.program_version_id, p.code AS plo_code,
           COALESCE(SUM(pc.credits * w.score), 0)::numeric AS score
    FROM curriculum.plos p
    LEFT JOIN curriculum.course_plos cp ON cp.plo_id = p.id
    LEFT JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
    LEFT JOIN curriculum.weight_codes w ON w.code = cp.weight_code
    GROUP BY p.program_version_id, p.code
), means AS (
    SELECT scores.*, AVG(score) OVER (PARTITION BY program_version_id) AS mean_score
    FROM scores
)
SELECT program_version_id, plo_code, score, mean_score,
       ROUND((score - mean_score) / NULLIF(mean_score, 0), 4) AS deviation,
       ABS((score - mean_score) / NULLIF(mean_score, 0)) > 0.20 AS exceeds_20_percent
FROM means;
