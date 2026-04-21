#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "$REPO_ROOT"

acore_log_info "Running tests for all projects..."

for helper_test in "${SCRIPT_DIR}/helpers"/*/test.sh; do
	[ -f "$helper_test" ] || continue
	bash "$helper_test"
done

for project_test in "${REPO_ROOT}"/src/*/scripts/test.sh; do
	[ -f "$project_test" ] || continue
	project_name="$(basename "$(dirname "$(dirname "$project_test")")")"
	acore_log_divider
	acore_log_info "▶️ Running tests in src/$project_name..."
	bash "$project_test"
done

acore_log_divider
acore_log_success "🧪 All test suites completed!"
