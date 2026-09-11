"""Source integrity checks; not a replacement for UHT, UBT, or gameplay tests."""
from pathlib import Path
import ast
import json
import re

root = Path(__file__).resolve().parents[1]
source = root / 'Source' / 'ProjectDuskline'
errors = []
checks = 0

def check(condition, message):
    global checks
    checks += 1
    if not condition:
        errors.append(message)

project = json.loads((root/'ProjectDuskline.uproject').read_text(encoding='utf-8'))
check(project['Modules'][0]['Name'] == 'ProjectDuskline', 'module mismatch')
check(any(p['Name'] == 'EnhancedInput' and p['Enabled'] for p in project['Plugins']), 'Enhanced Input disabled')
for path in root.rglob('*.py'):
    ast.parse(path.read_text(encoding='utf-8'), filename=str(path))
    checks += 1
for path in source.rglob('*'):
    if path.suffix not in ('.h', '.cpp'):
        continue
    text = path.read_text(encoding='utf-8')
    includes = re.findall(r'^#include "([^"]+)"', text, re.M)
    if 'GENERATED_BODY()' in text:
        check(includes[-1] == path.stem+'.generated.h', f'{path.name}: generated header must be last')
    for inc in includes:
        if inc.startswith(('Characters/', 'Game/Duskline', 'Game/RunRules', 'AI/Duskline', 'Items/Item', 'World/', 'UI/Duskline', 'Interaction/')) or inc in {'Components/'+x+'Component.h' for x in ('Health','Stamina','Combat','Inventory','NoiseEmitter')}:
            check((source/inc).is_file(), f'{path.name}: missing local include {inc}')
    stripped = re.sub(r'//[^\n]*|/\*.*?\*/|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', '', text, flags=re.S)
    stack = []
    pairs = {'}':'{',')':'(',']':'['}
    balanced = True
    for c in stripped:
        if c in '{([':
            stack.append(c)
        elif c in pairs:
            if not stack or stack.pop() != pairs[c]:
                balanced = False
                break
    check(balanced and not stack, f'{path.name}: unbalanced delimiters')
    check('GetAllActorsOfClass' not in text, f'{path.name}: runtime global actor search')
    check('PrimaryActorTick.bCanEverTick=true' not in text, f'{path.name}: actor tick enabled')
for name in ('README.md','Docs/Architecture.md','Docs/EditorSetup.md','Docs/TestPlan.md','Docs/ImplementationReport.md'):
    check((root/name).is_file(), f'missing document {name}')
report = {'checks':checks,'errors':errors,'scope':'source integrity only; Unreal build and runtime not verified'}
print(json.dumps(report, ensure_ascii=False, indent=2))
raise SystemExit(bool(errors))
