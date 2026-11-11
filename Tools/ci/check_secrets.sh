#!/usr/bin/env bash
# Decide whether a CI job has the secrets it needs.
# Usage: check_secrets.sh <unity|testflight> <github_output_file>
# Always exits 0 when the right action is to skip. Exits 1 only when secrets are
# present but unsafe (match repo points at this public game repo).
# Never prints secret values.
set -euo pipefail

mode="${1:?mode unity or testflight}"
out="${2:?output file}"

nonempty() { [ -n "${1:-}" ]; }

personal_ok=false
serial_ok=false
if nonempty "${UNITY_LICENSE:-}" && nonempty "${UNITY_EMAIL:-}" && nonempty "${UNITY_PASSWORD:-}"; then
  personal_ok=true
fi
if nonempty "${UNITY_SERIAL:-}" && nonempty "${UNITY_EMAIL:-}" && nonempty "${UNITY_PASSWORD:-}"; then
  serial_ok=true
fi

unity_ready=false
unity_mode=""
if [ "$personal_ok" = true ]; then
  unity_ready=true
  unity_mode=personal
elif [ "$serial_ok" = true ]; then
  unity_ready=true
  unity_mode=serial
fi

unity_skip_message="Unity build skipped. Personal license needs UNITY_LICENSE + UNITY_EMAIL + UNITY_PASSWORD (paste the .ulf file as-is, do not base64 it). A Pro license needs UNITY_EMAIL + UNITY_PASSWORD + UNITY_SERIAL instead. See docs/CI_SETUP.md."

apple_names=(APPSTORE_KEY_ID APPSTORE_ISSUER_ID APPSTORE_P8 APPLE_TEAM_ID MATCH_GIT_URL MATCH_PASSWORD MATCH_GIT_TOKEN)
apple_missing=()
for name in "${apple_names[@]}"; do
  val="${!name-}"
  if ! nonempty "$val"; then
    apple_missing+=("$name")
  fi
done

ready=false
message="$unity_skip_message"
license_mode=""

case "$mode" in
  unity)
    if [ "$unity_ready" = true ]; then
      ready=true
      license_mode="$unity_mode"
      message="Unity license secrets are set ($unity_mode)."
    fi
    ;;
  testflight)
    if [ "$unity_ready" != true ]; then
      message="TestFlight skipped. $unity_skip_message"
    elif [ "${#apple_missing[@]}" -gt 0 ]; then
      missing_csv=$(printf '%s, ' "${apple_missing[@]}")
      missing_csv=${missing_csv%, }
      message="TestFlight skipped. Missing secrets: ${missing_csv}. See docs/CI_SETUP.md."
    else
      url_lc=$(printf '%s' "$MATCH_GIT_URL" | tr '[:upper:]' '[:lower:]')
      norm=$(printf '%s' "$url_lc" | sed -E 's#\.git$##; s#/*$##')
      case "$norm" in
        https://github.com/keremsek99-wq/rugtopia|git@github.com:keremsek99-wq/rugtopia|ssh://git@github.com/keremsek99-wq/rugtopia)
          echo "MATCH_GIT_URL points at the public game repo. Use a separate PRIVATE certificates repo. Refusing to continue." >&2
          exit 1
          ;;
      esac
      ready=true
      license_mode="$unity_mode"
      message="Unity and App Store signing secrets are set. TestFlight can run."
    fi
    ;;
  *)
    echo "unknown mode: $mode" >&2
    exit 2
    ;;
esac

{
  printf 'ready=%s\n' "$ready"
  printf 'license_mode=%s\n' "$license_mode"
  printf 'message=%s\n' "$message"
} >> "$out"
