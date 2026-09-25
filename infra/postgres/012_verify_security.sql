DO $verify$
BEGIN
    IF to_regclass('security.users') IS NULL
       OR to_regclass('security.refresh_tokens') IS NULL
       OR to_regclass('security.lecturer_course_assignments') IS NULL
       OR to_regclass('security.student_program_assignments') IS NULL
       OR to_regclass('security.audit_logs') IS NULL THEN
        RAISE EXCEPTION 'Security schema verification failed: required tables are missing.';
    END IF;
END
$verify$;
