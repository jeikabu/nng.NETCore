#!/usr/bin/env pwsh

# Build NNG native shared libraries

param([string]$nng_source = "../nng/",
[switch]$clean,
[string]$git_branch,
[string]$runtimes = "$PSScriptRoot/../nng.NET/runtimes"
)

if (-not $IsLinux -and -not $(Test-Path $nng_source -PathType Container)){
    throw "$nng_source is not valid directory"
}

# $IsWindows is only in PowerShellCore, $PSVersionTable is for regular Powershell
$is_windows = $IsWindows -or $PSVersionTable.PSEdition -eq "Desktop"

$platforms = @()
if ($is_windows) {
    $platforms = @("win-x86","win32"), @("win-x64", "x64")
} elseif ($IsMacOS) {
    # Need comma to create an array of arrays/tuples.  See:
    # https://stackoverflow.com/questions/11138288/how-do-i-create-array-of-arrays-in-powershell
    $platforms = ,@("osx-x64", "")
} elseif ($IsLinux) {
    # Linux uses dockerfiles/build-nng
} else {
    throw "Unrecognized platform"
}

$current_dir = $(Get-Location)
try {
    if ($IsLinux) {
        docker build -t jeikabu/build-nng dockerfiles/build_nng
        docker run --rm --privileged multiarch/qemu-user-static:register
        docker run -i -t --rm -v "$PWD/nng.NET/runtimes:/runtimes" jeikabu/build-nng
    }
    else {
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
                cmake -A $arch -G "Visual Studio 16 2019" -DBUILD_SHARED_LIBS=ON -DNNG_ELIDE_DEPRECATED=ON -DNNG_TESTS=OFF -DNNG_TOOLS=OFF ..
                #msbuild nng.sln -t:Rebuild -p:Configuration=Release
                cmake --build . --config Release
                $dll = "Release/nng.dll"
            } else {
                cmake -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=ON -DNNG_ELIDE_DEPRECATED=ON -DNNG_TESTS=OFF -DNNG_TOOLS=OFF ..
                make -j2
                $dll = "libnng.dylib"
            }
            Copy-Item $dll "$runtimes/$path/native" -Force
            
            Pop-Location
        }
    }
} finally {
    Set-Location $current_dir
}

if ($git_branch) {
    if (!$(git config user.email) -or !(git config user.name)) {
        git config user.email "jeikabu@users.noreply.github.com"
        git config user.name "jeikabu"
    }
    
    git checkout -b $git_branch
    git add $runtimes
    git commit -m "Update native NNG libs"
    git push --set-upstream origin $git_branch
    if (!$?) {
        Write-Output "$? ExitCode: $LASTEXITCODE"
        git pull origin $git_branch
        Write-Output "$? ExitCode: $LASTEXITCODE"
        git push --set-upstream origin $git_branch
        Write-Output "$? ExitCode: $LASTEXITCODE"
    }
}