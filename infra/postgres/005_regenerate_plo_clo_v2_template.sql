-- Template used by scripts/regenerate_plo_clo_v2.py.
-- Rebuilds V2 from finalized V1 without changing any V1 PLO/CLO data.
BEGIN;
SET client_encoding = 'UTF8';

DO $seed_v2$
DECLARE
    v_data jsonb := __JSON_PAYLOAD__::jsonb;
    v_json_sha text := __JSON_SHA__;
    v_program_id bigint;
    v_v1_id bigint;
    v_v2_id bigint;
    v_v1_batch_id bigint;
    v_v2_batch_id bigint;
    v_json_document_id bigint;
    v_workbook_document_id bigint;
    v_syllabus_document_id bigint;
    v_program_course_id bigint;
    v_plo_id bigint;
    v_course_plo_id bigint;
    v_clo_id bigint;
    v_item jsonb;
    v_course jsonb;
    v_link jsonb;
    v_clo jsonb;
    v_support text;
    v_actual integer;
    v_expected integer;
BEGIN
    PERFORM pg_advisory_xact_lock(hashtext('curriculum_CS_V2_plo_clo_regeneration'));

    SELECT p.id INTO STRICT v_program_id
    FROM curriculum.programs p
    WHERE p.code = 'CS' AND p.archived_at IS NULL;

    SELECT pv.id INTO STRICT v_v1_id
    FROM curriculum.program_versions pv
    WHERE pv.program_id = v_program_id
      AND pv.version_code = 'V1'
      AND pv.dataset_status = 'finalized'
      AND pv.archived_at IS NULL;

    INSERT INTO curriculum.program_versions
        (program_id, version_code, dataset_status, source_version_id, is_current)
    VALUES (v_program_id, 'V2', 'draft', v_v1_id, false)
    ON CONFLICT (program_id, version_code) DO NOTHING;

    SELECT pv.id INTO STRICT v_v2_id
    FROM curriculum.program_versions pv
    WHERE pv.program_id = v_program_id AND pv.version_code = 'V2';

    IF (SELECT dataset_status FROM curriculum.program_versions WHERE id = v_v2_id) <> 'draft' THEN
        RAISE EXCEPTION 'V2 must be draft before regenerating PLO/CLO data';
    END IF;

    -- A seed rerun is a full reset of V2 learning-outcome data and course snapshots.
    DELETE FROM curriculum.clo_plos x
    USING curriculum.clos c, curriculum.program_courses pc
    WHERE x.clo_id = c.id
      AND c.program_course_id = pc.id
      AND pc.program_version_id = v_v2_id;

    DELETE FROM curriculum.clos c
    USING curriculum.program_courses pc
    WHERE c.program_course_id = pc.id AND pc.program_version_id = v_v2_id;

    DELETE FROM curriculum.course_plos cp
    USING curriculum.program_courses pc
    WHERE cp.program_course_id = pc.id AND pc.program_version_id = v_v2_id;

    DELETE FROM curriculum.program_courses WHERE program_version_id = v_v2_id;
    DELETE FROM curriculum.plos WHERE program_version_id = v_v2_id;

    INSERT INTO curriculum.program_courses
        (program_version_id, course_id, source_row, course_code_source_value,
         credits, semester_source_value, dataset_status, quality_flags,
         course_code_snapshot, course_name_snapshot, display_order)
    SELECT v_v2_id, pc.course_id, pc.source_row, pc.course_code_source_value,
           pc.credits, pc.semester_source_value, 'draft', pc.quality_flags,
           pc.course_code_snapshot, pc.course_name_snapshot, pc.display_order
    FROM curriculum.program_courses pc
    WHERE pc.program_version_id = v_v1_id AND pc.archived_at IS NULL
    ORDER BY pc.display_order, pc.id;

    SELECT COUNT(*) INTO v_actual
    FROM curriculum.program_courses WHERE program_version_id = v_v2_id;
    IF v_actual <> 59 THEN
        RAISE EXCEPTION 'V2 course count is %, expected 59', v_actual;
    END IF;

    FOR v_item IN SELECT value FROM jsonb_array_elements(v_data->'program_plos_original')
    LOOP
        INSERT INTO curriculum.plos
            (program_version_id, code, statement, level_code, source_cell, provenance)
        VALUES (v_v2_id, v_item->>'id', v_item->>'text', v_item->>'level',
                v_item->>'source_cell', 'copied_official_plo_from_v1');
    END LOOP;

    FOR v_course IN SELECT value FROM jsonb_array_elements(v_data->'courses_finalized')
    LOOP
        SELECT pc.id INTO STRICT v_program_course_id
        FROM curriculum.program_courses pc
        WHERE pc.program_version_id = v_v2_id
          AND pc.source_row = (v_course->>'source_row')::integer;

        FOR v_link IN SELECT value FROM jsonb_array_elements(v_course->'plo_links')
        LOOP
            SELECT p.id INTO STRICT v_plo_id
            FROM curriculum.plos p
            WHERE p.program_version_id = v_v2_id AND p.code = v_link->>'plo_id';

            INSERT INTO curriculum.course_plos
                (program_version_id, program_course_id, plo_id, weight_code,
                 progression_code, provenance, fit, fit_reason)
            VALUES (v_v2_id, v_program_course_id, v_plo_id,
                    v_link->>'weight_code', v_link->>'progression_code',
                    v_link->>'provenance', v_link->>'fit', v_link->>'fit_reason')
            RETURNING id INTO v_course_plo_id;
        END LOOP;

        FOR v_clo IN SELECT value FROM jsonb_array_elements(v_course->'clos')
        LOOP
            INSERT INTO curriculum.clos
                (program_course_id, code, statement, level_code, provenance,
                 source_reference, assumption_note, dataset_status)
            VALUES (v_program_course_id, v_clo->>'id', v_clo->>'text',
                    v_clo->>'bloom_level', v_clo->>'provenance',
                    v_clo->'source_reference', v_clo->>'assumption_note', 'draft')
            RETURNING id INTO v_clo_id;

            FOR v_support IN SELECT value FROM jsonb_array_elements_text(v_clo->'supports_plo')
            LOOP
                SELECT cp.id INTO STRICT v_course_plo_id
                FROM curriculum.course_plos cp
                JOIN curriculum.plos p ON p.id = cp.plo_id
                WHERE cp.program_course_id = v_program_course_id AND p.code = v_support;

                INSERT INTO curriculum.clo_plos (clo_id, program_course_id, course_plo_id)
                VALUES (v_clo_id, v_program_course_id, v_course_plo_id);
            END LOOP;
        END LOOP;
    END LOOP;

    -- Preserve import provenance for the regenerated JSON and original documents.
    SELECT b.id, b.workbook_document_id, b.syllabus_document_id
    INTO STRICT v_v1_batch_id, v_workbook_document_id, v_syllabus_document_id
    FROM curriculum.import_batches b
    WHERE b.program_version_id = v_v1_id
    ORDER BY b.imported_at DESC, b.id DESC
    LIMIT 1;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (__JSON_FILE_NAME__, 'json', v_json_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_json_document_id;

    INSERT INTO curriculum.import_batches
        (program_version_id, dataset_document_id, workbook_document_id,
         syllabus_document_id, dataset_sha256)
    VALUES (v_v2_id, v_json_document_id, v_workbook_document_id,
            v_syllabus_document_id, v_json_sha)
    ON CONFLICT (program_version_id, dataset_sha256)
        DO UPDATE SET imported_at = now(), dataset_document_id = EXCLUDED.dataset_document_id
    RETURNING id INTO v_v2_batch_id;

    FOR v_item IN SELECT value FROM jsonb_array_elements(v_data->'curriculum_map_source_data')
    LOOP
        INSERT INTO curriculum.source_course_rows (import_batch_id, source_row, raw_data)
        VALUES (v_v2_batch_id, (v_item->>'source_row')::integer, v_item)
        ON CONFLICT (import_batch_id, source_row) DO UPDATE SET raw_data = EXCLUDED.raw_data;
    END LOOP;

    -- Exact counts must agree with the generated JSON.
    SELECT COUNT(*) INTO v_actual FROM curriculum.plos WHERE program_version_id = v_v2_id;
    IF v_actual <> 5 THEN RAISE EXCEPTION 'V2 PLO count is %, expected 5', v_actual; END IF;

    SELECT COUNT(*) INTO v_actual
    FROM curriculum.course_plos cp JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
    WHERE pc.program_version_id = v_v2_id;
    SELECT SUM(jsonb_array_length(value->'plo_links')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'V2 course-PLO count is %, expected %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual
    FROM curriculum.clos c JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
    WHERE pc.program_version_id = v_v2_id;
    SELECT SUM(jsonb_array_length(value->'clos')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'V2 CLO count is %, expected %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual
    FROM curriculum.clo_plos x JOIN curriculum.program_courses pc ON pc.id = x.program_course_id
    WHERE pc.program_version_id = v_v2_id;
    SELECT COUNT(*) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized') AS course_item(course_data)
    CROSS JOIN LATERAL jsonb_array_elements(course_item.course_data->'clos') AS clo_item(clo_data)
    CROSS JOIN LATERAL jsonb_array_elements(clo_item.clo_data->'supports_plo');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'V2 CLO-PLO count is %, expected %', v_actual, v_expected; END IF;

    -- Every course meets min(credits, 5).
    IF EXISTS (
        SELECT 1
        FROM curriculum.program_courses pc
        LEFT JOIN curriculum.course_plos cp ON cp.program_course_id = pc.id
        WHERE pc.program_version_id = v_v2_id
        GROUP BY pc.id, pc.credits
        HAVING COUNT(cp.id) < LEAST(pc.credits, 5)
    ) THEN RAISE EXCEPTION 'V2 has courses below the credit-to-PLO rule'; END IF;

    -- Every CLO supports at least one PLO.
    IF EXISTS (
        SELECT 1 FROM curriculum.clos c
        JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
        LEFT JOIN curriculum.clo_plos x ON x.clo_id = c.id
        WHERE pc.program_version_id = v_v2_id
        GROUP BY c.id HAVING COUNT(x.course_plo_id) = 0
    ) THEN RAISE EXCEPTION 'V2 has CLOs without a PLO'; END IF;

    -- Every course-PLO is supported by at least one CLO.
    IF EXISTS (
        SELECT 1 FROM curriculum.course_plos cp
        JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
        LEFT JOIN curriculum.clo_plos x ON x.course_plo_id = cp.id
        WHERE pc.program_version_id = v_v2_id
        GROUP BY cp.id HAVING COUNT(x.clo_id) = 0
    ) THEN RAISE EXCEPTION 'V2 has course PLOs without a supporting CLO'; END IF;

    -- Each course has between two and five measurable CLOs.
    IF EXISTS (
        SELECT 1 FROM curriculum.program_courses pc
        LEFT JOIN curriculum.clos c ON c.program_course_id = pc.id
        WHERE pc.program_version_id = v_v2_id
        GROUP BY pc.id HAVING COUNT(c.id) < 2 OR COUNT(c.id) > 5
    ) THEN RAISE EXCEPTION 'V2 has a course outside the 2-to-5 CLO range'; END IF;

    -- The regenerated set must contain actual many-to-many CLO mappings.
    IF NOT EXISTS (
        SELECT 1 FROM curriculum.clo_plos x
        JOIN curriculum.program_courses pc ON pc.id = x.program_course_id
        WHERE pc.program_version_id = v_v2_id
        GROUP BY x.clo_id HAVING COUNT(x.course_plo_id) > 1
    ) THEN RAISE EXCEPTION 'V2 has no CLO linked to multiple PLOs'; END IF;

    -- Weighted PLO score deviation must remain within 20 percent.
    IF EXISTS (
        SELECT 1 FROM curriculum.plo_balance
        WHERE program_version_id = v_v2_id AND exceeds_20_percent
    ) THEN RAISE EXCEPTION 'V2 PLO balance exceeds the 20 percent threshold'; END IF;

    RAISE NOTICE 'V2 regenerated successfully: % courses, % PLOs, % course-PLOs, % CLOs, % CLO-PLOs',
        (SELECT COUNT(*) FROM curriculum.program_courses WHERE program_version_id = v_v2_id),
        (SELECT COUNT(*) FROM curriculum.plos WHERE program_version_id = v_v2_id),
        (SELECT COUNT(*) FROM curriculum.course_plos WHERE program_version_id = v_v2_id),
        (SELECT COUNT(*) FROM curriculum.clos c JOIN curriculum.program_courses pc ON pc.id = c.program_course_id WHERE pc.program_version_id = v_v2_id),
        (SELECT COUNT(*) FROM curriculum.clo_plos x JOIN curriculum.program_courses pc ON pc.id = x.program_course_id WHERE pc.program_version_id = v_v2_id);
END
$seed_v2$;

COMMIT;
