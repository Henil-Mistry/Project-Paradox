#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-Unity}"
if ! command -v "$unity_editor" >/dev/null 2>&1; then
    echo 'Set UNITY_EDITOR to the installed Unity 6000.6.7f1 executable.' >&2
    exit 1
fi
graphics=(-nographics)
if [[ "${PARADOX_GRAPHICS:-0}" == 1 ]]; then graphics=(-force-glcore); fi
"$unity_editor" -batchmode "${graphics[@]}" -projectPath "$project_root" \
    -runTests -testPlatform PlayMode \
    -testResults "$project_root/Docs/TestResults.xml" \
    -logFile "$project_root/Docs/last-test-run.log"
