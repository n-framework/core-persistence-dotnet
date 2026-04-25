SHELL := bash
.PHONY: setup build format lint test clean

help:
	@echo "Available commands:"
	@echo "  make setup    - Restore NuGet packages"
	@echo "  make build   - Build the solution"
	@echo "  make format - Run formatters"
	@echo "  make lint   - Run lint checks"
	@echo "  make test  - Run test suites"
	@echo "  make test-aot - Run Native AOT smoke tests"
	@echo "  make clean - Clean build artifacts"

setup:
	dotnet restore NFramework.Persistence.slnx

build:
	dotnet build NFramework.Persistence.slnx

format:
	./scripts/format.sh

lint:
	./scripts/lint.sh

test:
	./scripts/test.sh

test-aot:
	dotnet publish tests/smoke/NFramework.Persistence.AotSample/NFramework.Persistence.AotSample.csproj -c Release -r linux-x64 --self-contained /p:PublishAot=true

clean:
	dotnet clean NFramework.Persistence.slnx
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true
