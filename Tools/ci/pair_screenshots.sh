#!/usr/bin/env bash
# Clean side-by-side: a label row above each picture, then a gap between them.
# The gap is its own image, so nothing is spliced through the screenshots.
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
out="$root/Screenshots"
src="$root/docs/screenshots/prototype-match"
mkdir -p "$out"
font=/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf
img() {
  if command -v magick >/dev/null 2>&1; then magick "$@"; else command convert "$@"; fi
}
column() {
  local picture="$1" label="$2" dest="$3"
  img \
    \( -size 520x64 xc:'#22140f' -fill '#f6e7cf' -font "$font" -pointsize 34 -gravity center -annotate +0+0 "$label" \) \
    \( "$picture" -resize x900 -background '#22140f' -gravity center -extent 520x900 \) \
    -append "$dest"
}
pair() {
  local js="$1" unity="$2" dest="$3"
  if [[ ! -f "$js" || ! -f "$unity" ]]; then
    echo "skip $dest (missing $js or $unity)"
    return 0
  fi
  local tmp
  tmp=$(mktemp -d)
  column "$js" "Prototype" "$tmp/l.png"
  column "$unity" "Unity" "$tmp/r.png"
  img "$tmp/l.png" \( -size 36x964 xc:'#22140f' \) "$tmp/r.png" +append "$dest"
  rm -rf "$tmp"
  echo "wrote $dest"
}
pair "$src/js-playing.png" "$out/01-playing.png" "$out/compare-playing.png"
pair "$src/js-mid.png" "$out/02-mid.png" "$out/compare-mid.png"
pair "$src/js-win.png" "$out/03-win.png" "$out/compare-win.png"
theme() {
  local dark="$1" light="$2" dest="$3"
  if [[ ! -f "$dark" || ! -f "$light" ]]; then
    echo "skip $dest"
    return 0
  fi
  local tmp
  tmp=$(mktemp -d)
  column "$dark" "Koyu" "$tmp/l.png"
  column "$light" "Açık" "$tmp/r.png"
  img "$tmp/l.png" \( -size 36x964 xc:'#22140f' \) "$tmp/r.png" +append "$dest"
  rm -rf "$tmp"
  echo "wrote $dest"
}
theme "$out/theme-dark-playing.png" "$out/theme-light-playing.png" "$out/theme-compare-playing.png"
theme "$out/theme-dark-win.png" "$out/theme-light-win.png" "$out/theme-compare-win.png"
