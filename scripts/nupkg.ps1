param(
    [string]$Version,
    [string]$NugetApiKey = "",
    [string]$Nuget = "nuget",
    [string]$CertFile = "",
    [string]$CertBase64 = ""
)

$ErrorActionPreference = "Stop"

# Strip the leading "v".  E.g. "v1.3.2-rc0" => "1.3.2-rc0"
$Version = $Version -replace "^v",""

$nupkgs = "./bin/Release/*.nupkg"
Remove-Item $nupkgs -Force -ErrorAction Ignore

dotnet pack --configuration Release -p:Version=$Version

$packages = Get-ChildItem $nupkgs

if ($CertFile -or $CertBase64) {
    # If $Nuget isn't a path to a file and isn't a command in $PATH, download latest stable nuget.exe
    if (-not $(Test-Path $Nuget -PathType Leaf) -and -not $(Get-Command $Nuget -ErrorAction Ignore)) {
        $Nuget = "./nuget.exe"
        Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $Nuget
    }
    
    try {
        # If $Cert isn't a file assume it's a base64-encoded code-signing certificate
        $tempCert = $null
        if (-not $(Test-Path $CertFile -PathType Leaf)) {
            $tempCert = New-TemporaryFile
            [IO.File]::WriteAllBytes($tempCert.FullName, [Convert]::FromBase64String($CertBase64))
            $CertFile = $tempCert.FullName
        }
        
        foreach ($pkg in $packages) {
            & $Nuget sign $pkg -Timestamper http://sha256timestamp.ws.symantec.com/sha256/timestamp -CertificatePath $CertFile
        }
    } finally {
        if ($tempCert) {
            $tempCert.Delete()
        }
    }
    
}

if ($NugetApiKey) {
    foreach ($pkg in $packages) {
        dotnet nuget push $pkg -k $NugetApiKey -s https://api.nuget.org/v3/index.json
    }    
}
