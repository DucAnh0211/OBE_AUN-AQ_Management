"""Subject/offerings navigation on local preview :5173, with mock API :5298; never mutates real data."""
import json
from pathlib import Path
from urllib.parse import urlparse,parse_qs
from playwright.sync_api import sync_playwright,expect

subject={'id':1,'programVersionId':1,'institutionalCode':'000001','name':'Đại số tuyến tính kỹ thuật','credits':3,'semester':'Kỳ 1','displayOrder':1,'status':'finalized','archivedAt':None}
def offering(id,code,cls): return {'id':id,'classId':id,'versionId':1,'programCourseId':1,'academicPeriod':'2026-2027-HK1','code':code,'classCode':cls,'className':'Lớp '+cls,'subjectCode':'000001','subjectName':subject['name'],'credits':3,'lecturerId':None,'lecturerName':None,'readOnly':False,'studentCount':0}
offerings=[offering(11,'100001','A1'),offering(12,'100002','B1')]
def paged(rows): return {'items':rows,'page':1,'pageSize':50,'totalItems':len(rows),'totalPages':1 if rows else 0}
requests=[]
with sync_playwright() as p:
    browser=p.chromium.launch(channel='msedge',headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000}); errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    def api(route):
        req=route.request; url=urlparse(req.url); path=url.path; requests.append(path)
        assert req.method in ('GET','POST') and (req.method=='GET' or path=='/api/auth/refresh'),req.url
        if path=='/api/auth/refresh': data={'accessToken':'synthetic-token','expiresAt':'2099-01-01T00:00:00Z','user':{'id':1,'email':'subject@example.test','fullName':'Subject UI','role':'admin','status':'active','mustChangePassword':False,'createdAt':'2026-11-07T00:00:00Z'}}
        elif path=='/api/curriculum/program-versions/1': data={'id':1,'programId':1,'versionCode':'V1','status':'finalized','courseCount':1,'archivedAt':None}
        elif path=='/api/curriculum/programs/1': data={'id':1,'universityId':1,'universityName':'Trường kiểm thử','code':'CS','name':'CTĐT kiểm thử'}
        elif path.endswith('/courses/1/offerings'):
            search=parse_qs(url.query).get('search',[''])[0]; data=paged([o for o in offerings if not search or search in o['code'] or search in o['classCode']])
        elif path.endswith('/courses/1'): data=subject
        elif path.endswith('/courses'): data=paged([subject])
        elif path.endswith('/academic-periods'): data=[{'name':'2026-2027','semesters':[{'name':'Học kỳ 1','period':'2026-2027-HK1'}]}]
        else: raise AssertionError(req.url)
        route.fulfill(status=200,content_type='application/json',body=json.dumps(data))
    page.route('http://localhost:5298/api/**',api)
    page.goto('http://localhost:5173/curriculum/versions/1')
    expect(page.get_by_role('heading',name='Danh sách bộ môn',exact=True)).to_be_visible()
    expect(page.get_by_role('link',name='Lớp theo học kỳ',exact=True)).to_have_count(1)
    row=page.locator('tbody tr').first
    assert row.locator('.table-actions a').all_text_contents()==['Danh sách học phần','CLO/PLO']
    assert row.get_by_role('link',name=subject['name']).get_attribute('href').endswith('/courses/1/offerings')
    row.get_by_role('link',name=subject['name']).click()
    expect(page.get_by_role('heading',name='Danh sách học phần',exact=True)).to_be_visible()
    expect(page.locator('tbody tr')).to_have_count(2)
    assert page.locator('a[href*="/classes"]').count()==0
    assert not any('/classes' in path for path in requests)
    page.get_by_role('searchbox',name='Tìm học phần của bộ môn').fill('100002')
    expect(page.locator('tbody tr')).to_have_count(1)
    expect(page.locator('tbody tr').first).to_contain_text('B1')
    folder=Path('tmp/subject-navigation');folder.mkdir(parents=True,exist_ok=True)
    page.screenshot(path=str(folder/'subject-offerings-desktop.png'),full_page=True)
    page.set_viewport_size({'width':390,'height':844})
    assert page.evaluate('document.documentElement.scrollWidth <= innerWidth')
    page.screenshot(path=str(folder/'subject-offerings-mobile.png'),full_page=True)
    page.set_viewport_size({'width':1440,'height':1000})
    page.get_by_role('link',name='Danh sách bộ môn',exact=True).click()
    page.get_by_role('link',name='Lớp theo học kỳ',exact=True).click()
    expect(page.get_by_role('heading',name='Lớp theo học kỳ',exact=True)).to_be_visible()
    assert not errors,errors
    browser.close()
print('PASS subject navigation: catalog labels, exactly two row actions, subject click lists only its offerings, search, no class navigation within subject, single header class entry, responsive and no JS errors')
