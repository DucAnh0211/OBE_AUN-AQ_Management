BEGIN;

CREATE SCHEMA IF NOT EXISTS accreditation;

CREATE TABLE IF NOT EXISTS accreditation.aun_frameworks (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code text NOT NULL,
    version text NOT NULL,
    assessment_level text NOT NULL DEFAULT 'programme',
    default_language text NOT NULL DEFAULT 'vi',
    status text NOT NULL DEFAULT 'draft',
    source_title text NOT NULL,
    source_url text,
    content_hash char(64),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    published_at timestamptz,
    retired_at timestamptz,
    CONSTRAINT aun_frameworks_code_version_uq UNIQUE (code, version),
    CONSTRAINT aun_frameworks_status_ck CHECK (status IN ('draft', 'published', 'retired')),
    CONSTRAINT aun_frameworks_level_ck CHECK (assessment_level = 'programme')
);

CREATE TABLE IF NOT EXISTS accreditation.aun_criteria (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    framework_id bigint NOT NULL REFERENCES accreditation.aun_frameworks(id) ON DELETE CASCADE,
    code text NOT NULL,
    display_order integer NOT NULL CHECK (display_order > 0),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (framework_id, code),
    UNIQUE (framework_id, display_order)
);

CREATE TABLE IF NOT EXISTS accreditation.aun_requirements (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    criterion_id bigint NOT NULL REFERENCES accreditation.aun_criteria(id) ON DELETE CASCADE,
    code text NOT NULL,
    display_order integer NOT NULL CHECK (display_order > 0),
    source_page integer CHECK (source_page IS NULL OR source_page > 0),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (criterion_id, code),
    UNIQUE (criterion_id, display_order)
);

CREATE TABLE IF NOT EXISTS accreditation.aun_criterion_translations (
    criterion_id bigint NOT NULL REFERENCES accreditation.aun_criteria(id) ON DELETE CASCADE,
    language_code text NOT NULL,
    title text NOT NULL,
    description text,
    translation_status text NOT NULL DEFAULT 'draft',
    translation_source text,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (criterion_id, language_code),
    CHECK (translation_status IN ('official', 'reviewed', 'draft', 'machine_translated'))
);

CREATE TABLE IF NOT EXISTS accreditation.aun_requirement_translations (
    requirement_id bigint NOT NULL REFERENCES accreditation.aun_requirements(id) ON DELETE CASCADE,
    language_code text NOT NULL,
    statement text NOT NULL,
    guidance text,
    translation_status text NOT NULL DEFAULT 'draft',
    translation_source text,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (requirement_id, language_code),
    CHECK (translation_status IN ('official', 'reviewed', 'draft', 'machine_translated'))
);

CREATE INDEX IF NOT EXISTS aun_frameworks_status_ix
    ON accreditation.aun_frameworks(status, created_at DESC);
CREATE INDEX IF NOT EXISTS aun_criteria_framework_ix
    ON accreditation.aun_criteria(framework_id, display_order);
CREATE INDEX IF NOT EXISTS aun_requirements_criterion_ix
    ON accreditation.aun_requirements(criterion_id, display_order);

-- Canonical v4.0 structure and official English requirement statements from
-- Appendix A of the October 2020 Programme Assessment Guide.
INSERT INTO accreditation.aun_frameworks
    (code, version, assessment_level, default_language, status, source_title, source_url, published_at)
VALUES
    ('AUN-QA-PROGRAMME', '4.0', 'programme', 'vi', 'published',
     'Guide to AUN-QA Assessment at Programme Level Version 4.0',
     'https://www.aunsec.org/application/files/2816/7290/3752/Guide_to_AUN-QA_Assessment_at_Programme_Level_Version_4.0_4.pdf',
     now())
ON CONFLICT (code, version) DO NOTHING;

WITH framework AS (
    SELECT id FROM accreditation.aun_frameworks
    WHERE code = 'AUN-QA-PROGRAMME' AND version = '4.0'
), seed(code, display_order, title_vi, title_en) AS (
    VALUES
      ('1',1,'Kết quả học tập mong đợi','Expected Learning Outcomes'),
      ('2',2,'Cấu trúc và nội dung chương trình dạy học','Programme Structure and Content'),
      ('3',3,'Phương thức dạy và học','Teaching and Learning Approach'),
      ('4',4,'Đánh giá kết quả học tập','Student Assessment'),
      ('5',5,'Đội ngũ giảng viên','Academic Staff'),
      ('6',6,'Các dịch vụ hỗ trợ người học','Student Support Services'),
      ('7',7,'Cơ sở hạ tầng và trang thiết bị','Facilities and Infrastructure'),
      ('8',8,'Đầu ra và kết quả đạt được','Output and Outcomes')
)
INSERT INTO accreditation.aun_criteria(framework_id, code, display_order)
SELECT framework.id, seed.code, seed.display_order FROM framework CROSS JOIN seed
ON CONFLICT (framework_id, code) DO NOTHING;

WITH seed(code, title_vi, title_en) AS (
    VALUES
      ('1','Kết quả học tập mong đợi','Expected Learning Outcomes'),
      ('2','Cấu trúc và nội dung chương trình dạy học','Programme Structure and Content'),
      ('3','Phương thức dạy và học','Teaching and Learning Approach'),
      ('4','Đánh giá kết quả học tập','Student Assessment'),
      ('5','Đội ngũ giảng viên','Academic Staff'),
      ('6','Các dịch vụ hỗ trợ người học','Student Support Services'),
      ('7','Cơ sở hạ tầng và trang thiết bị','Facilities and Infrastructure'),
      ('8','Đầu ra và kết quả đạt được','Output and Outcomes')
)
INSERT INTO accreditation.aun_criterion_translations
    (criterion_id, language_code, title, translation_status, translation_source)
SELECT c.id, lang.code,
       CASE lang.code WHEN 'vi' THEN seed.title_vi ELSE seed.title_en END,
       CASE lang.code WHEN 'en' THEN 'official' ELSE 'draft' END,
       CASE lang.code WHEN 'en' THEN 'AUN-QA Guide v4.0' ELSE 'Khoa học máy tính SAR' END
FROM accreditation.aun_criteria c
JOIN accreditation.aun_frameworks f ON f.id = c.framework_id
JOIN seed ON seed.code = c.code
CROSS JOIN (VALUES ('vi'), ('en')) lang(code)
WHERE f.code = 'AUN-QA-PROGRAMME' AND f.version = '4.0'
ON CONFLICT (criterion_id, language_code) DO UPDATE
SET title = EXCLUDED.title,
    translation_source = EXCLUDED.translation_source,
    updated_at = now()
WHERE aun_criterion_translations.translation_status = 'draft'
  AND aun_criterion_translations.title LIKE '%?%';

WITH counts(code, requirement_count) AS (
    VALUES ('1',5),('2',7),('3',6),('4',7),('5',8),('6',6),('7',9),('8',5)
)
INSERT INTO accreditation.aun_requirements(criterion_id, code, display_order)
SELECT c.id, c.code || '.' || n.value, n.value
FROM accreditation.aun_criteria c
JOIN accreditation.aun_frameworks f ON f.id = c.framework_id
JOIN counts ON counts.code = c.code
CROSS JOIN LATERAL generate_series(1, counts.requirement_count) n(value)
WHERE f.code = 'AUN-QA-PROGRAMME' AND f.version = '4.0'
ON CONFLICT (criterion_id, code) DO NOTHING;

WITH official_statements(code, source_page, statement) AS (
    VALUES
      ('1.1',63,'The programme to show that the expected learning outcomes are appropriately formulated in accordance with an established learning taxonomy, are aligned to the vision and mission of the university, and are known to all stakeholders.'),
      ('1.2',63,'The programme to show that the expected learning outcomes for all courses are appropriately formulated and are aligned to the expected learning outcomes of the programme.'),
      ('1.3',63,'The programme to show that the expected learning outcomes consist of both generic outcomes (related to written and oral communication, problem-solving, information technology, teambuilding skills, etc) and subject specific outcomes (related to knowledge and skills of the study discipline).'),
      ('1.4',63,'The programme to show that the requirements of the stakeholders, especially the external stakeholders, are gathered, and that these are reflected in the expected learning outcomes.'),
      ('1.5',63,'The programme to show that the expected learning outcomes are achieved by the students by the time they graduate.'),
      ('2.1',63,'The specifications of the programme and all its courses are shown to be comprehensive, up-to-date, and made available and communicated to all stakeholders.'),
      ('2.2',63,'The design of the curriculum is shown to be constructively aligned with achieving the expected learning outcomes.'),
      ('2.3',63,'The design of the curriculum is shown to include feedback from stakeholders, especially external stakeholders.'),
      ('2.4',63,'The contribution made by each course in achieving the expected learning outcomes is shown to be clear.'),
      ('2.5',63,'The curriculum to show that all its courses are logically structured, properly sequenced (progression from basic to intermediate to specialised courses), and are integrated.'),
      ('2.6',64,'The curriculum to have option(s) for students to pursue major and/or minor specialisations.'),
      ('2.7',64,'The programme to show that its curriculum is reviewed periodically following an established procedure and that it remains up-to-date and relevant to industry.'),
      ('3.1',64,'The educational philosophy is shown to be articulated and communicated to all stakeholders. It is also shown to be reflected in the teaching and learning activities.'),
      ('3.2',64,'The teaching and learning activities are shown to allow students to participate responsibly in the learning process.'),
      ('3.3',64,'The teaching and learning activities are shown to involve active learning by the students.'),
      ('3.4',64,'The teaching and learning activities are shown to promote learning, learning how to learn, and instilling in students a commitment for life-long learning (e.g., commitment to critical inquiry, information-processing skills, and a willingness to experiment with new ideas and practices).'),
      ('3.5',64,'The teaching and learning activities are shown to inculcate in students, new ideas, creative thought, innovation, and an entrepreneurial mindset.'),
      ('3.6',64,'The teaching and learning processes are shown to be continuously improved to ensure their relevance to the needs of industry and are aligned to the expected learning outcomes.'),
      ('4.1',64,'A variety of assessment methods are shown to be used and are shown to be constructively aligned to achieving the expected learning outcomes and the teaching and learning objectives.'),
      ('4.2',64,'The assessment and assessment-appeal policies are shown to be explicit, communicated to students, and applied consistently.'),
      ('4.3',64,'The assessment standards and procedures for student progression and degree completion, are shown to be explicit, communicated to students, and applied consistently.'),
      ('4.4',64,'The assessments methods are shown to include rubrics, marking schemes, timelines, and regulations, and these are shown to ensure validity, reliability, and fairness in assessment.'),
      ('4.5',65,'The assessment methods are shown to measure the achievement of the expected learning outcomes of the programme and its courses.'),
      ('4.6',65,'Feedback of student assessment is shown to be provided in a timely manner.'),
      ('4.7',65,'The student assessment and its processes are shown to be continuously reviewed and improved to ensure their relevance to the needs of industry and alignment to the expected learning outcomes.'),
      ('5.1',65,'The programme to show that academic staff planning (including succession, promotion, re-deployment, termination, and retirement plans) is carried out to ensure that the quality and quantity of the academic staff fulfil the needs for education, research, and service.'),
      ('5.2',65,'The programme to show that staff workload is measured and monitored to improve the quality of education, research, and service.'),
      ('5.3',65,'The programme to show that the competences of the academic staff are determined, evaluated, and communicated.'),
      ('5.4',65,'The programme to show that the duties allocated to the academic staff are appropriate to qualifications, experience, and aptitude.'),
      ('5.5',65,'The programme to show that promotion of the academic staff is based on a merit system which accounts for teaching, research, and service.'),
      ('5.6',65,'The programme to show that the rights and privileges, benefits, roles and relationships, and accountability of the academic staff, taking into account professional ethics and their academic freedom, are well defined and understood.'),
      ('5.7',65,'The programme to show that the training and developmental needs of the academic staff are systematically identified, and that appropriate training and development activities are implemented to fulfil the identified needs.'),
      ('5.8',65,'The programme to show that performance management including reward and recognition is implemented to assess academic staff teaching and research quality.'),
      ('6.1',66,'The student intake policy, admission criteria, and admission procedures to the programme are shown to be clearly defined, communicated, published, and up-to-date.'),
      ('6.2',66,'Both short-term and long-term planning of academic and non-academic support services are shown to be carried out to ensure sufficiency and quality of support services for teaching, research, and community service.'),
      ('6.3',66,'An adequate system is shown to exist for student progress, academic performance, and workload monitoring. Student progress, academic performance, and workload are shown to be systematically recorded and monitored. Feedback to students and corrective actions are made where necessary.'),
      ('6.4',66,'Co-curricular activities, student competition, and other student support services are shown to be available to improve learning experience and employability.'),
      ('6.5',66,'The competences of the support staff rendering student services are shown to be identified for recruitment and deployment. These competences are shown to be evaluated to ensure their continued relevance to stakeholders needs. Roles and relationships are shown to be well-defined to ensure smooth delivery of the services.'),
      ('6.6',66,'Student support services are shown to be subjected to evaluation, benchmarking, and enhancement.'),
      ('7.1',66,'The physical resources to deliver the curriculum, including equipment, material, and information technology, are shown to be sufficient.'),
      ('7.2',66,'The laboratories and equipment are shown to be up-to-date, readily available, and effectively deployed.'),
      ('7.3',66,'A digital library is shown to be set-up, in keeping with progress in information and communication technology.'),
      ('7.4',66,'The information technology systems are shown to be set up to meet the needs of staff and students.'),
      ('7.5',66,'The university is shown to provide a highly accessible computer and network infrastructure that enables the campus community to fully exploit information technology for teaching, research, service, and administration.'),
      ('7.6',67,'The environmental, health, and safety standards and access for people with special needs are shown to be defined and implemented.'),
      ('7.7',67,'The university is shown to provide a physical, social, and psychological environment that is conducive for education, research, and personal well-being.'),
      ('7.8',67,'The competences of the support staff rendering services related to facilities are shown to be identified and evaluated to ensure that their skills remain relevant to stakeholder needs.'),
      ('7.9',67,'The quality of the facilities (library, laboratory, IT, and student services) are shown to be subjected to evaluation and enhancement.'),
      ('8.1',67,'The pass rate, dropout rate, and average time to graduate are shown to be established, monitored, and benchmarked for improvement.'),
      ('8.2',67,'Employability as well as self-employment, entrepreneurship, and advancement to further studies, are shown to be established, monitored, and benchmarked for improvement.'),
      ('8.3',67,'Research and creative work output and activities carried out by the academic staff and students, are shown to be established, monitored, and benchmarked for improvement.'),
      ('8.4',67,'Data are provided to show directly the achievement of the programme outcomes, which are established and monitored.'),
      ('8.5',67,'Satisfaction level of the various stakeholders are shown to be established, monitored, and benchmarked for improvement.')
), official_requirements AS (
    SELECT r.id, official_statements.source_page, official_statements.statement
    FROM accreditation.aun_requirements r
    JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
    JOIN accreditation.aun_frameworks f ON f.id = c.framework_id
    JOIN official_statements ON official_statements.code = r.code
    WHERE f.code = 'AUN-QA-PROGRAMME' AND f.version = '4.0'
), updated_requirements AS (
    UPDATE accreditation.aun_requirements r
    SET source_page = official_requirements.source_page,
        updated_at = now()
    FROM official_requirements
    WHERE r.id = official_requirements.id
    RETURNING r.id
)
INSERT INTO accreditation.aun_requirement_translations
    (requirement_id, language_code, statement, translation_status, translation_source)
SELECT official_requirements.id, 'en', official_requirements.statement,
       'official', 'AUN-QA Programme Assessment Guide v4.0 (October 2020), Appendix A'
FROM official_requirements
JOIN updated_requirements ON updated_requirements.id = official_requirements.id
ON CONFLICT (requirement_id, language_code) DO UPDATE
SET statement = EXCLUDED.statement,
    translation_status = EXCLUDED.translation_status,
    translation_source = EXCLUDED.translation_source,
    updated_at = now()
WHERE aun_requirement_translations.translation_source =
          'Structural seed; replace with reviewed wording from AUN-QA Guide v4.0'
   OR aun_requirement_translations.statement LIKE
          'AUN-QA requirement % — refer to the official Programme Assessment Guide v4.0.';

COMMIT;
