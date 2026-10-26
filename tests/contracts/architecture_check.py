"""Static boundary and file-layout checks; run from repository root. No dependencies."""
from pathlib import Path
import re

roots=[Path('modules/common/curriculum/backend'),Path('modules/sv5/reporting/backend')]
count=0
for root in roots:
    for area in ('domain','application'):
        for path in (root/area).rglob('*.cs'):
            source=path.read_text(encoding='utf-8-sig')
            assert not re.search(r'^using .*?(EntityFrameworkCore|Npgsql|Dapper|AspNetCore|\.Infrastructure|\.Presentation)',source,re.M),path
    for folder in ('domain/Entities','application/DTOs','application/Interfaces','infrastructure/Repositories','infrastructure/Persistence/Configurations'):
        for path in (root/folder).rglob('*.cs'):
            types=re.findall(r'^(?:public|internal)\s+(?:(?:sealed|static|partial|abstract)\s+)*(?:class|record|interface)\s+(\w+)',path.read_text(encoding='utf-8-sig'),re.M)
            assert types == [path.stem],(path,types)
            count+=1
    for path in root.rglob('*.csproj'):
        assert 'Include="Dapper"' not in path.read_text(encoding='utf-8-sig'),path
    for path in (root/'presentation').glob('*Endpoints.cs'):
        assert '.Infrastructure;' not in path.read_text(encoding='utf-8-sig'),path
print(f'PASS architecture: {count} separate entity/DTO/interface/repository/configuration files; inner layers have no persistence/web imports')

assert not list(Path('infra/postgres').rglob('*.sql')), 'Legacy bootstrap SQL must not return'
compose = Path('compose.yaml').read_text(encoding='utf-8')
assert 'docker-entrypoint-initdb.d' not in compose
assert 'database-init:' in compose and 'service_completed_successfully' in compose
print('PASS database lifecycle: EF initialization service; no legacy SQL/bootstrap mounts')
