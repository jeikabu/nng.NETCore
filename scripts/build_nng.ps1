#!/usr/bin/env pwsh

# Updates Windows binaries.  On Windows, must be called from Developer PowerShell
param([string]$nng_source = "../nng/",
[switch]$clean,
[string]$runtimes = "$PSScriptRoot/../nng.NETCore/runtimes"
)

if (-not $(Test-Path $nng_source -PathType Container)){
    throw "$nng_source is not valid directory"
}

$is_windows = $IsWindows -or $PSVersionTable.PSEdition -eq "Desktop"

if ($is_windows -and -not $(Get-Command msbuild -ErrorAction Ignore)) {
    throw "Not in Visual Studio developer powershell"
}

$platforms = @()
if ($is_windows) {
    $platforms = @("win-x86","win32"), @("win-x64", "x64")
} elseif ($IsMacOS) {
    # Need comma to create an array of arrays/tuples.  See:
    # https://stackoverflow.com/questions/11138288/how-do-i-create-array-of-arrays-in-powershell
    $platforms = ,@("osx-x64", "")
} elseif ($IsLinux) {
    $platforms = ,@("linux-x64", "")
} else {
    throw "Unrecognized platform"
}

$current_dir = $(Get-Location)
try {
    Write-Host "Placing nng $platforms binaries in $runtimes..."
    Set-Location $nng_source
    foreach ($platform in $platforms) {
        $path, $arch = $platform
        
        $build_path = "build_$path"
        Write-Host "Building $arch in $build_path..."
        if ($clean) {
            Remove-Item -Recurse $build_path -ErrorAction Ignore
        }
        New-Item -ItemType Directory $build_path -Force
        Push-Location $build_path

        $dll = ""
        if ($is_windows) {
            cmake -A $arch -G "Visual Studio 16 2019" -DBUILD_SHARED_LIBS=ON -DNNG_TESTS=OFF -DNNG_TOOLS=OFF ..
            msbuild nng.sln -t:Rebuild -p:Configuration=Release
            $dll = "Release/nng.dll"
        } else {
            cmake -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=ON -DNNG_TESTS=OFF -DNNG_TOOLS=OFF ..
            make -j2
            if ($IsMacOS) {
                $dll = "libnng.dylib"
            } else {
                $dll = "libnng.so"
            }
        }
        Copy-Item $dll "$runtimes/$path/native" -Force
        
        Pop-Location
    }
} finally {
    Set-Location $current_dir
}
