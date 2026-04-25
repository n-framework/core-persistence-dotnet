#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../../../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"
cd "$REPO_ROOT"

acore_log_section "🛠️ Building C# solution..."

for slnx_file in "${REPO_ROOT}"/*.slnx; do
	[ -f "$slnx_file" ] || continue
	acore_log_info "Building: $slnx_file"
	dotnet build "$slnx_file" --configuration Debug
done

acore_log_success "✨ C# build complete!"
