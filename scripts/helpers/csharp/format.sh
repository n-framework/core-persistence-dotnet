#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=packages/acore-scripts/src/logger.sh
source "${SCRIPT_DIR}/../../../packages/acore-scripts/src/logger.sh"

REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"
cd "$REPO_ROOT"

acore_log_section "🔧 Fixing C# code style violations (dotnet format)..."

for slnx_file in $(fd -e slnx . "$REPO_ROOT"); do
	found_slnx=("$slnx_file")
	dotnet format whitespace "${found_slnx[0]}" --severity warn &> /dev/null || true
	dotnet format style "${found_slnx[0]}" --severity warn &> /dev/null || true
	dotnet format analyzers "${found_slnx[0]}" --severity warn &> /dev/null || true
done

acore_log_section "🎨 Formatting C# code with CSharpier..."
dotnet csharpier format .

acore_log_success "✅ C# formatting complete!"
