-- Read-only verification for PLO_CLO_59_mon_v2.json.
WITH v AS (
    SELECT pv.id FROM curriculum.program_versions pv
    JOIN curriculum.programs p ON p.id = pv.program_id
    WHERE p.code = 'CS' AND pv.version_code = 'V2'
)
SELECT 'PLO' AS item, COUNT(*) AS actual, 5 AS expected
FROM curriculum.plos p JOIN v ON v.id = p.program_version_id
UNION ALL
SELECT 'Môn', COUNT(*), 59 FROM curriculum.program_courses pc JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'Môn–PLO', COUNT(*), 151 FROM curriculum.course_plos cp
JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'CLO', COUNT(*), 155 FROM curriculum.clos c
JOIN curriculum.program_courses pc ON pc.id = c.program_course_id JOIN v ON v.id = pc.program_version_id
UNION ALL
SELECT 'CLO–PLO', COUNT(*), 210 FROM curriculum.clo_plos x
JOIN curriculum.program_courses pc ON pc.id = x.program_course_id JOIN v ON v.id = pc.program_version_id;

SELECT plo_code, score, mean_score, deviation, exceeds_20_percent
FROM curriculum.plo_balance b
JOIN curriculum.program_versions pv ON pv.id = b.program_version_id
JOIN curriculum.programs p ON p.id = pv.program_id
WHERE p.code = 'CS' AND pv.version_code = 'V2'
ORDER BY plo_code;

-- Must return zero rows: every course meets min(credits, 5).
SELECT pc.source_row, pc.course_name_snapshot, pc.credits, COUNT(cp.id) AS plo_count
FROM curriculum.program_courses pc
LEFT JOIN curriculum.course_plos cp ON cp.program_course_id = pc.id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
WHERE pv.version_code = 'V2'
GROUP BY pc.id
HAVING COUNT(cp.id) < LEAST(pc.credits, 5);

-- Must return zero rows: every CLO supports at least one course PLO.
SELECT pc.source_row, c.code
FROM curriculum.clos c
JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
LEFT JOIN curriculum.clo_plos x ON x.clo_id = c.id
WHERE pv.version_code = 'V2'
GROUP BY pc.source_row, c.id
HAVING COUNT(x.course_plo_id) = 0;

-- Must return zero rows: every course PLO is covered by at least one CLO.
SELECT pc.source_row, p.code
FROM curriculum.course_plos cp
JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
JOIN curriculum.plos p ON p.id = cp.plo_id
LEFT JOIN curriculum.clo_plos x ON x.course_plo_id = cp.id
WHERE pv.version_code = 'V2'
GROUP BY pc.source_row, p.code, cp.id
HAVING COUNT(x.clo_id) = 0;

-- Must return zero rows: each course has two to five CLOs.
SELECT pc.source_row, pc.course_name_snapshot, COUNT(c.id) AS clo_count
FROM curriculum.program_courses pc
LEFT JOIN curriculum.clos c ON c.program_course_id = pc.id
JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
WHERE pv.version_code = 'V2'
GROUP BY pc.id
HAVING COUNT(c.id) < 2 OR COUNT(c.id) > 5;

-- Expected: 55 CLOs each link to two or more PLOs.
SELECT COUNT(*) AS multi_plo_clos, 55 AS expected
FROM (
    SELECT x.clo_id
    FROM curriculum.clo_plos x
    JOIN curriculum.program_courses pc ON pc.id = x.program_course_id
    JOIN curriculum.program_versions pv ON pv.id = pc.program_version_id
    WHERE pv.version_code = 'V2'
    GROUP BY x.clo_id
    HAVING COUNT(x.course_plo_id) > 1
) multi;

-- V1 must remain unchanged.
WITH v AS (
    SELECT id FROM curriculum.program_versions WHERE version_code = 'V1'
)
SELECT
    (SELECT COUNT(*) FROM curriculum.plos p JOIN v ON v.id = p.program_version_id) AS v1_plos,
    (SELECT COUNT(*) FROM curriculum.program_courses pc JOIN v ON v.id = pc.program_version_id) AS v1_courses,
    (SELECT COUNT(*) FROM curriculum.course_plos cp JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id JOIN v ON v.id = pc.program_version_id) AS v1_course_plos,
    (SELECT COUNT(*) FROM curriculum.clos c JOIN curriculum.program_courses pc ON pc.id = c.program_course_id JOIN v ON v.id = pc.program_version_id) AS v1_clos,
    (SELECT COUNT(*) FROM curriculum.clo_plos x JOIN curriculum.program_courses pc ON pc.id = x.program_course_id JOIN v ON v.id = pc.program_version_id) AS v1_clo_plos;
