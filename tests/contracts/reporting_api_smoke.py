"""Exercise UC25-27 against an isolated API/database seeded with V1/V2.

Required env: REPORT_TEST_URL, REPORT_TEST_AUTHOR, REPORT_TEST_READER,
REPORT_TEST_OUTSIDER. Never run against a production API: this creates and revokes reports.
Only Python's standard library is required.
"""
import io
import json
import os
from pathlib import Path
import urllib.error
import urllib.request
import zipfile
import xml.etree.ElementTree as ET

BASE = os.environ["REPORT_TEST_URL"].rstrip("/") + "/api/reporting"
AUTHOR = os.environ["REPORT_TEST_AUTHOR"]
READER = os.environ["REPORT_TEST_READER"]
OUTSIDER = os.environ["REPORT_TEST_OUTSIDER"]
OUTPUT = Path(os.environ.get("REPORT_TEST_ARTIFACTS", "tmp/reporting-test/artifacts"))
OUTPUT.mkdir(parents=True, exist_ok=True)


def request(path, token=AUTHOR, body=None, status=200, binary=False):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(BASE + path, headers=headers,
        data=None if body is None else json.dumps(body).encode("utf-8"))
    try:
        response = urllib.request.urlopen(req, timeout=90)
    except urllib.error.HTTPError as error:
        response = error
    payload = response.read()
    assert response.code == status, (path, response.code, payload[:1000])
    assert response.headers.get("Cache-Control") == "no-store", path
    if binary:
        return payload
    return json.loads(payload) if payload else None


request("/session", token="", status=401)
assert request("/session")["canGenerate"] is True
programs = request("/programs")
assert programs
program_id = programs[0]["id"]
versions = request(f"/programs/{program_id}/versions")
version_id = next(v["id"] for v in versions if v["versionCode"] == "V2")
courses = request(f"/versions/{version_id}/courses")
assert len(courses) == 59, len(courses)

reports = {}
for kind in ("course", "clo", "plo"):
    body = {"kind": kind, "versionId": version_id,
            "programCourseId": courses[0]["id"] if kind == "course" else None}
    report = request("/reports", body=body, status=201)
    reports[kind] = report
    assert request("/reports/" + report["id"]) == report
    if kind != "course":
        assert report["metrics"][0]["denominator"] == 155
        assert report["metrics"][0]["numerator"] == 155
        assert len(report["source"]["cloMappings"]) == 210
    for fmt in ("docx", "pdf"):
        payload = request(f'/reports/{report["id"]}/download?format={fmt}', binary=True)
        (OUTPUT / f"{kind}.{fmt}").write_bytes(payload)
        if fmt == "pdf":
            assert payload.startswith(b"%PDF-")
        else:
            with zipfile.ZipFile(io.BytesIO(payload)) as archive:
                xml = ET.fromstring(archive.read("word/document.xml"))
                assert report["snapshotHash"] in "".join(xml.itertext())

listing = request("/reports?page=1&pageSize=10")
assert listing["totalItems"] >= 3
assert len(request("/reports?page=1&pageSize=1")["items"]) == 1
request("/reports?page=0", status=400)
request("/reports", body={"kind": "course", "versionId": version_id}, status=400)
request("/reports", body={"kind": "course", "versionId": version_id, "programCourseId": 999999}, status=404)
request("/reports", token=READER, body={"kind": "plo", "versionId": version_id}, status=403)
assert request("/reports/" + reports["plo"]["id"], token=READER)["id"] == reports["plo"]["id"]
request("/reports/" + reports["plo"]["id"] + "/revoke", token=READER, body={}, status=403)
assert request("/reports", token=OUTSIDER)["totalItems"] == 0
request("/reports/" + reports["plo"]["id"], token=OUTSIDER, status=404)
request("/reports/" + reports["plo"]["id"] + "/download?format=pdf", token=OUTSIDER, status=404)
request(f"/programs/{program_id}/versions", token=OUTSIDER, status=403)
request("/reports/" + reports["course"]["id"] + "/revoke", body={}, status=204)
request("/reports/" + reports["course"]["id"], status=404)
request("/reports/" + reports["course"]["id"] + "/download?format=docx", status=404)
request("/reports/" + reports["plo"]["id"] + "/download?format=exe", status=400)
(OUTPUT / "reports.json").write_text(json.dumps(reports, ensure_ascii=False), encoding="utf-8")
print("PASS: UC25-27, V2 counts, persisted snapshots, DOCX/PDF, pagination, validation, reader/outsider access, revocation.")
