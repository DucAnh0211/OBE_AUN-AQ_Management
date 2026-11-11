"""Migration AlignAcademicRecordsWithRegulations on an explicitly disposable, EMPTY database (never real data).

    1. Create the old schema:   dotnet ef database update AddSemesterClassesAndCourseOfferings --project modules/common/curriculum/backend --context CurriculumDbContext
    2. Seed legacy rows:        python tests/contracts/academic_records_migration_smoke.py prepare
    3. Apply the migration:     dotnet run --project tools/database -- migrate   (or dotnet ef database update)
    4. Verify:                  python tests/contracts/academic_records_migration_smoke.py verify

MIGRATION_DB_CONTAINER / MIGRATION_DB_USER / MIGRATION_DB_NAME select the disposable Postgres container (docker exec psql).
Legacy fixture covers: two offerings of one course in one semester, a LEGACY class, a student in two home classes,
an archived course with grades, edited grade history and a version-level assessment scheme."""
import os, subprocess, sys

container=os.environ.get('MIGRATION_DB_CONTAINER','obe-academic-test-db'); user=os.environ.get('MIGRATION_DB_USER','academic_test'); database=os.environ.get('MIGRATION_DB_NAME','academic_records_migration_test')
def sql(query):
    if url:=os.environ.get('MIGRATION_DATABASE_URL'):   # psycopg instead of docker exec (e.g. from a container on the DB network)
        import psycopg
        with psycopg.connect(url, autocommit=True) as conn:
            cur=conn.execute(query)
            value=None if cur.description is None else cur.fetchone()[0]
            return '' if value is None else str(value)
    r=subprocess.run(['docker','exec','-i',container,'psql','-v','ON_ERROR_STOP=1','-U',user,'-d',database,'-At','-q'],input=query,capture_output=True,text=True,encoding='utf-8')
    assert r.returncode==0,r.stderr
    return r.stdout.strip()
FINGERPRINT="""SELECT count(*) || '|' || sum("Score") || '|' || (SELECT count(*) FROM curriculum.grade_changes) || '|' ||
  (SELECT string_agg(g."EnrollmentId" || ':' || c."Code" || '=' || g."Score", ',' ORDER BY g."EnrollmentId", c."Code")
   FROM curriculum.student_grades g JOIN curriculum.assessment_components c ON c."Id" = g."ComponentId") FROM curriculum.student_grades"""
LEGACY="""SET search_path = curriculum;
DO $$ BEGIN IF EXISTS (SELECT 1 FROM universities) THEN RAISE EXCEPTION 'Refusing to seed: database is not empty'; END IF; END $$;
INSERT INTO universities (id, code, name) OVERRIDING SYSTEM VALUE VALUES (1, 'U1', 'Trường thử');
INSERT INTO programs (id, university_id, code, name) OVERRIDING SYSTEM VALUE VALUES (1, 1, 'P1', 'CNTT');
INSERT INTO program_versions (id, program_id, version_code, dataset_status, is_current, "GradeSchemeRevision") OVERRIDING SYSTEM VALUE
VALUES (1, 1, 'V1', 'finalized', true, 3), (2, 1, 'V2', 'draft', false, 0);
INSERT INTO courses (id, university_id, institutional_code, name) OVERRIDING SYSTEM VALUE VALUES (1, 1, '000101', 'Lập trình'), (2, 1, '000102', 'Cơ sở dữ liệu'), (3, 1, '000103', 'Học phần cũ');
INSERT INTO program_courses (id, program_version_id, course_id, credits, dataset_status, quality_flags, course_code_snapshot, course_name_snapshot, display_order, archived_at)
OVERRIDING SYSTEM VALUE VALUES (1, 1, 1, 3, 'finalized', '[]', '000101', 'Lập trình', 1, NULL), (2, 1, 2, 3, 'finalized', '[]', '000102', 'Cơ sở dữ liệu', 2, NULL),
(3, 1, 3, 2, 'finalized', '[]', '000103', 'Học phần cũ', 3, now());
INSERT INTO assessment_components ("Id", "ProgramVersionId", "Code", "Name", "Weight", "MaxScore", "DisplayOrder") VALUES (1, 1, 'QT', 'Quá trình', 40, 10, 1), (2, 1, 'THI', 'Thi kết thúc', 60, 100, 2);
INSERT INTO academic_classes ("Id", "ProgramVersionId", "AcademicPeriod", "Code", "Name", "CreatedAt") VALUES
(1, 1, '2025-2026-HK1', 'CNTT01', 'CNTT 01 cũ', now() - interval '1 year'), (2, 1, '2025-2026-HK2', 'CNTT01', 'CNTT 01', now()),
(3, 1, '2025-2026-HK1', 'LEGACY', 'Dữ liệu cũ', now()), (4, 1, '2025-2026-HK1', 'CNTT02', 'CNTT 02', now());
INSERT INTO course_offerings ("Id", "AcademicClassId", "ProgramVersionId", "ProgramCourseId", "AcademicPeriod", "LecturerId", "CreatedAt") VALUES
(1, 1, 1, 1, '2025-2026-HK1', 7, now()), (2, 4, 1, 1, '2025-2026-HK1', NULL, now()), (3, 2, 1, 2, '2025-2026-HK2', 8, now()), (4, 3, 1, 3, '2025-2026-HK1', NULL, now());
INSERT INTO academic_students ("Id", "UniversityId", "StudentCode", "FullName", "CreatedAt") VALUES (1, 1, '001', 'Nguyễn An', now()), (2, 1, '002', 'Trần Bình', now()), (3, 1, '003', 'Lê Chi', now());
INSERT INTO course_enrollments ("Id", "ProgramCourseId", "OfferingId", "StudentId", "AcademicPeriod", "Revision", "CreatedAt") VALUES
(1, 1, 1, 1, '2025-2026-HK1', 2, now()), (2, 1, 2, 2, '2025-2026-HK1', 0, now()), (3, 2, 3, 1, '2025-2026-HK2', 1, now()), (4, 2, 3, 2, '2025-2026-HK2', 0, now()), (5, 3, 4, 3, '2025-2026-HK1', 1, now());
INSERT INTO student_grades ("EnrollmentId", "ComponentId", "Score", "UpdatedAt", "UpdatedBy") VALUES (1, 1, 8, now(), 7), (1, 2, 75, now(), 7), (3, 1, 9, now(), 8), (5, 2, 50, now(), 1);
INSERT INTO grade_changes ("EnrollmentId", "ComponentId", "PreviousScore", "NewScore", "ActorId", "ChangedAt", "Source") VALUES
(1, 1, NULL, 7, 7, now(), 'manual'), (1, 1, 7, 8, 7, now(), 'manual'), (1, 2, NULL, 75, 7, now(), 'import'), (3, 1, NULL, 9, 8, now(), 'manual'), (5, 2, NULL, 50, 1, now(), 'manual');
-- Explicit ids above: move identity sequences past them, as on a real database.
SELECT setval(pg_get_serial_sequence('curriculum.' || t, 'Id'), 10) FROM unnest(ARRAY['assessment_components', 'academic_classes', 'course_offerings', 'academic_students', 'course_enrollments']) AS t;
"""
if len(sys.argv)>1 and sys.argv[1]=='prepare':
    sql(LEGACY)
    before=sql(FINGERPRINT); assert before=='4|142.00|5|1:QT=8.00,1:THI=75.00,3:QT=9.00,5:THI=50.00',before
    print('Prepared legacy fixture:',before)
elif len(sys.argv)>1 and sys.argv[1]=='verify':
    assert sql(FINGERPRINT)=='4|142.00|5|1:QT=8.00,1:THI=75.00,3:QT=9.00,5:THI=50.00','scores, history or component codes changed'
    for table in ('student_grades','grade_changes'):
        assert sql(f'SELECT count(*) FROM curriculum.{table} g JOIN curriculum.course_enrollments e ON e."Id"=g."EnrollmentId" JOIN curriculum.assessment_components c ON c."Id"=g."ComponentId" WHERE c."ProgramCourseId"<>e."ProgramCourseId"')=='0',table
    assert sql('SELECT string_agg("ProgramCourseId" || c."Code" || c."Kind", \',\' ORDER BY "ProgramCourseId", c."Code") FROM curriculum.assessment_components c')=='1QTprocess,1THIfinal,2QTprocess,2THIfinal,3QTprocess,3THIfinal'
    assert sql('SELECT string_agg(id || \':\' || grade_scheme_revision, \',\' ORDER BY id) FROM curriculum.program_courses')=='1:3,2:3,3:3'
    assert sql('SELECT string_agg("Code" || \'=\' || "Name", \',\' ORDER BY "Code") FROM curriculum.home_classes')=='CNTT01=CNTT 01,CNTT02=CNTT 02'   # LEGACY bỏ qua, tên mới nhất
    assert sql('SELECT string_agg(o."Id" || \':\' || o."GroupName" || \':\' || COALESCE(h."Code", \'-\'), \',\' ORDER BY o."Id") FROM curriculum.course_offerings o LEFT JOIN curriculum.home_classes h ON h."Id"=o."TargetHomeClassId"')=='1:N01:CNTT01,2:N02:CNTT02,3:N01:CNTT01,4:N01:-'
    assert sql('SELECT string_agg("OfferingId" || \':\' || "UserId" || \':\' || "Role", \',\' ORDER BY "OfferingId") FROM curriculum.offering_lecturers')=='1:7:main,3:8:main'
    assert sql('SELECT string_agg("StudentCode" || \':\' || COALESCE(h."Code", \'-\') || \':\' || "Status", \',\' ORDER BY "StudentCode") FROM curriculum.academic_students s LEFT JOIN curriculum.home_classes h ON h."Id"=s."HomeClassId"')=='001:CNTT01:studying,002:-:studying,003:-:studying'
    assert sql('SELECT count(*) FROM curriculum.course_enrollments WHERE "AttemptKind"<>\'first\' OR "PublishedAt" IS NOT NULL')=='0'
    assert sql('SELECT count(*) FROM curriculum.course_offerings WHERE "GradeStatus"<>\'draft\'')=='0'
    assert sql('SELECT count(*) || \'/\' || (SELECT count(*) FROM curriculum.grade_scales) FROM curriculum.grading_policies')=='1/5'
    assert sql("SELECT count(*) FROM information_schema.tables WHERE table_schema='curriculum' AND table_name='academic_classes'")=='0'
    print('PASS migration: scores/history preserved and remapped to per-course syllabus components, LEGACY classes excluded, home classes merged by code, '
          'N01/N02 groups, lecturers → main, ambiguous students left for admin, TT08 policy seeded')
else:
    sys.exit(__doc__)
