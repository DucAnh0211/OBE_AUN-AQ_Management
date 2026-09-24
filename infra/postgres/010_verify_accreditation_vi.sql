DO $verify$
DECLARE
    v_framework_id bigint;
    vi_statement_count integer;
BEGIN
    SELECT id INTO v_framework_id
    FROM accreditation.aun_frameworks
    WHERE code = 'AUN-QA-PROGRAMME' AND version = '4.0';

    IF v_framework_id IS NULL THEN
        RAISE EXCEPTION 'AUN-QA v4.0 is missing';
    END IF;

    SELECT COUNT(*) INTO vi_statement_count
    FROM accreditation.aun_requirement_translations rt
    JOIN accreditation.aun_requirements r ON r.id = rt.requirement_id
    JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
    WHERE c.framework_id = v_framework_id
      AND rt.language_code = 'vi'
      AND rt.translation_status = 'draft'
      AND rt.translation_source =
          'SAR Khoa học máy tính do đơn vị cung cấp; bản tiếng Việt tham khảo, không phải bản dịch chính thức của AUN'
      AND length(trim(rt.statement)) > 0;

    IF vi_statement_count <> 53 THEN
        RAISE EXCEPTION 'Expected 53 Vietnamese draft statements, got %', vi_statement_count;
    END IF;
END
$verify$;
