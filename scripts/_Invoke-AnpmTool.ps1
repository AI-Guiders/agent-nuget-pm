[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Project,
    [Parameter(Mandatory)] [string] $PayloadJson,
    [string] $Config
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$doc = $PayloadJson | ConvertFrom-Json
$tool = [string]$doc.tool
if ([string]::IsNullOrWhiteSpace($tool)) { throw 'Payload tool name is required.' }

$runArgs = @()
if ($Config) {
    $runArgs += @('--config', $Config)
}

$runArgs += @('--invoke', $tool)
if ($doc.arguments) {
    foreach ($prop in $doc.arguments.PSObject.Properties) {
        $runArgs += "--$($prop.Name)"
        $runArgs += [string]$prop.Value
    }
}

& dotnet run --project $Project --nologo -v q -- @runArgs
