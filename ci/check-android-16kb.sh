#!/usr/bin/env bash
set -euo pipefail

AAB_PATH="${1:-Builds/Android/Fsp-release.aab}"

if [[ ! -f "$AAB_PATH" ]]; then
  echo "::error::AAB not found: $AAB_PATH"
  exit 1
fi

workdir="$(mktemp -d)"
trap 'rm -rf "$workdir"' EXIT
unzip -q "$AAB_PATH" -d "$workdir/aab"

mapfile -t libs < <(find "$workdir/aab" -type f -name '*.so' | sort)
if [[ ${#libs[@]} -eq 0 ]]; then
  echo "::error::No native .so libraries found in $AAB_PATH"
  exit 1
fi

readelf_bin="$(command -v llvm-readelf || command -v readelf || true)"
if [[ -z "$readelf_bin" ]]; then
  echo "::error::Neither llvm-readelf nor readelf is available on the runner"
  exit 1
fi

failed=0
for lib in "${libs[@]}"; do
  rel="${lib#$workdir/aab/}"
  aligns=$("$readelf_bin" -lW "$lib" | awk '$1=="LOAD" {print $NF}')
  if [[ -z "$aligns" ]]; then
    echo "::error::$rel has no LOAD segments"
    failed=1
    continue
  fi

  while read -r align; do
    [[ -z "$align" ]] && continue
    value=$((align))
    if (( value < 0x4000 )); then
      echo "::error::$rel has LOAD alignment $align (< 0x4000 / 16 KB)"
      failed=1
    fi
  done <<< "$aligns"

done

if (( failed != 0 )); then
  echo "Android 16 KB native page-size validation FAILED."
  exit 1
fi

echo "Android 16 KB native page-size validation PASSED for ${#libs[@]} native libraries."
