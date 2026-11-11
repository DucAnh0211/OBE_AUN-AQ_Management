"""Dependent year → semester dropdowns on the course-offering page; Vite :5173 with isolated mock API, no real data mutation.
SMOKE_CHANNEL selects the browser channel (default msedge; empty = bundled Chromium)."""
import json, os
from pathlib import Path
from urllib.parse import urlparse,parse_qs
from playwright.sync_api import sync_playwright,expect

origin='http://localhost:5173'
periods=[{'name':'2027-2028','semesters':[{'name':'Học kỳ 1','period':'2027-2028-HK1'},{'name':'Học kỳ 2','period':'2027-2028-HK2'},{'name':'Học kỳ hè','period':'2027-2028-HKHE'}]},
 {'name':'2026-2027','semesters':[{'name':'Học kỳ 1','period':'2026-2027-HK1'},{'name':'Học kỳ 2','period':'2026-2027-HK2'},{'name':'Học kỳ hè','period':'2026-2027-HKHE'}]},
 {'name':'2020','semesters':[{'name':'Học kỳ 1','period':'2020-HK1'}]}]
def offering(period): return {'id':1,'versionId':1,'programCourseId':1,'academicPeriod':period,'code':'100001','groupName':'N01','subjectCode':'000001','subjectName':'Học phần thử',
  'credits':3,'lecturers':[{'userId':7,'fullName':'GV Chính','email':'gv@example.test','role':'main'}],'readOnly':False,'studentCount':2,'capacity':40,'targetHomeClassId':None,
  'targetHomeClassCode':None,'gradeStatus':'draft','statusNote':None,'publishedAt':None}
queries=[]
with sync_playwright() as p:
    browser=p.chromium.launch(channel=os.environ.get('SMOKE_CHANNEL','msedge') or None,headless=True); page=browser.new_page(viewport={'width':1440,'height':1000}); errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    def api(route):
        req=route.request; path=urlparse(req.url).path; payload={}
        assert req.method=='GET' or path=='/api/auth/refresh',req.url
        if path=='/api/auth/refresh':payload={'accessToken':'synthetic-token','expiresAt':'2099-01-01T00:00:00Z','user':{'id':1,'email':'period@example.test','fullName':'Period UI','role':'admin','status':'active','mustChangePassword':False,'createdAt':'2026-11-07T00:00:00Z'}}
        elif path.endswith('/academic-periods'):payload=periods
        elif path=='/api/curriculum/program-versions/1':payload={'id':1,'programId':1,'versionCode':'V1','status':'finalized','archivedAt':None}
        elif path=='/api/curriculum/programs/1':payload={'id':1,'universityId':1,'code':'CS','name':'CTĐT kiểm thử'}
        elif path.endswith('/offerings'):
            period=parse_qs(urlparse(req.url).query)['period'][0]; queries.append(period)
            rows=[offering(period)] if period=='2026-2027-HK1' else []
            payload={'items':rows,'page':1,'pageSize':50,'totalItems':len(rows),'totalPages':1 if rows else 0}
        else:raise AssertionError(req.url)
        route.fulfill(status=200,content_type='application/json',body=json.dumps(payload),headers={'Access-Control-Allow-Origin':origin,'Access-Control-Allow-Credentials':'true'})
    page.route('http://localhost:5298/api/**',api)
    page.goto(origin+'/curriculum/versions/1/classes?period=old')   # route cũ chuyển hướng sang lớp học phần
    expect(page).to_have_url(origin+'/curriculum/versions/1/offerings')
    page.goto(origin+'/curriculum/versions/1/offerings?period=2026-2027-HK1')
    batch=page.get_by_role('combobox',name='Năm học',exact=True); semester=page.get_by_role('combobox',name='Học kỳ',exact=True); open_list=page.get_by_role('button',name='Xem lớp học phần',exact=True)
    expect(batch).to_have_value('2026-2027'); expect(semester).to_have_value('2026-2027-HK1'); expect(page.get_by_role('link',name='Học phần thử',exact=True)).to_be_visible()
    expect(page.get_by_role('heading',name='Lớp học phần · 2026-2027-HK1',exact=True)).to_be_visible()
    expect(page.locator('tbody tr').first).to_contain_text('GV Chính'); expect(page.locator('tbody tr').first).to_contain_text('2/40')
    batch.select_option('2027-2028'); expect(semester).to_have_value(''); expect(open_list).to_be_disabled()
    expect(semester).to_be_enabled(); assert semester.locator('option').all_text_contents()==['Chọn học kỳ','Học kỳ 1','Học kỳ 2','Học kỳ hè']
    assert not any(o.startswith('2026') for o in semester.locator('option').evaluate_all('(options)=>options.map(o=>o.value)'))
    semester.select_option('2027-2028-HK2'); expect(open_list).to_be_enabled(); open_list.click(); expect(page).to_have_url(origin+'/curriculum/versions/1/offerings?period=2027-2028-HK2')
    expect(page.get_by_text('Chưa có lớp học phần trong học kỳ này.')).to_be_visible()
    semester.select_option('2027-2028-HKHE'); open_list.click(); expect(page).to_have_url(origin+'/curriculum/versions/1/offerings?period=2027-2028-HKHE')
    semester.select_option('2027-2028-HK2'); open_list.click()
    page.reload(); expect(batch).to_have_value('2027-2028'); expect(semester).to_have_value('2027-2028-HK2')
    batch.select_option('2020'); expect(semester).to_be_enabled(); assert semester.locator('option').count()==2
    semester.select_option('2020-HK1'); open_list.click(); expect(page).to_have_url(origin+'/curriculum/versions/1/offerings?period=2020-HK1')
    batch.select_option('2026-2027'); expect(semester).to_be_enabled(); semester.select_option('2026-2027-HK1'); open_list.click()
    expect(page.get_by_role('link',name='Học phần thử',exact=True)).to_be_visible()
    assert {'2027-2028-HK2','2027-2028-HKHE','2020-HK1','2026-2027-HK1'}<=set(queries)
    folder=Path('tmp/academic-period-ui');folder.mkdir(parents=True,exist_ok=True)
    page.screenshot(path=str(folder/'desktop.png'),full_page=True)
    page.set_viewport_size({'width':390,'height':844});page.screenshot(path=str(folder/'mobile.png'),full_page=True)
    assert page.evaluate('document.documentElement.scrollWidth<=innerWidth')
    assert page.get_by_role('textbox',name='Tìm lớp học phần').bounding_box()['height'] < 70
    box1=batch.bounding_box();box2=semester.bounding_box();assert abs(box1['x']-box2['x']) < 2 and box2['y']>box1['y']
    batch.select_option(''); expect(semester).to_be_disabled(); expect(open_list).to_be_disabled()
    assert not errors,errors
    browser.close()
print('PASS linked dropdowns on course offerings: legacy /classes redirect, preselect/reload, scoped semester options, legacy period IDs, lecturer/capacity columns, mobile layout and no JS errors')
