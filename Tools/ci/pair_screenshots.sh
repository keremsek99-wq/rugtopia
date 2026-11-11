#!/usr/bin/env bash
# Place the HTML prototype and the Unity capture side by side.
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
out="$root/Screenshots"
src="$root/docs/screenshots/prototype-match"
mkdir -p "$out"
img() {
  if command -v magick >/dev/null 2>&1; then magick "$@"; else command convert "$@"; fi
}
pair() {
  local js="$1" unity="$2" dest="$3"
  if [[ ! -f "$js" || ! -f "$unity" ]]; then
    echo "skip $dest (missing $js or $unity)"
    return 0
  fi
  img "$js" "$unity" -resize x1000 -gravity center -background '#22140f' -splice 12x0 +append "$dest"
  echo "wrote $dest"
}
pair "$src/js-playing.png" "$out/01-playing.png" "$out/compare-playing.png"
pair "$src/js-mid.png" "$out/02-mid.png" "$out/compare-mid.png"
pair "$src/js-win.png" "$out/03-win.png" "$out/compare-win.png"
