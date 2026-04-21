SHELL := bash
.PHONY: build test clean restore format

help:
	@echo "Available commands:"
	@echo "  make setup    - Restore NuGet packages"
	@echo "  make build   - Build the solution"
	@echo "  make format - Run formatters"
	@echo "  make lint   - Run lint checks"
	@echo "  make test  - Run test suites"
	@echo "  make clean - Clean build artifacts"

setup:
	dotnet restore NFramework.Persistence.slnx

build: restore
	dotnet build NFramework.Persistence.slnx

format:
	./scripts/format.sh

lint:
	./scripts/lint.sh

test:
	./scripts/test.sh

clean:
	dotnet clean NFramework.Persistence.slnx
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true
