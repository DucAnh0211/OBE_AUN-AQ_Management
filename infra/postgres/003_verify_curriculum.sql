-- Read-only verification after importing PLO_CLO_59_mon.json.
WITH v AS (
    SELECT pv.id FROM curriculum.program_versions pv
    JOIN curriculum.programs p ON p.id = pv.program_id
    WHERE p.code = 'CS' AND pv.version_code = 'V1'
)
SELECT 'PLO' AS item, COUNT(*) AS actual, 5 AS expected
FROM curriculum.plos p JOIN v ON v.id = p.program_version_id
UNION ALL
SELECT 'Môn', COUNT(*), 59 FROM curriculum.program_courses pc JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'Dòng gốc', COUNT(*), 59 FROM curriculum.source_course_rows sr
WHERE sr.import_batch_id = (
    SELECT b.id FROM curriculum.import_batches b JOIN v ON v.id = b.program_version_id
    ORDER BY b.imported_at DESC, b.id DESC LIMIT 1
)
UNION ALL
SELECT 'Môn–PLO', COUNT(*), 146 FROM curriculum.course_plos cp
JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'CLO', COUNT(*), 146 FROM curriculum.clos c
JOIN curriculum.program_courses pc ON pc.id = c.program_course_id JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'CLO–PLO', COUNT(*), 146 FROM curriculum.clo_plos x
JOIN curriculum.program_courses pc ON pc.id = x.program_course_id JOIN v ON v.id = pc.program_version_id;

SELECT plo_code, score, mean_score, deviation, exceeds_20_percent
FROM curriculum.plo_balance b
JOIN curriculum.program_versions pv ON pv.id = b.program_version_id
JOIN curriculum.programs p ON p.id = pv.program_id
WHERE p.code = 'CS' AND pv.version_code = 'V1'
ORDER BY plo_code;

-- Must return zero rows: every course-PLO link has at least one supporting CLO.
SELECT pc.source_row, p.code AS plo_code
FROM curriculum.course_plos cp
JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
JOIN curriculum.plos p ON p.id = cp.plo_id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
JOIN curriculum.programs program ON program.id = pv.program_id
LEFT JOIN curriculum.clo_plos x ON x.course_plo_id = cp.id
WHERE program.code = 'CS' AND pv.version_code = 'V1' AND x.course_plo_id IS NULL;

-- Must return zero rows: the 34 newly mapped courses meet min(credits, 5).
SELECT pc.source_row, pc.credits, COUNT(cp.id) AS plo_count
FROM curriculum.program_courses pc
LEFT JOIN curriculum.course_plos cp ON cp.program_course_id = pc.id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
JOIN curriculum.programs program ON program.id = pv.program_id
WHERE pc.id IN (
    SELECT cp2.program_course_id FROM curriculum.course_plos cp2
    WHERE cp2.provenance = 'selected_from_existing_plos'
) AND program.code = 'CS' AND pv.version_code = 'V1'
GROUP BY pc.id
HAVING COUNT(cp.id) < LEAST(pc.credits, 5);

-- Original links below the credit rule are retained; expect source rows 6, 7, 8, 13.
SELECT pc.source_row, pc.credits, COUNT(cp.id) AS plo_count
FROM curriculum.program_courses pc
JOIN curriculum.course_plos cp ON cp.program_course_id = pc.id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
JOIN curriculum.programs program ON program.id = pv.program_id
WHERE cp.provenance = 'original_curriculum_map'
  AND program.code = 'CS' AND pv.version_code = 'V1'
GROUP BY pc.id
HAVING COUNT(cp.id) < LEAST(pc.credits, 5)
ORDER BY pc.source_row;
