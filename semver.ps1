param(
    [string]$packageVersion = $null,
    [string]$config = "Release",
    [string]$target = "Rebuild",
    [string]$verbosity = "Minimal"
)

$rootFolder = Split-Path -parent $script:MyInvocation.MyCommand.Path
. $rootFolder\myget.include.ps1

$packageVersion = MyGet-Package-Version $packageVersion

$project = "semver\semver.csproj"        
$platform = "AnyCPU"
$targetFrameworks = @("v2.0","v3.5","v4.0","v4.5","v4.5.1")

$projectName = [System.IO.Path]::GetFileName($project) -ireplace ".(sln|csproj)$", ""
$outputFolder = Join-Path $rootFolder "bin\$projectName"
$buildOutputPath = Join-Path $outputFolder "$packageVersion\$platform\$config"

MyGet-AssemblyVersion-Set -projectFolder $project -version $packageVersion

MyGet-Build-Project -rootFolder $rootFolder `
    -outputFolder $outputFolder `
    -project $project `
    -config $config `
    -target $target `
    -targetFrameworks $targetFrameworks `
    -platform $platform `
    -verbosity $verbosity `
    -version $packageVersion `
        
MyGet-Build-Nupkg -rootFolder $rootFolder `
    -outputFolder $buildOutputPath `
    -project $project `
    -config $config `
    -version $packageVersion `
    -platform $platform `
    -nugetIncludeSymbols $false