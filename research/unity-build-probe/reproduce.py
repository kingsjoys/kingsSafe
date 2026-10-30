#!/usr/bin/env python3
"""Create a NEW isolated fixture; build baseline, add plugin, use SAME build entry twice."""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
from datetime import datetime
from pathlib import Path
from verify import verify

parser=argparse.ArgumentParser()
parser.add_argument('--unity', type=Path, default=Path('/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity'))
parser.add_argument('--work', type=Path, help='New directory only; existing directories are never overwritten')
args=parser.parse_args()
root=Path(__file__).resolve().parent
work=(args.work or root/'artifacts'/('reproduction-'+datetime.now().strftime('%Y%m%d-%H%M%S'))).resolve()
work.mkdir(parents=True, exist_ok=False)
project=work/'Project'
artifacts=work/'artifacts'
artifacts.mkdir()
unity=args.unity.resolve()
cecil=unity.parent.parent/'Resources/Scripting/MonoBleedingEdge/lib/mono/gac/Mono.Cecil/0.11.1.0__0738eb9f132ed756/Mono.Cecil.dll'
if not unity.is_file() or not cecil.is_file():
    raise SystemExit('Requires installed Unity 6000.3.6f1 with Mac Mono support and its Cecil 0.11.1; no download performed')

def run(command, env=None):
    subprocess.run([str(x) for x in command], env=env, check=True, timeout=900)

run([unity,'-batchmode','-nographics','-quit','-createProject',project,'-logFile',artifacts/'create.log'])
shutil.copytree(root/'Fixture/Assets', project/'Assets', dirs_exist_ok=True)

def build(log, output):
    environment=dict(os.environ,KS_PROBE_OUTPUT=str(output))
    run([unity,'-batchmode','-nographics','-quit','-projectPath',project,'-executeMethod','FixtureBuild.Run','-logFile',log],environment)

def play(app, log, attacks=False):
    binaries=list((app/'Contents/MacOS').iterdir())
    executable=next(p for p in binaries if p.is_file() and os.access(p,os.X_OK))
    command=[executable,'-batchmode','-nographics','-logFile',log]
    if attacks: command.append('--probe-attacks')
    run(command)

build(artifacts/'baseline-build.log',artifacts/'baseline.app')
play(artifacts/'baseline.app',artifacts/'baseline-player.log')
files=list((project/'Assets/Game').glob('*.cs'))+[project/'Assets/Editor/FixtureBuild.cs',project/'Assets/StreamingAssets/fixture.bundle']
hashes={str(p.relative_to(project)):hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
(artifacts/'source-baseline.json').write_text(json.dumps(hashes,indent=2))
plugin=project/'Assets/KingsSafeProbe'
shutil.copytree(root/'Plugin',plugin)
shutil.copyfile(cecil,plugin/'Editor/Mono.Cecil.dll')
shutil.copyfile(root/'Cecil.editor.meta',plugin/'Editor/Mono.Cecil.dll.meta')
build(artifacts/'protected-build.log',artifacts/'protected.app')
play(artifacts/'protected.app',artifacts/'protected-player.log',True)
build(artifacts/'protected-repeat-build.log',artifacts/'protected.app')
play(artifacts/'protected.app',artifacts/'protected-repeat-player.log',True)
verify(project,artifacts)
print('Evidence:', artifacts/'evidence.json')
