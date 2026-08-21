[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Project,
    [Parameter(Mandatory)] [string] $PayloadJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$doc = $PayloadJson | ConvertFrom-Json
$tool = [string]$doc.tool
if ([string]::IsNullOrWhiteSpace($tool)) { throw 'Payload tool name is required.' }

$cliArgs = @('--invoke', $tool)
if ($doc.arguments) {
    foreach ($prop in $doc.arguments.PSObject.Properties) {
        $cliArgs += "--$($prop.Name)"
        $cliArgs += [string]$prop.Value
    }
}

& dotnet run --project $Project --nologo -v q -- @cliArgs
