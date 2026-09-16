-- Generated seed SQL: replace __...__ tokens using scripts/generate_plo_clo_seed.py.
-- This template is not executable by itself. The generated file is UTF-8.
BEGIN;
SET client_encoding = 'UTF8';

DO $import$
DECLARE
    v_data jsonb := __JSON_PAYLOAD__::jsonb;
    v_json_sha text := __JSON_SHA__;
    v_workbook_sha text := __WORKBOOK_SHA__;
    v_syllabus_sha text := __SYLLABUS_SHA__;
    v_program_id bigint;
    v_version_id bigint;
    v_json_document_id bigint;
    v_workbook_document_id bigint;
    v_syllabus_document_id bigint;
    v_batch_id bigint;
    v_course_id bigint;
    v_program_course_id bigint;
    v_plo_id bigint;
    v_course_plo_id bigint;
    v_clo_id bigint;
    v_raw jsonb;
    v_plo jsonb;
    v_course jsonb;
    v_link jsonb;
    v_clo jsonb;
    v_support text;
    v_actual integer;
    v_expected integer;
BEGIN
    PERFORM pg_advisory_xact_lock(
        hashtext('curriculum_CS_' || (v_data->'metadata'->>'program_version') || '_import')
    );

    INSERT INTO curriculum.programs (code, name)
    VALUES ('CS', 'Khoa học máy tính')
    ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
    RETURNING id INTO v_program_id;

    INSERT INTO curriculum.program_versions (program_id, version_code, dataset_status)
    VALUES (v_program_id, v_data->'metadata'->>'program_version', 'finalized')
    ON CONFLICT (program_id, version_code)
        DO UPDATE SET dataset_status = EXCLUDED.dataset_status
    RETURNING id INTO v_version_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (__JSON_FILE_NAME__, 'json', v_json_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_json_document_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (v_data->'metadata'->>'source_workbook', 'xlsx', v_workbook_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_workbook_document_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (v_data->'metadata'->>'source_course_syllabus', 'docx', v_syllabus_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_syllabus_document_id;

    INSERT INTO curriculum.import_batches
        (program_version_id, dataset_document_id, workbook_document_id,
         syllabus_document_id, dataset_sha256)
    VALUES (v_version_id, v_json_document_id, v_workbook_document_id,
            v_syllabus_document_id, v_json_sha)
    ON CONFLICT (program_version_id, dataset_sha256)
        DO UPDATE SET imported_at = now(),
                      dataset_document_id = EXCLUDED.dataset_document_id,
                      workbook_document_id = EXCLUDED.workbook_document_id,
                      syllabus_document_id = EXCLUDED.syllabus_document_id
    RETURNING id INTO v_batch_id;

    FOR v_raw IN SELECT value FROM jsonb_array_elements(v_data->'curriculum_map_source_data')
    LOOP
        INSERT INTO curriculum.source_course_rows (import_batch_id, source_row, raw_data)
        VALUES (v_batch_id, (v_raw->>'source_row')::integer, v_raw)
        ON CONFLICT (import_batch_id, source_row)
            DO UPDATE SET raw_data = EXCLUDED.raw_data;
    END LOOP;

    FOR v_plo IN SELECT value FROM jsonb_array_elements(v_data->'program_plos_original')
    LOOP
        INSERT INTO curriculum.plos
            (program_version_id, code, statement, level_code, source_cell, provenance)
        VALUES (v_version_id, v_plo->>'id', v_plo->>'text', v_plo->>'level',
                v_plo->>'source_cell', 'original_curriculum_map')
        ON CONFLICT (program_version_id, code)
            DO UPDATE SET statement = EXCLUDED.statement,
                          level_code = EXCLUDED.level_code,
                          source_cell = EXCLUDED.source_cell
        RETURNING id INTO v_plo_id;
    END LOOP;

    FOR v_course IN SELECT value FROM jsonb_array_elements(v_data->'courses_finalized')
    LOOP
        v_program_course_id := NULL;
        v_course_id := NULL;
        SELECT id, course_id INTO v_program_course_id, v_course_id
        FROM curriculum.program_courses
        WHERE program_version_id = v_version_id
          AND source_row = (v_course->>'source_row')::integer;

        IF v_program_course_id IS NULL THEN
            INSERT INTO curriculum.courses (name)
            VALUES (v_course->>'name') RETURNING id INTO v_course_id;
            INSERT INTO curriculum.program_courses
                (program_version_id, course_id, source_row, course_code_source_value,
                 credits, semester_source_value, dataset_status, quality_flags)
            VALUES (v_version_id, v_course_id, (v_course->>'source_row')::integer,
                    v_course->>'course_code_source_value', (v_course->>'credits')::integer,
                    v_course->>'semester_source_value', v_course->>'dataset_status',
                    v_course->'quality_flags')
            RETURNING id INTO v_program_course_id;
        ELSE
            UPDATE curriculum.courses SET name = v_course->>'name' WHERE id = v_course_id;
            UPDATE curriculum.program_courses
            SET course_code_source_value = v_course->>'course_code_source_value',
                credits = (v_course->>'credits')::integer,
                semester_source_value = v_course->>'semester_source_value',
                dataset_status = v_course->>'dataset_status',
                quality_flags = v_course->'quality_flags'
            WHERE id = v_program_course_id;
        END IF;

        FOR v_link IN SELECT value FROM jsonb_array_elements(v_course->'plo_links')
        LOOP
            SELECT id INTO STRICT v_plo_id FROM curriculum.plos
            WHERE program_version_id = v_version_id AND code = v_link->>'plo_id';
            INSERT INTO curriculum.course_plos
                (program_version_id, program_course_id, plo_id, weight_code,
                 progression_code, provenance, fit, fit_reason)
            VALUES (v_version_id, v_program_course_id, v_plo_id,
                    v_link->>'weight_code', v_link->>'progression_code',
                    v_link->>'provenance', v_link->>'fit', v_link->>'fit_reason')
            ON CONFLICT (program_course_id, plo_id)
                DO UPDATE SET weight_code = EXCLUDED.weight_code,
                              progression_code = EXCLUDED.progression_code,
                              provenance = EXCLUDED.provenance,
                              fit = EXCLUDED.fit,
                              fit_reason = EXCLUDED.fit_reason
            RETURNING id INTO v_course_plo_id;
        END LOOP;

        FOR v_clo IN SELECT value FROM jsonb_array_elements(v_course->'clos')
        LOOP
            INSERT INTO curriculum.clos
                (program_course_id, code, statement, level_code, provenance,
                 source_reference, assumption_note, dataset_status)
            VALUES (v_program_course_id, v_clo->>'id', v_clo->>'text',
                    v_clo->>'bloom_level', v_clo->>'provenance',
                    v_clo->'source_reference', v_clo->>'assumption_note',
                    v_clo->>'dataset_status')
            ON CONFLICT (program_course_id, code)
                DO UPDATE SET statement = EXCLUDED.statement,
                              level_code = EXCLUDED.level_code,
                              provenance = EXCLUDED.provenance,
                              source_reference = EXCLUDED.source_reference,
                              assumption_note = EXCLUDED.assumption_note,
                              dataset_status = EXCLUDED.dataset_status
            RETURNING id INTO v_clo_id;

            FOR v_support IN SELECT value FROM jsonb_array_elements_text(v_clo->'supports_plo')
            LOOP
                SELECT cp.id INTO STRICT v_course_plo_id
                FROM curriculum.course_plos cp
                JOIN curriculum.plos p ON p.id = cp.plo_id
                WHERE cp.program_course_id = v_program_course_id
                  AND p.code = v_support;
                INSERT INTO curriculum.clo_plos (clo_id, program_course_id, course_plo_id)
                VALUES (v_clo_id, v_program_course_id, v_course_plo_id)
                ON CONFLICT (clo_id, course_plo_id) DO NOTHING;
            END LOOP;
        END LOOP;
    END LOOP;

    SELECT COUNT(*) INTO v_actual FROM curriculum.plos WHERE program_version_id = v_version_id;
    v_expected := jsonb_array_length(v_data->'program_plos_original');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'PLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.program_courses WHERE program_version_id = v_version_id;
    v_expected := jsonb_array_length(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Course count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.source_course_rows WHERE import_batch_id = v_batch_id;
    v_expected := jsonb_array_length(v_data->'curriculum_map_source_data');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Raw row count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.course_plos cp
    JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT SUM(jsonb_array_length(value->'plo_links')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Course-PLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.clos c
    JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT SUM(jsonb_array_length(value->'clos')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'CLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.clo_plos x
    JOIN curriculum.program_courses pc ON pc.id = x.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT COUNT(*) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized') AS c(course_data)
    CROSS JOIN LATERAL jsonb_array_elements(c.course_data->'clos') AS o(clo_data)
    CROSS JOIN LATERAL jsonb_array_elements(o.clo_data->'supports_plo') AS p(plo_data);
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'CLO-PLO count: % instead of %', v_actual, v_expected; END IF;
END
$import$;
COMMIT;
