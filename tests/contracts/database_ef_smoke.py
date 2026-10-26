"""Run on an isolated, newly created EMPTY PostgreSQL database only.
Requires ConnectionStrings__Postgres and DATABASE_EF_TEST_ISOLATED=true.
Exercises the same initialize command used by Docker, without requiring Docker.
"""
import os
import subprocess

assert os.environ.get('DATABASE_EF_TEST_ISOLATED') == 'true', 'Use an isolated empty database'
assert os.environ.get('ConnectionStrings__Postgres'), 'Set the isolated database connection'
env = dict(os.environ, Database__SeedDemo='true')

def run(command):
    result = subprocess.run(['dotnet', 'run', '--project', 'tools/database', '--', command],
                            env=env, capture_output=True, text=True, encoding='utf-8')
    assert result.returncode == 0, result.stdout + result.stderr
    return result.stdout

assert 'seeded and verified' in run('initialize'), 'Database must start empty'
assert 'Existing curriculum detected' in run('initialize'), 'Restart must not reseed'
assert 'Existing curriculum detected' in run('seed-demo'), 'Explicit rerun must not overwrite'
assert 'verification passed' in run('verify-demo')
print('PASS EF initialize: migrations + atomic V1/V2 seed + verification + non-destructive rerun')
