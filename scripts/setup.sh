#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "$REPO_ROOT"

acore_log_info "Running setup for all projects..."

for helper_setup in "${SCRIPT_DIR}/helpers"/*/setup.sh; do
	[ -f "$helper_setup" ] || continue
	bash "$helper_setup"
done

for project_setup in "${REPO_ROOT}"/src/*/scripts/setup.sh; do
	[ -f "$project_setup" ] || continue
	project_name="$(basename "$(dirname "$(dirname "$project_setup")")")"
	acore_log_divider
	acore_log_info "▶️ Running setup in src/$project_name..."
	bash "$project_setup"
done

acore_log_divider
acore_log_success "📦 All setup tasks completed!"
