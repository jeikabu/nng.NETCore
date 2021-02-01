param(
    [string]$Version,
    [string]$NugetApiKey = "",
    [string]$Nuget = "",
    [string]$Pfx = ""
)

$ErrorActionPreference = "Stop"

# Strip the leading "v".  E.g. "v1.3.2-rc0" => "1.3.2-rc0"
$Version = $Version -replace "^v",""

$nupkgs = "./bin/Release/*.nupkg"
Remove-Item $nupkgs -Force -ErrorAction Ignore

dotnet pack --configuration Release -p:Version=$Version

$packages = Get-ChildItem $nupkgs

if ($Pfx) {
    if (-not $(Test-Path $Pfx -PathType Leaf)) {
        throw "Need pfx"
    }    
    if (-not $(Test-Path $Nuget -PathType Leaf) -and -not $(Get-Command nuget -ErrorAction Ignore)) {
        throw "Nuget executable not found"
    }
    foreach ($pkg in $packages) {
        & $Nuget sign $pkg -Timestamper http://sha256timestamp.ws.symantec.com/sha256/timestamp -CertificatePath $Pfx
    }
}

if ($NugetApiKey) {
    foreach ($pkg in $packages) {
        dotnet nuget push $pkg -k $NugetApiKey -s https://api.nuget.org/v3/index.json
    }    
}
