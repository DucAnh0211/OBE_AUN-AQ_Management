"""Multi-university checks: isolation of codes, the accredited directory and per-user scope.
Run only against an isolated stack with the AUN-QA seed. Creates its own universities and users.
Requires MERGE_TEST_URL, MERGE_TEST_ADMIN_EMAIL, MERGE_TEST_ADMIN_PASSWORD. Standard library only.
"""
from datetime import date, timedelta
import json
import os
import urllib.error
import urllib.request
import uuid

base = os.environ['MERGE_TEST_URL'].rstrip('/')
tag = uuid.uuid4().hex[:8].upper()
temp_password = 'IsolatedTemporary123!'
password = 'IsolatedChanged456!'
today = date.today()


def call(path, method='GET', body=None, token=None, status=200):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    request = urllib.request.Request(base + path, method=method, headers=headers,
        data=None if body is None else json.dumps(body).encode())
    try:
        response = urllib.request.urlopen(request, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    raw = response.read()
    assert response.status == status, (method, path, response.status, raw[:800])
    return json.loads(raw) if raw else None


session = call('/api/auth/login', 'POST', {'email': os.environ['MERGE_TEST_ADMIN_EMAIL'], 'password': os.environ['MERGE_TEST_ADMIN_PASSWORD']})
admin = session['accessToken']
if session['user']['mustChangePassword']:
    admin = call('/api/auth/change-password', 'POST', {'currentPassword': os.environ['MERGE_TEST_ADMIN_PASSWORD'], 'newPassword': password}, admin)['accessToken']


def api(path, method='GET', body=None, status=200, token=None):
    return call(path, method, body, token or admin, status)


cur, acc = '/api/curriculum', '/api/accreditation'
call(cur + '/universities', status=401)
call(acc + '/directory/universities', status=401)

first = api(cur + '/universities', 'POST', {'code': 'A' + tag, 'name': 'Đại học thử nghiệm A'}, 201)
second = api(cur + '/universities', 'POST', {'code': 'B' + tag, 'name': 'Đại học thử nghiệm B'}, 201)
api(cur + '/universities', 'POST', {'code': 'A' + tag, 'name': 'Trùng mã'}, 409)
api(cur + '/universities', 'POST', {'code': '', 'name': 'Thiếu mã'}, 400)
api(f"{cur}/universities/{first['id']}", 'PUT', {'name': 'Đại học thử nghiệm A đổi tên'})
assert api(f"{cur}/universities/{first['id']}")['name'] == 'Đại học thử nghiệm A đổi tên'

# The same programme and course codes coexist in two universities without sharing rows.
one = api(cur + '/programs', 'POST', {'universityId': first['id'], 'code': 'CS', 'name': 'Khoa học máy tính'}, 201)
two = api(cur + '/programs', 'POST', {'universityId': second['id'], 'code': 'CS', 'name': 'Khoa học máy tính'}, 201)
assert one['id'] != two['id'] and one['universityCode'] == first['code'] and two['universityName'] == second['name']
api(cur + '/programs', 'POST', {'universityId': first['id'], 'code': 'CS', 'name': 'Trùng trong trường'}, 409)
api(cur + '/programs', 'POST', {'code': 'NO', 'name': 'Thiếu trường'}, 400)
api(cur + '/programs', 'POST', {'universityId': 999999999, 'code': 'NO', 'name': 'Trường không tồn tại'}, 404)
listed = api(f"{cur}/programs?universityId={first['id']}")
assert [p['id'] for p in listed['items']] == [one['id']], listed


def course_version(program):
    version = api(f"{cur}/programs/{program['id']}/versions", 'POST', {'versionCode': 'V1'}, 201)
    course = api(f"{cur}/program-versions/{version['id']}/courses", 'POST',
        {'institutionalCode': '123456', 'name': f"Lập trình {program['universityCode']}", 'credits': 3, 'semester': '1', 'displayOrder': 1}, 201)
    return version, course


version_one, course_one = course_version(one)
version_two, course_two = course_version(two)
assert course_one['courseId'] != course_two['courseId'], 'Course rows must not be shared between universities'
assert course_one['name'] != course_two['name']

# Nothing is accredited yet, so the default filter is empty and "all" still lists the programme.
assert api(f"{acc}/directory/universities/{first['id']}/programs")['totalItems'] == 0
rows = api(f"{acc}/directory/universities/{first['id']}/programs?status=all")['items']
assert len(rows) == 1 and rows[0]['accreditation'] is None
api(f"{acc}/directory/universities/{first['id']}/programs?status=sai", status=400)
api(f"{acc}/directory/universities/999999999/programs", status=404)

framework = api(acc + '/frameworks?status=published')[0]['id']
record = {'frameworkId': framework, 'status': 'accredited', 'assessedOn': str(today - timedelta(days=30)),
    'validUntil': str(today + timedelta(days=365)), 'verdictRating': 4, 'note': 'Dữ liệu thử nghiệm'}
path = f"{acc}/programs/{one['id']}/accreditations"
api(path, 'POST', {**record, 'status': 'sai'}, 400)
api(path, 'POST', {**record, 'verdictRating': 9}, 400)
api(path, 'POST', {**record, 'validUntil': str(today - timedelta(days=60))}, 400)
api(path, 'POST', {**record, 'frameworkId': None}, 400)
created = api(path, 'POST', record, 201)
assert created['effectiveStatus'] == 'accredited' and created['frameworkCode']
api(path, 'POST', record, 409)
row = api(f"{acc}/directory/universities/{first['id']}/programs")['items'][0]
assert row['program']['id'] == one['id'] and row['accreditation']['verdictRating'] == 4

# A newer self-assessment does not hide an accreditation still in force.
api(path, 'POST', {**record, 'status': 'self_assessed', 'assessedOn': str(today), 'validUntil': None, 'verdictRating': None}, 201)
row = api(f"{acc}/directory/universities/{first['id']}/programs")['items'][0]
assert row['accreditation']['id'] == created['id']
assert len(api(f"{acc}/directory/programs/{one['id']}/accreditations")) == 2

# An expired accreditation leaves the default list but stays visible with its real status.
expired = {**record, 'assessedOn': str(today - timedelta(days=2000)), 'validUntil': str(today - timedelta(days=1))}
api(f"{acc}/programs/{two['id']}/accreditations", 'POST', expired, 201)
assert api(f"{acc}/directory/universities/{second['id']}/programs")['totalItems'] == 0
assert api(f"{acc}/directory/universities/{second['id']}/programs?status=all")['items'][0]['accreditation']['effectiveStatus'] == 'expired'

counts = {u['id']: u for u in api(acc + '/directory/universities?pageSize=100')['items']}
assert counts[first['id']]['accreditedProgramCount'] == 1 and counts[first['id']]['programCount'] == 1
assert counts[second['id']]['accreditedProgramCount'] == 0

# Non-admins stay inside their assigned programme and never see the other university.
api(f"{cur}/program-versions/{version_one['id']}/publish", 'POST', {})
email = f'student-{tag.lower()}@example.test'
student = api('/api/admin/users', 'POST', {'email': email, 'fullName': 'Sinh viên thử nghiệm', 'role': 'student', 'temporaryPassword': temp_password}, 201)
api(f"/api/admin/users/{student['id']}/program-assignments", 'PUT', {'ids': [one['id']]}, 204)
logged = call('/api/auth/login', 'POST', {'email': email, 'password': temp_password})
token = call('/api/auth/change-password', 'POST', {'currentPassword': temp_password, 'newPassword': password}, logged['accessToken'])['accessToken']

visible = api(acc + '/directory/universities', token=token)['items']
assert [u['id'] for u in visible] == [first['id']], visible
assert [u['id'] for u in api(cur + '/universities', token=token)['items']] == [first['id']]
api(f"{cur}/universities/{second['id']}", status=403, token=token)
assert api(f"{acc}/directory/universities/{second['id']}/programs?status=all", token=token)['totalItems'] == 0
assert len(api(f"{acc}/directory/programs/{one['id']}/accreditations", token=token)) == 2
api(f"{acc}/directory/programs/{two['id']}/accreditations", status=403, token=token)
api(path, 'POST', {**record, 'assessedOn': str(today - timedelta(days=7))}, 403, token)
api(cur + '/universities', 'POST', {'code': 'X' + tag, 'name': 'Không có quyền'}, 403, token)

# Records can be corrected and removed; a university only archives once it is empty.
updated = api(f"{acc}/program-accreditations/{created['id']}", 'PUT', {**record, 'verdictRating': 5})
assert updated['verdictRating'] == 5
api(f"{acc}/program-accreditations/{created['id']}", 'DELETE', status=204)
api(f"{acc}/program-accreditations/{created['id']}", 'DELETE', status=404)
api(f"{cur}/universities/{first['id']}", 'DELETE', status=409)
api(f"{cur}/programs/{one['id']}", 'DELETE', status=204)
api(f"{cur}/universities/{first['id']}", 'DELETE', status=204)
assert first['id'] not in {u['id'] for u in api(acc + '/directory/universities?pageSize=100')['items']}
api(f"{cur}/programs/{two['id']}", 'DELETE', status=204)
api(f"{cur}/universities/{second['id']}", 'DELETE', status=204)
print('PASS University directory: code isolation, accredited filter, expiry, standing rule, validation, non-admin scope, archive rules')
