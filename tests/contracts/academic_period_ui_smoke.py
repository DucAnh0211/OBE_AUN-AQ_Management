"""Dependent dropdown regression on Vite :5173 with isolated mock API, no real data mutation."""
import json
from pathlib import Path
from urllib.parse import urlparse,parse_qs
from playwright.sync_api import sync_playwright,expect

periods=[{'name':'2027-2028','semesters':[{'name':'Học kỳ 1','period':'2027-2028-HK1'},{'name':'Học kỳ 2','period':'2027-2028-HK2'},{'name':'Học kỳ hè','period':'2027-2028-HKHE'}]},
 {'name':'2026-2027','semesters':[{'name':'Học kỳ 1','period':'2026-2027-HK1'},{'name':'Học kỳ 2','period':'2026-2027-HK2'},{'name':'Học kỳ hè','period':'2026-2027-HKHE'}]},
 {'name':'2020','semesters':[{'name':'Học kỳ 1','period':'2020-HK1'}]}]
queries=[]; writes=[]
with sync_playwright() as p:
    browser=p.chromium.launch(channel='msedge',headless=True); page=browser.new_page(viewport={'width':1440,'height':1000}); errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    def api(route):
        req=route.request; path=urlparse(req.url).path; payload={}
        if path=='/api/auth/refresh':payload={'accessToken':'synthetic-token','expiresAt':'2099-01-01T00:00:00Z','user':{'id':1,'email':'period@example.test','fullName':'Period UI','role':'admin','status':'active','mustChangePassword':False,'createdAt':'2026-11-07T00:00:00Z'}}
        elif path.endswith('/academic-periods'):payload=periods
        elif path.endswith('/assessment-scheme'):payload={'versionId':1,'revision':1,'locked':False,'components':[{'code':'QT','name':'Quá trình','weight':100,'maxScore':10}]}
        elif path.endswith('/courses/1'):payload={'id':1,'programVersionId':1,'institutionalCode':'000001','name':'Đại số tuyến tính kỹ thuật','archivedAt':None}
        elif path=='/api/curriculum/program-versions/1':payload={'id':1,'versionCode':'V1','status':'finalized','archivedAt':None}
        elif path.endswith('/classes'):
            if req.method=='POST':writes.append(req.post_data_json); payload={'id':2}
            else:
                period=parse_qs(urlparse(req.url).query)['period'][0]; queries.append(period)
                students=[{'id':1,'versionId':1,'code':'A1','name':'Lớp học thử','academicPeriod':period,'offeringCount':1}] if period=='2026-2027-HK1' else []
                payload={'items':students,'page':1,'totalItems':len(students),'totalPages':1}
        else:raise AssertionError(req.url)
        route.fulfill(status=200,content_type='application/json',body=json.dumps(payload))
    page.route('http://localhost:5298/api/**',api)
    page.goto('http://localhost:5173/curriculum/versions/1/classes?period=2026-2027-HK1')
    batch=page.get_by_role('combobox',name='Đợt học',exact=True); semester=page.get_by_role('combobox',name='Học kỳ',exact=True); open_list=page.get_by_role('button',name='Mở danh sách lớp',exact=True)
    expect(batch).to_have_value('2026-2027'); expect(semester).to_have_value('2026-2027-HK1'); expect(page.get_by_role('link',name='Lớp học thử',exact=True)).to_be_visible()
    expect(page.get_by_role('heading',name='Lớp · 2026-2027-HK1',exact=True)).to_be_visible()
    batch.select_option('2027-2028'); expect(semester).to_have_value(''); expect(open_list).to_be_disabled(); expect(page.get_by_role('link',name='Lớp học thử',exact=True)).to_have_count(0)
    expect(semester).to_be_enabled(); assert semester.locator('option').all_text_contents()==['Chọn học kỳ','Học kỳ 1','Học kỳ 2','Học kỳ hè']
    assert not any(o.startswith('2026') for o in semester.locator('option').evaluate_all('(options)=>options.map(o=>o.value)'))
    semester.select_option('2027-2028-HK2'); expect(open_list).to_be_enabled(); open_list.click(); expect(page).to_have_url('http://localhost:5173/curriculum/versions/1/classes?period=2027-2028-HK2')
    semester.select_option('2027-2028-HKHE'); open_list.click(); expect(page).to_have_url('http://localhost:5173/curriculum/versions/1/classes?period=2027-2028-HKHE')
    semester.select_option('2027-2028-HK2'); open_list.click()
    page.reload(); expect(batch).to_have_value('2027-2028'); expect(semester).to_have_value('2027-2028-HK2')
    batch.select_option('2020'); expect(semester).to_be_enabled(); assert semester.locator('option').count()==2
    semester.select_option('2020-HK1'); open_list.click(); expect(page).to_have_url('http://localhost:5173/curriculum/versions/1/classes?period=2020-HK1')
    batch.select_option('2026-2027'); expect(semester).to_be_enabled(); semester.select_option('2026-2027-HK1'); open_list.click()
    expect(page.get_by_role('link',name='Lớp học thử',exact=True)).to_be_visible()
    folder=Path('tmp/academic-period-ui');folder.mkdir(parents=True,exist_ok=True)
    page.screenshot(path=str(folder/'desktop.png'),full_page=True)
    page.set_viewport_size({'width':390,'height':844});page.screenshot(path=str(folder/'mobile.png'),full_page=True)
    assert page.evaluate('document.documentElement.scrollWidth<=innerWidth')
    assert page.get_by_role('textbox',name='Tìm lớp').bounding_box()['height'] < 70
    box1=batch.bounding_box();box2=semester.bounding_box();assert abs(box1['x']-box2['x']) < 2 and box2['y']>box1['y']
    batch.select_option(''); expect(semester).to_be_disabled(); expect(open_list).to_be_disabled()
    assert not errors,errors
    browser.close()
print('PASS linked dropdowns: preselect/reload, clear semester and stale roster on batch change, scoped options, legacy period IDs, mobile layout and no JS errors')
