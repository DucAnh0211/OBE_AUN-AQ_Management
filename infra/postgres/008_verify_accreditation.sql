DO $verify$
DECLARE
    v_framework_id bigint;
    criterion_count integer;
    requirement_count integer;
    official_statement_count integer;
    invalid_source_page_count integer;
BEGIN
    SELECT id INTO v_framework_id FROM accreditation.aun_frameworks
    WHERE code = 'AUN-QA-PROGRAMME' AND version = '4.0';
    IF v_framework_id IS NULL THEN RAISE EXCEPTION 'AUN-QA v4.0 is missing'; END IF;

    SELECT COUNT(*) INTO criterion_count FROM accreditation.aun_criteria
    WHERE aun_criteria.framework_id = v_framework_id;
    SELECT COUNT(*) INTO requirement_count
    FROM accreditation.aun_requirements r
    JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
    WHERE c.framework_id = v_framework_id;
    SELECT COUNT(*) INTO official_statement_count
    FROM accreditation.aun_requirement_translations rt
    JOIN accreditation.aun_requirements r ON r.id = rt.requirement_id
    JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
    WHERE c.framework_id = v_framework_id
      AND rt.language_code = 'en'
      AND rt.translation_status = 'official'
      AND rt.translation_source =
          'AUN-QA Programme Assessment Guide v4.0 (October 2020), Appendix A'
      AND rt.statement NOT LIKE 'AUN-QA requirement %';
    SELECT COUNT(*) INTO invalid_source_page_count
    FROM accreditation.aun_requirements r
    JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
    WHERE c.framework_id = v_framework_id
      AND (r.source_page IS NULL OR r.source_page NOT BETWEEN 63 AND 67);

    IF criterion_count <> 8 THEN RAISE EXCEPTION 'Expected 8 criteria, got %', criterion_count; END IF;
    IF requirement_count <> 53 THEN RAISE EXCEPTION 'Expected 53 requirements, got %', requirement_count; END IF;
    IF official_statement_count <> 53 THEN RAISE EXCEPTION 'Expected 53 official English statements, got %', official_statement_count; END IF;
    IF invalid_source_page_count <> 0 THEN RAISE EXCEPTION 'Found % requirements without a valid Appendix A source page', invalid_source_page_count; END IF;
END
$verify$;
