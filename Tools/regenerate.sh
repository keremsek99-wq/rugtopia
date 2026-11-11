#!/usr/bin/env bash
# Re-export levels + parity fixtures + UI strings from the HTML5 prototype, then run the C# core tests.
# Usage: Tools/regenerate.sh [/path/to/ilmek-prototype/game.js]
set -euo pipefail
cd "$(dirname "$0")/.."
node Tools/export_levels.js "${1:-prototype/game.js}"
dotnet test CoreTests
