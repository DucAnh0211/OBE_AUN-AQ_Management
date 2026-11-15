"""Back up the live DB, restore to the isolated container, upgrade, verify every old row.
The live DB is read only. Run after building tools/database.
"""
import datetime
import hashlib
import json
import os
import subprocess
from pathlib import Path
stamp=datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
backup=Path("tmp/backups")/f"school-roles-{stamp}"
backup.mkdir(parents=True,exist_ok=False)
def run(args,**kwargs):
    r=subprocess.run(args,capture_output=True,**kwargs)
    assert r.returncode==0,(args[:4],r.stderr.decode()[:2000] if isinstance(r.stderr,bytes) else r.stderr[:2000])
    return r.stdout
prod="obe-aun-qa-db-1";test="obe-shared-test-db";db=f"obe_school_upgrade_{stamp.replace('-','_')}"
run(["docker","exec",prod,"pg_dump","-U","postgres","-d","obe_management","-Fc","-f","/tmp/obe-school-before.dump"])
dump=backup/"obe_management-before.dump"
run(["docker","cp",f"{prod}:/tmp/obe-school-before.dump",str(dump.resolve())])
(backup/"SHA256.txt").write_text(hashlib.sha256(dump.read_bytes()).hexdigest(),encoding="ascii")
run(["docker","cp",str(dump.resolve()),f"{test}:/tmp/obe-school-before.dump"])
run(["docker","exec",test,"createdb","-U","postgres",db])
run(["docker","exec",test,"pg_restore","-U","postgres","-d",db,"--no-owner","--exit-on-error","/tmp/obe-school-before.dump"])
def sql(query):
    return run(["docker","exec",test,"psql","-U","postgres","-d",db,"-t","-A","-v","ON_ERROR_STOP=1","-c",query],text=True).strip()
tables=json.loads(sql("SELECT json_agg(json_build_array(table_schema,table_name)) FROM information_schema.tables WHERE table_schema IN ('curriculum','security','accreditation','knowledge','reporting') AND table_type='BASE TABLE' AND table_name<>'__EFMigrationsHistory'"))
excluded="ARRAY['school_id','AccessScope','OwnerUserId','OwnerRole','token_version','revoked_at']::text[]"
def fingerprints():
    result={}
    for schema,table in tables:
        result[f"{schema}.{table}"]=sql(f'''SELECT json_build_array(count(*),md5(COALESCE(string_agg(value,E'\\n' ORDER BY value),''))) FROM
            (SELECT ((to_jsonb(x)-{excluded})::text) value FROM "{schema}"."{table}" x) rows''')
    return result
before=fingerprints()
versions=sql("SELECT json_object_agg(id,token_version) FROM security.users")
env={**os.environ,"ConnectionStrings__Postgres":f"Host=127.0.0.1;Port=55439;Database={db};Username=postgres;Password=shared_test_only"}
r=run(["dotnet","tools/database/bin/Debug/net10.0/ObeAunQa.Database.dll","migrate"],env=env,text=True)
assert fingerprints()==before,"An existing business row changed during the school migration"
assert all(v==json.loads(versions)[k]+1 for k,v in json.loads(sql("SELECT json_object_agg(id,token_version) FROM security.users")).items())
assert sql("SELECT count(*) FROM security.users WHERE role NOT IN ('admin','school_admin','lecturer','student')")=="0"
assert sql("SELECT count(*) FROM security.users WHERE role<>'admin' AND school_id IS NULL")=="0"
assert sql("SELECT count(*) FROM security.users WHERE role='admin' AND school_id IS NOT NULL")=="0"
unassigned=[]
for schema,table in tables:
    if schema=="security" and table in ("users","audit_logs","refresh_tokens"):continue
    has_column=sql(f"SELECT count(*) FROM information_schema.columns WHERE table_schema='{schema}' AND table_name='{table}' AND column_name='school_id'")
    if has_column=="1":
        count=sql(f'SELECT count(*) FROM "{schema}"."{table}" WHERE school_id IS NULL')
        if count!="0":unassigned.append([schema,table,count])
assert not unassigned,unassigned
summary={"backup":str(backup),"cloneDatabase":db,"tablesPreserved":len(tables),"fingerprints":before,"migrationOutput":r,"allSchoolResourcesAssigned":True}
(backup/"verification.json").write_text(json.dumps(summary,indent=2),encoding="utf-8")
Path("tmp/school-test/migration.json").write_text(json.dumps(summary),encoding="utf-8")
print(f"PASS live-data migration clone: {len(tables)} old tables preserved, school ownership assigned, passwords unchanged, sessions revoked; backup {backup}")
