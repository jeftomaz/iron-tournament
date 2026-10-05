#!/usr/bin/env bash
set -euo pipefail

repository_root="$(git rev-parse --show-toplevel)"
verification_root="$(mktemp -d "${TMPDIR:-/tmp}/iron-tournament-verify.XXXXXX")"
project_root="$verification_root/project"
results_directory="$verification_root/results"
trap 'rm -rf -- "$verification_root"' EXIT

resolve_unity() {
  local candidate
  for candidate in \
    "${UNITY_EXECUTABLE:-}" \
    "/Applications/Unity/Hub/Editor/6000.5.7f1/Unity.app/Contents/MacOS/Unity" \
    "$HOME/.unity/bin/Unity"; do
    if [[ -n "$candidate" && -x "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return
    fi
  done

  if command -v Unity >/dev/null 2>&1; then
    command -v Unity
    return
  fi

  echo "Unity 6000.5.7f1 não encontrado. Defina UNITY_EXECUTABLE." >&2
  exit 1
}

unity="$(resolve_unity)"
rsync -a \
  --exclude .git \
  --exclude Library \
  --exclude Logs \
  --exclude Temp \
  "$repository_root/" "$project_root/"
mkdir -p "$results_directory"

"$unity" -batchmode -nographics -quit \
  -projectPath "$project_root" \
  -runTests -testPlatform EditMode \
  -testResults "$results_directory/editmode.xml" \
  -logFile "$results_directory/editmode.log"

"$unity" -batchmode -nographics -quit \
  -projectPath "$project_root" \
  -runTests -testPlatform PlayMode \
  -testResults "$results_directory/playmode.xml" \
  -logFile "$results_directory/playmode.log"

"$unity" -batchmode -nographics -quit \
  -projectPath "$project_root" \
  -executeMethod IronTournament.Editor.WebGlVerificationBuild.Build \
  -logFile "$results_directory/webgl.log"
