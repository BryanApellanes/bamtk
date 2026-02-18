#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BAMTEST_PROJECT="$SCRIPT_DIR/submodules/bamtest/bamtest/bamtest.csproj"
BAMTEST_BIN="$SCRIPT_DIR/submodules/bamtest/bamtest/bin/Release/net10.0/bamtest"
SOLUTION="$SCRIPT_DIR/bamtk.sln"
TEST_DIR="$SCRIPT_DIR/.bam/tests"
COVERAGE_XML="$TEST_DIR/bamtk.coverage.cobertura.xml"
REPORT_DIR="$TEST_DIR/coverage-report"

# Ensure test output directory exists
mkdir -p "$TEST_DIR"

# Build bamtest if not already built
if [ ! -f "$BAMTEST_BIN" ] && [ ! -f "${BAMTEST_BIN}.exe" ]; then
    echo "Building bamtest..."
    dotnet build "$BAMTEST_PROJECT" -c Release -v quiet
fi

# Determine the executable
if [ -f "${BAMTEST_BIN}.exe" ]; then
    BAMTEST="${BAMTEST_BIN}.exe"
elif [ -f "$BAMTEST_BIN" ]; then
    BAMTEST="$BAMTEST_BIN"
else
    BAMTEST=""
fi

# Run tests from .bam/tests directory, discovering projects from the solution
echo "Running tests from $TEST_DIR..."
pushd "$TEST_DIR" > /dev/null

TEST_SWITCH="${1:---ut}"

if [ -z "$BAMTEST" ]; then
    echo "bamtest binary not found, using dotnet run..."
    dotnet run --project "$BAMTEST_PROJECT" -- \
        "$TEST_SWITCH" \
        --sln="$SOLUTION" \
        --coverage \
        --coverage-output="$COVERAGE_XML" \
        --coverage-format=cobertura
else
    "$BAMTEST" \
        "$TEST_SWITCH" \
        --sln="$SOLUTION" \
        --coverage \
        --coverage-output="$COVERAGE_XML" \
        --coverage-format=cobertura
fi

popd > /dev/null

# Generate HTML coverage report if coverage XML exists
if [ -f "$COVERAGE_XML" ]; then
    echo ""
    echo "Generating HTML coverage report..."

    # Install reportgenerator if not available
    if ! command -v reportgenerator &> /dev/null; then
        echo "Installing reportgenerator..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    fi

    reportgenerator \
        "-reports:$COVERAGE_XML" \
        "-targetdir:$REPORT_DIR" \
        "-reporttypes:Html" \
        "-title:bamtk Test Coverage"

    REPORT_FILE="$REPORT_DIR/index.html"
    if [ -f "$REPORT_FILE" ]; then
        echo ""
        echo "Coverage report: $REPORT_FILE"
        echo "Opening in browser..."

        # Open the report (cross-platform)
        if command -v start &> /dev/null; then
            start "$REPORT_FILE"
        elif command -v xdg-open &> /dev/null; then
            xdg-open "$REPORT_FILE"
        elif command -v open &> /dev/null; then
            open "$REPORT_FILE"
        else
            echo "Could not auto-open. Please open manually: $REPORT_FILE"
        fi
    fi
else
    echo ""
    echo "No coverage XML found at $COVERAGE_XML — skipping report generation."
fi
