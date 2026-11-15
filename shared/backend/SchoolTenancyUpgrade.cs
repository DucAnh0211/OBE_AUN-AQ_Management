namespace ObeAunQa.SharedKernel;

// Runs once, from the final module migration after every schema has school_id.
public static class SchoolTenancyUpgrade
{
    public const string Sql = """
DO $upgrade$
DECLARE r record; b record; child_col text; parent_key text; joined text; pass integer; only_school bigint;
BEGIN
  -- Preserve existing ownership from explicit university foreign keys.
  FOR r IN SELECT table_schema s, table_name t FROM information_schema.columns WHERE column_name='school_id'
    AND table_schema IN ('curriculum','accreditation','reporting','security','knowledge') LOOP
    EXECUTE format('UPDATE %I.%I x SET school_id=COALESCE(to_jsonb(x)->>''UniversityId'',to_jsonb(x)->>''university_id'')::bigint WHERE school_id IS NULL AND COALESCE(to_jsonb(x)->>''UniversityId'',to_jsonb(x)->>''university_id'') IS NOT NULL',r.s,r.t);
  END LOOP;
  FOR pass IN 1..12 LOOP
    -- Inherit ownership through the real FK graph, including composite keys.
    FOR r IN SELECT con.oid, cn.nspname cs, c.relname ct, pn.nspname ps, p.relname pt,
        con.conkey,con.confkey,con.conrelid,con.confrelid
      FROM pg_constraint con JOIN pg_class c ON c.oid=con.conrelid JOIN pg_namespace cn ON cn.oid=c.relnamespace
      JOIN pg_class p ON p.oid=con.confrelid JOIN pg_namespace pn ON pn.oid=p.relnamespace
      WHERE con.contype='f' AND EXISTS(SELECT 1 FROM pg_attribute WHERE attrelid=c.oid AND attname='school_id')
        AND (EXISTS(SELECT 1 FROM pg_attribute WHERE attrelid=p.oid AND attname='school_id') OR pn.nspname='curriculum' AND p.relname='universities') LOOP
      SELECT string_agg(format('c.%I=p.%I',ca.attname,pa.attname),' AND ') INTO joined
        FROM unnest(r.conkey,r.confkey) pair(ck,pk) JOIN pg_attribute ca ON ca.attrelid=r.conrelid AND ca.attnum=pair.ck
          JOIN pg_attribute pa ON pa.attrelid=r.confrelid AND pa.attnum=pair.pk;
      EXECUTE format('UPDATE %I.%I c SET school_id=p.%I FROM %I.%I p WHERE c.school_id IS NULL AND p.%I IS NOT NULL AND %s',
        r.cs,r.ct,CASE WHEN r.pt='universities' THEN 'id' ELSE 'school_id' END,r.ps,r.pt,
        CASE WHEN r.pt='universities' THEN 'id' ELSE 'school_id' END,joined);
    END LOOP;
    -- Cross-module references do not have EF navigations.
    FOR r IN SELECT table_schema s, table_name t FROM information_schema.columns WHERE column_name='school_id'
      AND table_schema IN ('curriculum','accreditation','reporting','security','knowledge') LOOP
      FOR b IN SELECT * FROM (VALUES
        ('UserId','security','users'),('OwnerId','security','users'),('OwnerUserId','security','users'),
        ('RequestedBy','security','users'),('ProgramId','curriculum','programs'),('ProgramVersionId','curriculum','program_versions'),
        ('VersionId','curriculum','program_versions'),('ProgramCourseId','curriculum','program_courses'),
        ('DocumentId','accreditation','source_documents'),('ReportId','reporting','reports')) v(field,ps,pt) LOOP
        SELECT column_name INTO child_col FROM information_schema.columns WHERE table_schema=r.s AND table_name=r.t
          AND replace(lower(column_name),'_','')=lower(b.field) AND (b.field<>'DocumentId' OR data_type='uuid');
        SELECT column_name INTO parent_key FROM information_schema.columns WHERE table_schema=b.ps AND table_name=b.pt AND lower(column_name)='id';
        IF child_col IS NOT NULL AND parent_key IS NOT NULL THEN
          EXECUTE format('UPDATE %I.%I c SET school_id=p.school_id FROM %I.%I p WHERE c.school_id IS NULL AND c.%I=p.%I AND p.school_id IS NOT NULL',r.s,r.t,b.ps,b.pt,child_col,parent_key);
        END IF;
      END LOOP;
    END LOOP;
    -- Infer memberships only when every existing assignment agrees on one school.
    UPDATE security.users u SET school_id=q.school FROM (
      SELECT user_id, min(school_id) school FROM (
        SELECT "UserId" user_id,school_id FROM curriculum.academic_students WHERE "UserId" IS NOT NULL
        UNION ALL SELECT "UserId",school_id FROM curriculum.lecturer_profiles
        UNION ALL SELECT "UserId",school_id FROM curriculum.offering_lecturers
        UNION ALL SELECT user_id,school_id FROM security.lecturer_course_assignments
        UNION ALL SELECT user_id,school_id FROM security.student_program_assignments
        UNION ALL SELECT "UserId",school_id FROM curriculum.org_unit_assignments
      ) memberships WHERE school_id IS NOT NULL GROUP BY user_id HAVING count(DISTINCT school_id)=1
    ) q WHERE u.id=q.user_id AND u.school_id IS NULL AND u.role<>'admin';
  END LOOP;
  -- An unowned legacy resource is unambiguous only in an installation with one school.
  SELECT min(id) INTO only_school FROM curriculum.universities HAVING count(*)=1;
  IF only_school IS NOT NULL THEN
    FOR r IN SELECT table_schema s,table_name t FROM information_schema.columns WHERE column_name='school_id'
      AND table_schema IN ('curriculum','accreditation','reporting','security','knowledge') LOOP
      IF r.s='security' AND r.t='audit_logs' THEN
        UPDATE security.audit_logs a SET school_id=u.school_id FROM security.users u WHERE a.target_type='user' AND a.target_id=u.id::text AND a.school_id IS NULL;
      ELSIF r.s='security' AND r.t='users' THEN
        EXECUTE format('UPDATE security.users SET school_id=%s WHERE school_id IS NULL AND role<>''admin''',only_school);
      ELSE
        EXECUTE format('UPDATE %I.%I SET school_id=%s WHERE school_id IS NULL',r.s,r.t,only_school);
      END IF;
    END LOOP;
  END IF;
  -- Old sessions do not contain a school claim; force a fresh login without changing passwords.
  UPDATE security.users SET token_version=token_version+1;
  UPDATE security.refresh_tokens SET revoked_at=COALESCE(revoked_at,now());
END $upgrade$;
""";
}
