#!/usr/bin/env bash
set -euo pipefail

base_ref="${BASE_REF:-}"
head_ref="${HEAD_REF:-}"
pr_body="${PR_BODY:-}"

if [[ "$base_ref" != "main" ]]; then
  echo "PR deve apontar para main; destino atual: ${base_ref:-indefinido}." >&2
  exit 1
fi

git fetch --no-tags origin '+refs/heads/*:refs/remotes/origin/*'

if ! git merge-base --is-ancestor origin/main HEAD; then
  echo "Branch desatualizada: execute git rebase origin/main." >&2
  exit 1
fi

while IFS= read -r ref; do
  case "$ref" in
    refs/remotes/origin/HEAD|refs/remotes/origin/main|"refs/remotes/origin/$head_ref")
      continue
      ;;
  esac

  if git merge-base --is-ancestor "$ref" origin/main; then
    continue
  fi

  if git merge-base --is-ancestor "$ref" HEAD; then
    echo "Branch empilhada sobre ${ref#refs/remotes/origin/}; recrie-a a partir de main." >&2
    exit 1
  fi
done < <(git for-each-ref --format='%(refname)' refs/remotes/origin)

if [[ "$pr_body" != *"- [x] O PR contém uma única tarefa coesa."* &&
      "$pr_body" != *"- [X] O PR contém uma única tarefa coesa."* ]]; then
  echo "Confirme no template: O PR contém uma única tarefa coesa." >&2
  exit 1
fi

echo "Política de PR aprovada."
