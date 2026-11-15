"""Upgrade the pre-OBE schema with actual demo grades and identity assignments.
Requires the dedicated obe-shared-test-db container, built tools/database and EF CLI.
Creates a new disposable DB; never resets an existing DB.
"""
import json, os, subprocess, uuid

container='obe-shared-test-db'; database='obe_shared_upgrade_'+uuid.uuid4().hex[:8]
env={**os.environ,'ConnectionStrings__Postgres':f'Host=127.0.0.1;Port=55439;Database={database};Username=postgres;Password=shared_test_only','PYTHONUTF8':'1'}
def run(args,stdin=None):
    r=subprocess.run(args,input=stdin,text=True,encoding='utf-8',capture_output=True,env=env)
    assert r.returncode==0,(args,r.stdout[-1500:],r.stderr[-1500:])
    return r.stdout.strip()
def sql(query):return run(['docker','exec','-i',container,'psql','-v','ON_ERROR_STOP=1','-U','postgres','-d',database,'-At'],query)
run(['docker','exec',container,'createdb','-U','postgres',database])
for project,migration in [('modules/common/curriculum/backend','20261110115802_AlignAcademicRecordsWithRegulations'),('modules/common/identity/backend','20261110115833_OfferingLecturerReadAccess')]:
    run(['dotnet','ef','database','update',migration,'--no-build','--project',project])
run(['dotnet','tools/database/bin/Debug/net10.0/ObeAunQa.Database.dll','seed-demo'])
sql("INSERT INTO security.users(email,normalized_email,full_name,password_hash,role,status) VALUES('upgrade@example.test','upgrade@example.test','Upgrade fixture','migration-fixture-not-a-password','lecturer','active');")
sql('INSERT INTO security.lecturer_course_assignments(user_id,program_course_id,assigned_by) SELECT (SELECT id FROM security.users LIMIT 1),id,(SELECT id FROM security.users LIMIT 1) FROM curriculum.program_courses ORDER BY curriculum.program_courses.id LIMIT 1;')
tables=sql("SELECT table_schema||'.'||table_name FROM information_schema.tables WHERE table_schema IN ('curriculum','security') AND table_type='BASE TABLE' AND table_name<>'__EFMigrationsHistory' ORDER BY table_schema,table_name;").splitlines()
def fingerprint(table):
    schema,name=table.split('.')
    return sql(f'SELECT count(*)::text || \':\' || coalesce(md5(string_agg(row_data,\'\' ORDER BY row_data)),\'empty\') FROM (SELECT row_to_json(t)::text row_data FROM "{schema}"."{name}" t) x;')
before={t:fingerprint(t) for t in tables}
assert int(before['curriculum.student_grades'].split(':')[0])>0
run(['dotnet','tools/database/bin/Debug/net10.0/ObeAunQa.Database.dll','migrate'])
after={t:fingerprint(t) for t in tables}
assert before==after,{t:(before[t],after[t]) for t in tables if before[t]!=after[t]}
assert sql("SELECT count(*) FROM curriculum.outcome_snapshots;")=='0'
run(['dotnet','tools/database/bin/Debug/net10.0/ObeAunQa.Database.dll','verify-demo'])
print(f'PASS outcome migration: {len(tables)} existing tables preserved byte-for-byte by row JSON checksums, including grades/history/assignments; DB {database}')
