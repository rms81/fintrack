#!/usr/bin/env pwsh

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$PSDefaultParameterValues['*:ErrorAction'] = 'Stop'

$ScriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

###########################################################################
# CONFIGURATION
###########################################################################

$BuildProjectFile = Join-Path $ScriptDir "build/_build.csproj"
$TempDirectory = Join-Path $ScriptDir ".nuke/temp"

$DotNetGlobalFile = Join-Path $ScriptDir "global.json"
$DotNetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
$DotNetChannel = "STS"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_NOLOGO = 1

###########################################################################
# EXECUTION
###########################################################################

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

# If dotnet CLI is installed globally and meets version requirements, use it
$DotNetExe = Get-Command dotnet -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path
if ($null -eq $DotNetExe) {
    # If global.json exists, read SDK version
    if (Test-Path $DotNetGlobalFile) {
        $DotNetVersion = (Get-Content $DotNetGlobalFile | ConvertFrom-Json).sdk.version
    }

    # Install dotnet locally
    $DotNetDirectory = Join-Path $TempDirectory "dotnet"
    $env:DOTNET_ROOT = $DotNetDirectory
    $DotNetExe = Join-Path $DotNetDirectory "dotnet"

    if (-not (Test-Path $DotNetExe)) {
        New-Item -ItemType Directory -Path $DotNetDirectory -Force | Out-Null
        $DotNetInstallFile = Join-Path $TempDirectory "dotnet-install.ps1"
        Invoke-WebRequest $DotNetInstallUrl -OutFile $DotNetInstallFile

        if ($null -eq $DotNetVersion) {
            & $DotNetInstallFile -Channel $DotNetChannel -InstallDir $DotNetDirectory -NoPath
        } else {
            & $DotNetInstallFile -Version $DotNetVersion -InstallDir $DotNetDirectory -NoPath
        }
    }
}

Write-Host "Microsoft (R) .NET SDK version $(& $DotNetExe --version)"

ExecSafe { & $DotNetExe build $BuildProjectFile -c Release -o $TempDirectory /nologo -v q }
ExecSafe { & "$TempDirectory/_build" @args }
