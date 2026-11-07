"""UI regression on Vite :5173 with synthetic API responses; no real accounts/database are changed."""
import json
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

scheme={'versionId':1,'revision':0,'locked':False,'components':[]}
puts=[]; errors=[]
with sync_playwright() as p:
    browser=p.chromium.launch(channel='msedge',headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000})
    page.on('pageerror',lambda e:errors.append(str(e)))
    def api(route):
        request=route.request; path=request.url.split('/api/',1)[1]
        status=200; payload={}
        if request.method=='OPTIONS':
            route.fulfill(status=204,headers={'Access-Control-Allow-Origin':'http://localhost:5173','Access-Control-Allow-Headers':'content-type,authorization','Access-Control-Allow-Methods':'GET,POST,PUT,OPTIONS','Access-Control-Allow-Credentials':'true'});return
        if path=='auth/refresh':
            payload={'accessToken':'synthetic-local-test-token','expiresAt':'2099-01-01T00:00:00Z','user':{'id':1,'email':'weight-ui@example.test','fullName':'Weight UI Admin','role':'admin','status':'active','mustChangePassword':False,'lastLoginAt':None,'createdAt':'2026-11-07T00:00:00Z'}}
        elif path.endswith('/assessment-scheme'):
            if request.method=='PUT':
                value=request.post_data_json; puts.append(value)
                if len({c['code'] for c in value['components']})!=len(value['components']):
                    status=400; payload={'status':400,'detail':'Dữ liệu gửi lên không hợp lệ.','errors':{'components':['Mã đầu điểm bị trùng. Hãy dùng mã khác nhau.']}}
                else:
                    scheme.update(revision=scheme['revision']+1,components=value['components']);payload=scheme
            else:payload=scheme
        elif path=='curriculum/program-versions/1':
            payload={'id':1,'programId':1,'versionCode':'V1','status':'finalized','archivedAt':None}
        else:raise AssertionError('Unexpected API: '+request.method+' '+path)
        route.fulfill(status=status,content_type='application/json',body=json.dumps(payload),headers={'Access-Control-Allow-Origin':'http://localhost:5173','Access-Control-Allow-Credentials':'true'})
    page.route('http://localhost:5298/api/**',api)
    page.goto('http://localhost:5173/curriculum/versions/1/assessment-scheme')
    add=page.get_by_role('button',name='Thêm đầu điểm',exact=True)
    add.click(); weight=page.get_by_label('Trọng số 1',exact=True)
    expect(weight).to_have_value('')
    weight.fill('25'); weight.clear(); expect(weight).to_have_value('')
    page.get_by_label('Mã đầu điểm 1',exact=True).fill('QT'); page.get_by_label('Tên đầu điểm 1',exact=True).fill('QT1'); weight.fill('25')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('alert')).to_contain_text('25.00%')
    expect(page.get_by_role('alert')).to_contain_text('100%')
    assert not puts,'Invalid total must be caught before HTTP PUT'
    folder=Path('tmp/weight-ui'); folder.mkdir(parents=True,exist_ok=True)
    page.screenshot(path=str(folder/'total-25-validation.png'),full_page=True)
    add.click(); expect(page.get_by_label('Trọng số 2',exact=True)).to_have_value('')
    page.get_by_label('Mã đầu điểm 2',exact=True).fill('QT'); page.get_by_label('Tên đầu điểm 2',exact=True).fill('Thi kết thúc'); page.get_by_label('Trọng số 2',exact=True).fill('75')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('alert')).to_contain_text('Mã đầu điểm bị trùng')
    page.get_by_label('Mã đầu điểm 2',exact=True).fill('THI')
    page.get_by_role('button',name='Lưu cơ cấu điểm',exact=True).click()
    expect(page.get_by_role('status')).to_contain_text('Đã lưu cơ cấu điểm')
    assert [c['weight'] for c in puts[-1]['components']]==[25,75]
    assert all(isinstance(c['weight'],(int,float)) for c in puts[-1]['components'])
    page.reload();expect(page.get_by_label('Trọng số 1',exact=True)).to_have_value('25');expect(page.get_by_label('Trọng số 2',exact=True)).to_have_value('75')
    page.get_by_label('Điểm tối đa 1',exact=True).clear(); expect(page.get_by_label('Điểm tối đa 1',exact=True)).to_have_value('')
    assert not errors,errors
    browser.close()
print('PASS: blank weight on add/clear, total 25% blocked before API with clear explanation, backend field errors visible, valid 25+75 saved as numbers, persisted weights reload correctly')
