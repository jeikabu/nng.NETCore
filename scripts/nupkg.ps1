param(
    [string]$Version,
    [string]$NugetApiKey,
    [string]$Nuget,
    [string]$Pfx
)

$ErrorActionPreference = "Stop"

if (-not $(Test-Path $Pfx -PathType Leaf)) {
    throw "Need pfx"
}

if (-not $(Test-Path $Nuget -PathType Leaf) -and -not $(Get-Command nuget -ErrorAction Ignore)) {
    throw "Nuget executable not found"
}

dotnet pack -c Release

$packages = "./bin/Release/Subor.nng.NETCore.$Version.nupkg", "./bin/Release/Subor.nng.NETCore.Shared.$Version.nupkg"

foreach ($pkg in $packages) {
    & $Nuget sign $pkg -Timestamper http://sha256timestamp.ws.symantec.com/sha256/timestamp -CertificatePath $Pfx
}

foreach ($pkg in $packages) {
    dotnet nuget push $pkg -k $NugetApiKey -s https://api.nuget.org/v3/index.json
}
