#!/usr/bin/env python3
"""Verify captured evidence; passing means mechanism checks, never product security."""
import argparse
import hashlib
import json
import re
import subprocess
from pathlib import Path


def verify(project, artifacts):
    baseline = (artifacts / 'baseline-player.log').read_text(errors='replace')
    protected = (artifacts / 'protected-player.log').read_text(errors='replace')
    build = (artifacts / 'protected-build.log').read_text(errors='replace')
    repeat = (artifacts / 'protected-repeat-build.log').read_text(errors='replace')
    repeat_player = (artifacts / 'protected-repeat-player.log').read_text(errors='replace')
    required = {
        'baseline': ['PROBE_COMPLETED', 'rawEqualsValue=True', 'rejected=False', 'originalPrivateNamePresent=True'],
        'protected': ['PROBE_COMPLETED', 'rawEqualsValue=False', 'rejected=True', 'originalPrivateNamePresent=False',
                      'coordinatedRewriteAccepted=True', 'fullStateReplayAccepted=True', 'corruptedCiphertextRejected=True',
                      'PROBE_BOUNDARIES_COMPLETED'],
    }
    for name, log in [('baseline', baseline), ('protected', protected)]:
        for marker in required[name]:
            if marker not in log:
                raise AssertionError(f'{name}: missing {marker}')
    semantic = lambda log: next(line for line in log.splitlines() if line.startswith('PROBE_SEMANTICS_PASS '))
    if not (semantic(baseline) == semantic(protected) == semantic(repeat_player)):
        raise AssertionError('semantic result differs')
    for name, log in [('protected build', build), ('unchanged repeat', repeat)]:
        for marker in ['fields=1 accesses=7 resourceCalls=1', 'PROBE_RESOURCE_PASS bundles=1', 'PROBE_ADHOC_SIGN_PASS', 'PROBE_BUILD_PASS']:
            if marker not in log:
                raise AssertionError(f'{name}: missing {marker}')
    for marker in required['protected']:
        if marker not in repeat_player:
            raise AssertionError(f'repeat player: missing {marker}')
    hashes = json.loads((artifacts/'source-baseline.json').read_text())
    for name, before in hashes.items():
        if hashlib.sha256((project/name).read_bytes()).hexdigest() != before:
            raise AssertionError(f'original changed: {name}')
    baseline_bundle = artifacts/'baseline.app/Contents/Resources/Data/StreamingAssets/fixture.bundle'
    protected_bundle = artifacts/'protected.app/Contents/Resources/Data/StreamingAssets/fixture.bundle'
    if not baseline_bundle.read_bytes().startswith(b'UnityFS') or not protected_bundle.read_bytes().startswith(b'KSP1'):
        raise AssertionError('unexpected bundle headers')
    subprocess.run(['/usr/bin/codesign', '--verify', '--deep', '--strict', str(artifacts/'protected.app')], check=True)
    result = {
        'scope': 'Unity 6000.3.6f1 / macOS arm64 host and run / universal Mono Player / own tiny fixture / headless',
        'classification': 'mechanism probe; not a security acceptance report',
        'same_business_and_build_script_sha256': hashes,
        'same_fixture_bundle_sha256': hashes['Assets/StreamingAssets/fixture.bundle'],
        'same_runtime_result': semantic(protected),
        'automatic_transform': 'one field / seven accesses / one resource call / private name transformation',
        'single_storage_write_rejected_on_next_business_read': True,
        'corrupted_ciphertext_rejected': True,
        'coordinated_value_and_tag_rewrite_bypasses': True,
        'complete_valid_state_replay_bypasses': True,
        'unchanged_repeat_build_and_player': True,
        'local_ad_hoc_signature_valid': True,
        'product_security_acceptance': 'NOT RUN',
        'windows_il2cpp_android_ios': 'NOT RUN',
        'external_memory_read_detection_or_self_termination': 'NOT RUN',
        'performance_and_full_visual_quality': 'NOT RUN',
    }
    (artifacts/'evidence.json').write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n')
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return result


if __name__ == '__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--project', type=Path, default=Path(__file__).parent/'Project')
    parser.add_argument('--artifacts', type=Path, default=Path(__file__).parent/'artifacts')
    args=parser.parse_args()
    verify(args.project.resolve(), args.artifacts.resolve())
