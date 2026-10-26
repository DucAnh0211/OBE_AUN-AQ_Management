"""Integration checks for the merged Identity/Accreditation/Curriculum/Reporting API.
Use an isolated EF-initialized database with V1/V2. Creates test users/program/framework.
Requires MERGE_TEST_URL, MERGE_TEST_ADMIN_EMAIL, MERGE_TEST_ADMIN_PASSWORD.
Optional MERGE_TEST_TOKEN_FILE writes the admin token to an ignored local test artifact.
"""
import json
import os
from pathlib import Path
import urllib.request
import urllib.error
import uuid

base = os.environ['MERGE_TEST_URL'].rstrip('/')
tag = uuid.uuid4().hex[:10]
temp_password = 'IsolatedTemporary123!'
password = 'IsolatedChanged456!'
admin = None

def call(path, method='GET', body=None, token=None, status=200, cookie=None):
    headers = {'Content-Type': 'application/json'}
    if token: headers['Authorization'] = 'Bearer ' + token
    if cookie: headers['Cookie'] = cookie
    req = urllib.request.Request(base + path, method=method, headers=headers,
        data=None if body is None else json.dumps(body).encode())
    try: response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error: response = error
    raw = response.read()
    assert response.status == status, (method, path, response.status, raw[:1200])
    return (json.loads(raw) if raw else None, response.headers.get('Set-Cookie', '').split(';')[0])

def api(path, method='GET', body=None, status=200, token=None):
    return call(path, method, body, token or admin, status)[0]

call('/api/curriculum/programs', status=401)
session, _ = call('/api/auth/login', 'POST', {'email': os.environ['MERGE_TEST_ADMIN_EMAIL'], 'password': os.environ['MERGE_TEST_ADMIN_PASSWORD']})
admin = session['accessToken']
if session['user']['mustChangePassword']:
    api('/api/curriculum/programs', status=403)
    session, _ = call('/api/auth/change-password', 'POST', {'currentPassword': os.environ['MERGE_TEST_ADMIN_PASSWORD'], 'newPassword': password}, admin)
    admin = session['accessToken']
assert api('/api/reporting/session')['canGenerate']
assert api('/api/reporting/programs')
call('/api/admin/users', token='local-test-author-token-32-characters-only', status=401)
api('/api/admin/users?q=merge')

def user(role):
    email = f'{role}-{tag}@example.test'
    created = api('/api/admin/users', 'POST', {'email': email, 'fullName': 'Integration '+role, 'role': role, 'temporaryPassword': temp_password}, 201)
    api('/api/admin/users', 'POST', {'email': email, 'fullName': 'Duplicate', 'role': role, 'temporaryPassword': temp_password}, 409)
    logged, _ = call('/api/auth/login', 'POST', {'email': email, 'password': temp_password})
    call('/api/curriculum/programs', token=logged['accessToken'], status=403)
    changed, cookie = call('/api/auth/change-password', 'POST', {'currentPassword': temp_password, 'newPassword': password}, logged['accessToken'])
    return created, changed['accessToken'], cookie

lecturer, lecturer_token, lecturer_cookie = user('lecturer')
student, student_token, _ = user('student')
for token in (lecturer_token, student_token):
    call('/api/admin/users', token=token, status=403)
    call('/api/reporting/session', token=token, status=403)

program = api('/api/curriculum/programs', 'POST', {'code': 'MERGE'+tag, 'name': 'EF merge integration'}, 201)
pid = program['id']
version = api(f'/api/curriculum/programs/{pid}/versions', 'POST', {'versionCode': 'TEST1'}, 201)
vid = version['id']
course = api(f'/api/curriculum/program-versions/{vid}/courses', 'POST', {'name': 'Assigned course', 'credits': 3, 'semester': '1'}, 201)
cid = course['id']
unassigned = api(f'/api/curriculum/program-versions/{vid}/courses', 'POST', {'name': 'Other course', 'credits': 2, 'semester': '1'}, 201)
api(f'/api/admin/users/{lecturer["id"]}/course-assignments', 'PUT', {'ids': [cid]}, 204)
api(f'/api/admin/users/{student["id"]}/program-assignments', 'PUT', {'ids': [pid]}, 204)
api(f'/api/admin/users/{student["id"]}/course-assignments', 'PUT', {'ids': [cid]}, 409)
api(f'/api/admin/users/{lecturer["id"]}/course-assignments', 'PUT', {'ids': [9223372036854770000]}, 400)
assert api(f'/api/admin/users/{lecturer["id"]}/course-assignments') == [cid]
course_path = f'/api/curriculum/program-versions/{vid}/courses/{cid}'
call(course_path, token=student_token, status=403)
call(course_path, token=lecturer_token)
call(f'/api/curriculum/program-versions/{vid}/courses/{unassigned["id"]}', token=lecturer_token, status=403)
clo = api(course_path+'/clos', 'POST', {'code': 'CLO1', 'statement': 'Apply the course knowledge', 'levelCode': 'C3'}, 201, lecturer_token)
api(f'/api/curriculum/program-versions/{vid}/publish', 'POST')
call(course_path, token=student_token)
call(course_path+'/clos', 'POST', {'code': 'CLO2', 'statement': 'Cannot edit published', 'levelCode': 'C3'}, lecturer_token, 403)
cloned = api(f'/api/curriculum/programs/{pid}/versions', 'POST', {'versionCode': 'TEST2', 'sourceVersionId': vid}, 201)
assigned = api(f'/api/admin/users/{lecturer["id"]}/course-assignments')
assert len(assigned) == 2 and cid in assigned, assigned
clone_courses = api(f'/api/curriculum/program-versions/{cloned["id"]}/courses')['items']
clone_id = next(c['id'] for c in clone_courses if c['name'] == 'Assigned course')
assert clone_id in assigned
call(f'/api/curriculum/program-versions/{cloned["id"]}', token=student_token, status=403)

frameworks = api('/api/accreditation/frameworks')
canonical = next(f for f in frameworks if f['code'] == 'AUN-QA-PROGRAMME' and f['version'] == '4.0')
tree = api(f'/api/accreditation/frameworks/{canonical["id"]}/tree?language=en')
assert tree['criterionCount'] == 8 and tree['requirementCount'] == 53
call(f'/api/accreditation/frameworks/{canonical["id"]}/tree', token=lecturer_token)
call('/api/accreditation/frameworks', token=student_token, status=403)
f = api('/api/accreditation/frameworks', 'POST', {'code': 'TEST-'+tag, 'version': '1', 'sourceTitle': 'Integration fixture'}, 201)
fp = f'/api/accreditation/frameworks/{f["id"]}'
call(fp, token=lecturer_token, status=403)
c = api(fp+'/criteria', 'POST', {'code': '1', 'displayOrder': 1}, 201)
r = api(fp+f'/criteria/{c["id"]}/requirements', 'POST', {'code': '1.1', 'displayOrder': 1, 'sourcePage': 1}, 201)
api(fp+f'/criteria/{c["id"]}/translations/en', 'PUT', {'title': 'English title', 'status': 'official'}, 204)
api(fp+f'/requirements/{r["id"]}/translations/en', 'PUT', {'statement': 'English requirement', 'status': 'official'}, 204)
fallback = api(fp+'/tree?language=fr')['criteria'][0]
assert fallback['title'] == 'English title' and fallback['translationStatus'] == 'fallback_en'
api(fp+'/criteria', 'POST', {'code': '1', 'displayOrder': 2}, 409)
api(fp+'/criteria', 'POST', {'code': '2', 'displayOrder': 0}, 400)
api(fp+f'/requirements/{r["id"]}', 'PUT', {'code': '2.1', 'displayOrder': 1}, 400)
api(fp+'/publish', 'POST')
call(fp, token=lecturer_token)
api(fp+'/criteria', 'POST', {'code': '2', 'displayOrder': 2}, 409)
api(fp+f'/criteria/{c["id"]}/translations/en', 'PUT', {'title': 'Immutable'}, 409)
api(fp+f'/criteria/{c["id"]}/translations/vi', 'PUT', {'title': 'Tiêu chí thử nghiệm', 'status': 'reviewed'}, 204)
api(fp+'/retire', 'POST')
call(fp, token=lecturer_token, status=403)

payload = {'code': 'IMPORT-'+tag, 'version': '1', 'sourceTitle': 'Integration import', 'criteria': [{'code': '1', 'displayOrder': 1,
    'translations': {'en': {'title': 'Imported', 'status': 'official'}},
    'requirements': [{'code': '1.1', 'displayOrder': 1, 'translations': {'en': {'statement': 'Imported requirement', 'status': 'official'}}}]}]}
assert api('/api/accreditation/framework-imports/preview', 'POST', payload)['valid']
imported = api('/api/accreditation/framework-imports', 'POST', payload, 201)
imported_path = f'/api/accreditation/frameworks/{imported["id"]}'
import_tree = api(imported_path+'/tree?language=en')
assert import_tree['criteria'][0]['requirements'][0]['statement'] == 'Imported requirement'
api(imported_path+f'/requirements/{import_tree["criteria"][0]["requirements"][0]["id"]}', 'DELETE', status=204)
api(imported_path+f'/criteria/{import_tree["criteria"][0]["id"]}', 'DELETE', status=204)
api(imported_path, 'DELETE', status=204)

refreshed, _ = call('/api/auth/refresh', 'POST', cookie=lecturer_cookie)
call('/api/auth/refresh', 'POST', cookie=lecturer_cookie, status=401)
call('/api/auth/me', token=refreshed['accessToken'], status=401)
api(f'/api/admin/users/{student["id"]}', 'PUT', {'fullName': student['fullName'], 'role': 'student', 'status': 'disabled'})
call('/api/auth/me', token=student_token, status=401)
assert api('/api/admin/audit-logs')['totalItems'] > 0
api(f'/api/curriculum/programs/{pid}', 'DELETE', status=204)
if output := os.environ.get('MERGE_TEST_TOKEN_FILE'):
    Path(output).write_text(admin, encoding='utf-8')
print('PASS merged EF: authentication, password change, role/assignment scope, clone assignments, JWT reporting, framework CRUD/import/translations/state, refresh replay and token invalidation')
