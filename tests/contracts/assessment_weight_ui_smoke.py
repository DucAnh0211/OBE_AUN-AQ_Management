"""UI regression on Vite :5173 with synthetic API responses; no real accounts/database are changed.
Course syllabus assessment (đề cương đánh giá): blank weights, 100% total blocked before HTTP, backend errors, CLO links saved as numbers.
SMOKE_CHANNEL selects the browser channel (default msedge; empty = bundled Chromium)."""
import json, os
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

origin='http://localhost:5173'
scheme={'courseId':1,'revision':0,'locked':False,'components':[],'cloCodes':['CLO1','CLO2']}
policy={'universityId':1,'courseScoreDecimals':1,'minComponents':2,'minFinalWeight':None,'retakePolicy':'highest','gpaDecimals':2,
  'bands':[{'letter':'A','minScore':8.5,'point4':4,'isPassing':True},{'letter':'F','minScore':0,'point4':0,'isPassing':False}]}
course={'id':1,'programVersionId':1,'courseId':1,'institutionalCode':'000001','name':'Đại số tuyến tính','credits':3,'semester':'1','displayOrder':1,'status':'finalized','archivedAt':None,'departmentId':None,'countsTowardGpa':True}
puts=[]; errors=[]
with sync_playwright() as p:
    browser=p.chromium.launch(channel=os.environ.get('SMOKE_CHANNEL','msedge') or None,headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000})
    page.on('pageerror',lambda e:errors.append(str(e)))
    def api(route):
        request=route.request; path=request.url.split('/api/',1)[1].split('?')[0]
        status=200; payload={}
        if request.method=='OPTIONS':
            route.fulfill(status=204,headers={'Access-Control-Allow-Origin':origin,'Access-Control-Allow-Headers':'content-type,authorization','Access-Control-Allow-Methods':'GET,POST,PUT,OPTIONS','Access-Control-Allow-Credentials':'true'});return
        if path=='auth/refresh':
            payload={'accessToken':'synthetic-local-test-token','expiresAt':'2099-01-01T00:00:00Z','user':{'id':1,'email':'weight-ui@example.test','fullName':'Weight UI Admin','role':'admin','status':'active','mustChangePassword':False,'lastLoginAt':None,'createdAt':'2026-11-07T00:00:00Z'}}
        elif path=='curriculum/program-versions/1/courses/1/assessment-scheme':
            if request.method=='PUT':
                value=request.post_data_json; puts.append(value)
                if len({c['code'] for c in value['components']})!=len(value['components']):
                    status=400; payload={'status':400,'detail':'Dữ liệu gửi lên không hợp lệ.','errors':{'components':['Mã đầu điểm bị trùng. Hãy dùng mã khác nhau.']}}
                else:
                    scheme.update(revision=scheme['revision']+1,components=value['components']);payload=scheme
            else:payload=scheme
        elif path=='curriculum/program-versions/1/courses/1': payload=course
        elif path=='curriculum/program-versions/1/grading-policy': payload=policy
        elif path=='curriculum/program-versions/1': payload={'id':1,'programId':1,'versionCode':'V1','status':'finalized','archivedAt':None}
        elif path=='curriculum/programs/1': payload={'id':1,'universityId':1,'code':'CS','name':'CTĐT kiểm thử'}
        elif path=='curriculum/universities/1/org-units': payload=[{'id':5,'universityId':1,'parentId':4,'kind':'department','code':'BMT','name':'Bộ môn Toán','archived':False}]
        else:raise AssertionError('Unexpected API: '+request.method+' '+path)
        route.fulfill(status=status,content_type='application/json',body=json.dumps(payload),headers={'Access-Control-Allow-Origin':origin,'Access-Control-Allow-Credentials':'true'})
    page.route('http://localhost:5298/api/**',api)
    page.goto(origin+'/curriculum/versions/1/courses/1/assessment')
    expect(page.get_by_role('heading',name='Cơ cấu điểm trong đề cương',exact=True)).to_be_visible()
    expect(page.get_by_role('combobox',name='Bộ môn quản lý',exact=True)).to_contain_text('Bộ môn Toán')
    add=page.get_by_role('button',name='Thêm đầu điểm',exact=True)
    add.click(); weight=page.get_by_label('Trọng số 1',exact=True)
    expect(weight).to_have_value('')
    weight.fill('25'); weight.clear(); expect(weight).to_have_value('')
    page.get_by_label('Mã đầu điểm 1',exact=True).fill('QT'); page.get_by_label('Tên đầu điểm 1',exact=True).fill('QT1'); weight.fill('25')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('alert')).to_contain_text('25.00%')
    expect(page.get_by_role('alert')).to_contain_text('100%')
    expect(page.get_by_text('Quy chế yêu cầu tối thiểu 2 đầu điểm.')).to_be_visible()
    assert not puts,'Invalid total must be caught before HTTP PUT'
    folder=Path('tmp/weight-ui'); folder.mkdir(parents=True,exist_ok=True)
    page.screenshot(path=str(folder/'total-25-validation.png'),full_page=True)
    add.click(); expect(page.get_by_label('Trọng số 2',exact=True)).to_have_value('')
    page.get_by_label('Mã đầu điểm 2',exact=True).fill('QT'); page.get_by_label('Tên đầu điểm 2',exact=True).fill('Thi kết thúc'); page.get_by_label('Trọng số 2',exact=True).fill('75')
    page.get_by_label('Loại đầu điểm 2',exact=True).select_option('final')
    page.get_by_label('QT đánh giá CLO1 (%)',exact=True).first.fill('100')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('alert')).to_contain_text('Mã đầu điểm bị trùng')
    page.get_by_label('Mã đầu điểm 2',exact=True).fill('THI')
    page.get_by_label('THI đánh giá CLO2 (%)',exact=True).fill('60')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('status')).to_contain_text('Đã lưu cơ cấu điểm')
    last=puts[-1]['components']
    assert [c['weight'] for c in last]==[25,75] and all(isinstance(c['weight'],(int,float)) for c in last)
    assert [c['kind'] for c in last]==['process','final'] and last[0]['clos']=={'CLO1':100} and last[1]['clos']=={'CLO2':60}
    page.reload();expect(page.get_by_label('Trọng số 1',exact=True)).to_have_value('25');expect(page.get_by_label('Trọng số 2',exact=True)).to_have_value('75')
    expect(page.get_by_label('THI đánh giá CLO2 (%)',exact=True)).to_have_value('60')
    page.get_by_label('Điểm tối đa 1',exact=True).clear(); expect(page.get_by_label('Điểm tối đa 1',exact=True)).to_have_value('')
    assert not errors,errors
    browser.close()
print('PASS: blank weight on add/clear, total 25% blocked before API with policy hints, backend field errors visible, valid 25+75 saved as numbers with kinds and CLO links, persisted values reload correctly')
