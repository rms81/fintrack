#!/usr/bin/env bash

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/build/_build.csproj"
TEMP_DIRECTORY="$SCRIPT_DIR/.nuke/temp"

DOTNET_GLOBAL_FILE="$SCRIPT_DIR/global.json"
DOTNET_INSTALL_URL="https://dot.net/v1/dotnet-install.sh"
DOTNET_CHANNEL="STS"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

###########################################################################
# EXECUTION
###########################################################################

function FirstJsonValue {
    perl -nle 'print $1 if m{"'"$1"'"\s*:\s*"([^"]+)"}' <<< "${@:2}"
}

# If dotnet CLI is installed globally and meets version requirements, use it
if [ -x "$(command -v dotnet)" ]; then
    export DOTNET_EXE="$(command -v dotnet)"
else
    # If global.json exists, read SDK version
    if [[ -f "$DOTNET_GLOBAL_FILE" ]]; then
        DOTNET_VERSION=$(FirstJsonValue "version" "$(cat "$DOTNET_GLOBAL_FILE")")
        if [[ "$DOTNET_VERSION" == "" ]]; then
            unset DOTNET_VERSION
        fi
    fi

    # Install dotnet locally
    DOTNET_DIRECTORY="$TEMP_DIRECTORY/dotnet"
    export DOTNET_ROOT="$DOTNET_DIRECTORY"
    export DOTNET_EXE="$DOTNET_DIRECTORY/dotnet"

    if [ ! -f "$DOTNET_EXE" ]; then
        mkdir -p "$DOTNET_DIRECTORY"
        curl -sSL "$DOTNET_INSTALL_URL" -o "$TEMP_DIRECTORY/dotnet-install.sh"
        chmod +x "$TEMP_DIRECTORY/dotnet-install.sh"

        if [[ -z ${DOTNET_VERSION+x} ]]; then
            "$TEMP_DIRECTORY/dotnet-install.sh" --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_DIRECTORY" --no-path
        else
            "$TEMP_DIRECTORY/dotnet-install.sh" --version "$DOTNET_VERSION" --install-dir "$DOTNET_DIRECTORY" --no-path
        fi
    fi
fi

echo "Microsoft (R) .NET SDK version $("$DOTNET_EXE" --version)"

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" -c Release -o "$TEMP_DIRECTORY" /nologo -v q
"$TEMP_DIRECTORY/_build" "$@"
