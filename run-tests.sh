#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BAMTEST_PROJECT="$SCRIPT_DIR/submodules/bamtest/bamtest/bamtest.csproj"
BAMTEST_BIN="$SCRIPT_DIR/submodules/bamtest/bamtest/bin/Release/net10.0/bamtest"

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
    # Fallback to dotnet run
    echo "bamtest binary not found, using dotnet run..."
    dotnet run --project "$BAMTEST_PROJECT" -- \
        --ut ${1:---coverage} \
        --coverage-output=bamtk.coverage.cobertura.xml \
        --coverage-format=cobertura
    exit $?
fi

"$BAMTEST" \
    --ut ${1:---coverage} \
    --coverage-output=bamtk.coverage.cobertura.xml \
    --coverage-format=cobertura
