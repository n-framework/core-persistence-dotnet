#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../../../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"
cd "$REPO_ROOT"

acore_log_section "🔍 Linting C# code with dotnet build..."

for slnx_file in $(fd -e slnx . "$REPO_ROOT"); do
	acore_log_info "Analyzing: $slnx_file"
	dotnet restore "$slnx_file"

	acore_log_info "Verifying code style with dotnet format..."
	dotnet format "$slnx_file" style --verify-no-changes

	acore_log_info "Running strict C# analyzers..."
	dotnet build "$slnx_file" -warnaserror:true -verbosity:normal \
		-p:EnforceCodeStyleInBuild=true \
		-p:EnableCodeStyleSeverity=warning \
		-p:EnableNETAnalyzers=true \
		-p:AnalysisMode=All \
		-p:AnalysisLevel=latest
done

acore_log_success "✨ C# linting complete!"
