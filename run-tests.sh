#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BAMTEST_PROJECT="$SCRIPT_DIR/submodules/bamtest/bamtest/bamtest.csproj"
BAMTEST_BIN="$HOME/.bam/build/Release/bamtest/net10.0/bamtest"
SOLUTION="$SCRIPT_DIR/bamtk.sln"
TEST_DIR="$SCRIPT_DIR/.bam/tests"
COVERAGE_XML="$TEST_DIR/bamtk.coverage.cobertura.xml"
REPORT_DIR="$TEST_DIR/coverage-report"

# Determine argument style from BAM_ARG_STYLE or platform default
if [ -n "$BAM_ARG_STYLE" ]; then
    ARG_STYLE="$BAM_ARG_STYLE"
elif [[ "$(uname -s)" == MINGW* || "$(uname -s)" == MSYS* || "$(uname -s)" == CYGWIN* || "$OS" == "Windows_NT" ]]; then
    ARG_STYLE="Windows"
else
    ARG_STYLE="Posix"
fi

if [ "$ARG_STYLE" = "Windows" ]; then
    PREFIX="/"
    SEP=":"
else
    PREFIX="--"
    SEP="="
fi

# Export so bamtest also picks up the style
export BAM_ARG_STYLE="$ARG_STYLE"
# Prevent MSYS/Git Bash from mangling /arg into C:/Program Files/Git/arg
export MSYS_NO_PATHCONV=1

# Ensure test output directory exists
mkdir -p "$TEST_DIR"

# Build bamtest
echo "Building bamtest..."
dotnet build "$BAMTEST_PROJECT" -c Release -v quiet

# Determine the executable
if [ -f "${BAMTEST_BIN}.exe" ]; then
    BAMTEST="${BAMTEST_BIN}.exe"
elif [ -f "$BAMTEST_BIN" ]; then
    BAMTEST="$BAMTEST_BIN"
else
    echo "ERROR: bamtest binary not found after build" >&2
    exit 1
fi

# Run tests from .bam/tests directory, discovering projects from the solution
TEST_SWITCH="${1:-${PREFIX}ut}"
echo "Running tests ($ARG_STYLE style) from $TEST_DIR..."
pushd "$TEST_DIR" > /dev/null

"$BAMTEST" \
    "$TEST_SWITCH" \
    "${PREFIX}sln${SEP}${SOLUTION}" \
    "${PREFIX}coverage" \
    "${PREFIX}coverage-output${SEP}${COVERAGE_XML}" \
    "${PREFIX}coverage-format${SEP}cobertura"

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
    fi
else
    echo ""
    echo "No coverage XML found at $COVERAGE_XML — skipping report generation."
fi
