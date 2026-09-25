BEGIN;

CREATE SCHEMA IF NOT EXISTS security;

CREATE TABLE IF NOT EXISTS security.users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email text NOT NULL,
    normalized_email text NOT NULL UNIQUE,
    full_name text NOT NULL,
    password_hash text NOT NULL,
    role text NOT NULL,
    status text NOT NULL DEFAULT 'active',
    must_change_password boolean NOT NULL DEFAULT true,
    token_version integer NOT NULL DEFAULT 1,
    failed_login_count integer NOT NULL DEFAULT 0,
    locked_until timestamptz,
    last_login_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT security_users_role_ck CHECK (role IN ('admin', 'lecturer', 'student')),
    CONSTRAINT security_users_status_ck CHECK (status IN ('active', 'disabled')),
    CONSTRAINT security_users_failed_login_ck CHECK (failed_login_count >= 0)
);

CREATE TABLE IF NOT EXISTS security.refresh_tokens (
    id uuid PRIMARY KEY,
    user_id bigint NOT NULL REFERENCES security.users(id) ON DELETE CASCADE,
    token_hash char(64) NOT NULL UNIQUE,
    expires_at timestamptz NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    revoked_at timestamptz,
    replaced_by_token_id uuid REFERENCES security.refresh_tokens(id),
    created_by_ip text,
    revoked_by_ip text
);

CREATE TABLE IF NOT EXISTS security.lecturer_course_assignments (
    user_id bigint NOT NULL REFERENCES security.users(id) ON DELETE CASCADE,
    program_course_id bigint NOT NULL REFERENCES curriculum.program_courses(id) ON DELETE CASCADE,
    assigned_by bigint REFERENCES security.users(id),
    assigned_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, program_course_id)
);

CREATE TABLE IF NOT EXISTS security.student_program_assignments (
    user_id bigint NOT NULL REFERENCES security.users(id) ON DELETE CASCADE,
    program_id bigint NOT NULL REFERENCES curriculum.programs(id) ON DELETE CASCADE,
    assigned_by bigint REFERENCES security.users(id),
    assigned_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, program_id)
);

CREATE TABLE IF NOT EXISTS security.audit_logs (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    actor_user_id bigint REFERENCES security.users(id),
    action text NOT NULL,
    target_type text NOT NULL,
    target_id text,
    details jsonb NOT NULL DEFAULT '{}'::jsonb,
    ip_address text,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS refresh_tokens_user_ix
    ON security.refresh_tokens(user_id, expires_at DESC);
CREATE INDEX IF NOT EXISTS lecturer_assignments_course_ix
    ON security.lecturer_course_assignments(program_course_id, user_id);
CREATE INDEX IF NOT EXISTS student_assignments_program_ix
    ON security.student_program_assignments(program_id, user_id);
CREATE INDEX IF NOT EXISTS audit_logs_created_ix
    ON security.audit_logs(created_at DESC, id DESC);

COMMIT;
