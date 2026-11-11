#!/usr/bin/env bash
# Offline checks for the secret gate. No real secrets.
set -euo pipefail
root=$(cd "$(dirname "$0")" && pwd)
tmp=$(mktemp -d)
trap 'rm -rf "$tmp"' EXIT

unset UNITY_LICENSE UNITY_EMAIL UNITY_PASSWORD UNITY_SERIAL \
  APPSTORE_KEY_ID APPSTORE_ISSUER_ID APPSTORE_P8 APPLE_TEAM_ID \
  MATCH_GIT_URL MATCH_PASSWORD MATCH_GIT_TOKEN || true

read_ready() { grep '^ready=' "$1" | head -1 | cut -d= -f2; }

bash "$root/check_secrets.sh" unity "$tmp/empty.txt"
[ "$(read_ready "$tmp/empty.txt")" = "false" ]

UNITY_LICENSE='<License/>' UNITY_EMAIL='a@b.c' UNITY_PASSWORD='pw-value-xyz' \
  bash "$root/check_secrets.sh" unity "$tmp/personal.txt"
[ "$(read_ready "$tmp/personal.txt")" = "true" ]
grep -q 'license_mode=personal' "$tmp/personal.txt"

UNITY_SERIAL='XX-XXXX' UNITY_EMAIL='a@b.c' UNITY_PASSWORD='pw-value-xyz' \
  bash "$root/check_secrets.sh" unity "$tmp/serial.txt"
[ "$(read_ready "$tmp/serial.txt")" = "true" ]
grep -q 'license_mode=serial' "$tmp/serial.txt"

UNITY_LICENSE='<License/>' \
  bash "$root/check_secrets.sh" unity "$tmp/license-only.txt"
[ "$(read_ready "$tmp/license-only.txt")" = "false" ]

# Must not echo the secret value.
if grep -q 'pw-value-xyz' "$tmp/personal.txt"; then
  echo "gate leaked a secret value" >&2
  exit 1
fi

export UNITY_LICENSE='<License/>' UNITY_EMAIL='a@b.c' UNITY_PASSWORD='pw' \
  APPSTORE_KEY_ID='KEY' APPSTORE_ISSUER_ID='ISS' APPSTORE_P8='cDhh' \
  APPLE_TEAM_ID='TEAMID1234' MATCH_PASSWORD='match-pass' MATCH_GIT_TOKEN='ghp_example'
export MATCH_GIT_URL='https://github.com/keremsek99-wq/rugtopia-certificates.git'
bash "$root/check_secrets.sh" testflight "$tmp/tf.txt"
[ "$(read_ready "$tmp/tf.txt")" = "true" ]

export MATCH_GIT_URL='https://github.com/keremsek99-wq/rugtopia.git'
set +e
bash "$root/check_secrets.sh" testflight "$tmp/bad.txt" 2>"$tmp/bad.err"
code=$?
set -e
[ "$code" -eq 1 ]
grep -q 'public game repo' "$tmp/bad.err"

unset MATCH_GIT_TOKEN
export MATCH_GIT_URL='https://github.com/keremsek99-wq/rugtopia-certificates.git'
bash "$root/check_secrets.sh" testflight "$tmp/missing.txt"
[ "$(read_ready "$tmp/missing.txt")" = "false" ]
grep -q 'MATCH_GIT_TOKEN' "$tmp/missing.txt"

echo "secret gate self-check ok"
