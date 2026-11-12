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
mkdir -p "$root/docs/screenshots/theme-compare"
theme "$out/theme-dark-playing.png" "$out/theme-light-playing.png" "$out/theme-compare-playing.png"
theme "$out/theme-dark-win.png" "$out/theme-light-win.png" "$out/theme-compare-win.png"
theme "$out/theme-dark-playing.png" "$out/theme-light-playing.png" "$root/docs/screenshots/theme-compare/compare-playing.png"
theme "$out/theme-dark-win.png" "$out/theme-light-win.png" "$root/docs/screenshots/theme-compare/compare-win.png"
research="$root/docs/screenshots/iter-research"
mkdir -p "$research"
cp "$out/iter-ftue.png" "$research/ftue.png"
cp "$out/iter-playing.png" "$research/playing.png"
cp "$out/iter-hard.png" "$research/hard-badge.png"
cp "$out/iter-fail.png" "$research/fail.png"
echo "wrote $research"
light="$root/docs/screenshots/light-default"
mkdir -p "$light"
cp "$out/light-playing.png" "$light/playing.png"
cp "$out/light-mid.png" "$light/mid.png"
cp "$out/light-win.png" "$light/win.png"
cp "$out/light-fail.png" "$light/fail.png"
cp "$out/light-hard.png" "$light/hard-badge.png"
echo "wrote $light"
dense="$root/docs/screenshots/dense-fix"
mkdir -p "$dense"
cp "$out/dense-level-1.png" "$dense/level-1.png"
cp "$out/dense-level-9.png" "$dense/level-9.png"
cp "$out/dense-level-15.png" "$dense/level-15.png"
cp "$out/dense-level-21.png" "$dense/level-21.png"
echo "wrote $dense"
shell="$root/docs/screenshots/shell"
mkdir -p "$shell"
cp "$out/shell-home-fresh.png" "$shell/home-fresh.png"
cp "$out/shell-home-mid.png" "$shell/home-mid.png"
cp "$out/shell-home-streak.png" "$shell/home-streak.png"
cp "$out/shell-daily.png" "$shell/daily-modal.png"
cp "$out/shell-streak.png" "$shell/streak.png"
cp "$out/shell-path.png" "$shell/path.png"
cp "$out/shell-win.png" "$shell/win.png"
cp "$out/shell-play-9.png" "$shell/play-level-9.png"
echo "wrote $shell"
