"""Restore a supplied backup into the explicitly disposable test container; compare gradebook/mapping data before and after migration.
Scope: migration AddSemesterClassesAndCourseOfferings only — migrate the copy with `dotnet ef database update AddSemesterClassesAndCourseOfferings`.
The later AlignAcademicRecordsWithRegulations migration intentionally rewrites these tables; see academic_records_migration_smoke.py."""
import hashlib, json, os, subprocess, sys
from pathlib import Path
container='obe-academic-test-db'; database='offering_migration_test'
def run(*args, **kw):
    r=subprocess.run(args, capture_output=True, text=True, encoding='utf-8', **kw)
    assert r.returncode==0,r.stderr or r.stdout
    return r.stdout.strip()
def sql(q): return run('docker','exec','-i',container,'psql','-v','ON_ERROR_STOP=1','-U','academic_test','-d',database,'-At',input=q)
def footprint():
    result={}
    for table in ['academic_students','course_enrollments','student_grades','grade_changes','assessment_components','clos','plos','course_plos','clo_plos','program_courses']:
        # The sole permitted change to pre-existing rows is the new enrollment offering reference.
        rows=sql(f"SELECT COALESCE(jsonb_agg(to_jsonb(t) - 'OfferingId' ORDER BY (to_jsonb(t) - 'OfferingId')::text), '[]'::jsonb) FROM curriculum.{table} t")
        result[table]=hashlib.sha256(rows.encode()).hexdigest()
    return result
if len(sys.argv)>1 and sys.argv[1]=='prepare':
    backup=Path(sys.argv[2]).resolve(); assert backup.is_file()
    run('docker','exec',container,'createdb','-U','academic_test',database)
    sql('CREATE SCHEMA curriculum')
    run('docker','cp',str(backup),container+':/tmp/offering-before.dump')
    run('docker','exec',container,'pg_restore','--exit-on-error','-U','academic_test','-d',database,'--no-owner','--no-privileges','--schema=curriculum','/tmp/offering-before.dump')
    Path('tmp/academic-test').mkdir(parents=True,exist_ok=True)
    Path('tmp/academic-test/offering-migration-before.json').write_text(json.dumps(footprint()))
    print('Prepared isolated copy of current curriculum and gradebooks')
else:
    assert footprint()==json.loads(Path('tmp/academic-test/offering-migration-before.json').read_text())
    assert sql('SELECT count(*) FROM curriculum.course_enrollments e LEFT JOIN curriculum.course_offerings o ON o."Id"=e."OfferingId" WHERE o."Id" IS NULL OR o."ProgramCourseId"<>e."ProgramCourseId" OR o."AcademicPeriod"<>e."AcademicPeriod"')=='0'
    assert sql("SELECT count(*) FROM curriculum.course_offerings WHERE \"Code\" !~ '^[0-9]{6}$'")=='0'
    print('PASS migration: original student/enrollment IDs, scores, grade history, assessment scheme and CLO/PLO mappings unchanged; all historical gradebooks assigned to correct semester legacy classes')
