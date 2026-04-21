#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "$REPO_ROOT"

acore_log_info "Running build for all projects..."

for helper_build in "${SCRIPT_DIR}/helpers"/*/build.sh; do
	[ -f "$helper_build" ] || continue
	bash "$helper_build"
done

for project_build in "${REPO_ROOT}"/src/*/scripts/build.sh; do
	[ -f "$project_build" ] || continue
	project_name="$(basename "$(dirname "$(dirname "$project_build")")")"
	acore_log_divider
	acore_log_info "▶️ Running build in src/$project_name..."
	bash "$project_build"
done

acore_log_divider
acore_log_success "🛠️ All build tasks completed!"
