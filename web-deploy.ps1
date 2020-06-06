# Before running:
# Check if PS execution policy is ok
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
# Check MSBuild, Nuget and VSTest exe paths are valid


<#
.Synopsis
Script to build, deploy, test and package web application.

.DESCRIPTION
Script to build, deploy, test and package web application.
1. Get latest source
2. Run webpack
3. Restore packages
4. Change AssemblyVersion
5. Build project
6. Run Tests
7. Deploy - file system
8. Package app
9. Optional: clean artifacts

.EXAMPLE
./web-deploy -Branch "master" -CleanEnvironment $true

#>

#Script web-deploy
#Creator Alan Macgowan
#Date 06/07/2020
#Updated
#References
# 1. Get latest source
# 2. Run webpack
# 3. Restore packages
# 4. Change AssemblyVersion
# 5. Build project
# 6. Run Tests
# 7. Deploy - file system
# 8. Package app

#other:
# run code metrics
# run code coverage
# smoke tests
# deploy remote iis?
#


param(
    [String]$GitRepository = "https://github.com/alanmacgowan/WebApplication.git",
    [String]$Branch = "master",
    [String]$PublishUrl = "c:\Jenkins_builds\sites\dev",
    [bool]$CleanEnvironment = $false
)

$MSBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
$NugetPath = "C:\Program Files (x86)\Jenkins\nuget.exe"
$VSTestPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$SourcesFolder = $PSScriptRoot + "\sources"

Function Initialize-Directory{
    Write-Host "Checking directory exists" -ForegroundColor Green
    If (-Not(Test-Path $SourcesFolder)) {
        Write-Host "Creating directory SourcesFolder" -ForegroundColor Green
        New-Item -ItemType "directory" -Path  $SourcesFolder
    }
}

Function Get-SourceCode{
    Write-Host "Getting Source Code - Branch: $Branch" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication"
    If (Test-Path $SourcePath) {
        Set-Location $SourcePath
        git pull origin $Branch
    }
    Else {
        git clone -b $Branch $GitRepository $SourcePath
    }
}

Function Get-Packages{
    Write-Host "Getting Nuget packages" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication"
    Set-Location $SourcePath
    $Args = @("restore", "WebApplication.sln");

    & $NugetPath $Args
}

Function Build-Webpack{
    Write-Host "Running Webpack" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication\WebApplication"
    Set-Location $SourcePath
    npm install
    npm run build:prod
}

Function Build-Solution{
    Write-Host "Building Solution" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication"
    Set-Location $SourcePath
    $Args = @("WebApplication.sln", "/p:DeployOnBuild=true", "/p:DeployDefaultTarget=WebPublish", "/p:WebPublishMethod=FileSystem", "/p:SkipInvalidConfigurations=true", "/t:build", "/p:Configuration=Release", "/p:Platform=`"Any CPU`"", "/p:DeleteExistingFiles=True", "/p:publishUrl=$PublishUrl");

    & $MSBuildPath $Args
}

Function Run-Tests{
    Write-Host "Running Tests" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication\WebApplication.Tests.Unit\bin\Release"
    Set-Location $SourcePath
    $Args = @("WebApplication.Tests.Unit.dll", "/Logger:trx;LogFileName=Results.trx", "/Framework:Framework45");

    & $VSTestPath $Args
}

Function Compress-Site{
    Write-Host "Generating zip file" -ForegroundColor Green
    Set-Location $SourcesFolder
    $CompressedFile = $PSScriptRoot + "\webapp_$((Get-Date).ToString("yyyyMMdd_HHmmss")).zip"
    Compress-Archive -Path $PublishUrl -DestinationPath $CompressedFile
}

Function Publish-Site{
    $ErrorActionPreference = 'Stop'
    #Try{
        Initialize-Directory

        Get-SourceCode
    
        Get-Packages
    
        Build-Webpack

        Build-Solution
    
        Run-Tests

        Compress-Site
    <#}
    Catch{
        Write-Host "Error" -ForegroundColor Red
        Write-Host "Message: [$($_.Exception.Message)"] -ForegroundColor Red -BackgroundColor DarkBlue
    }
    Finally{#>
        Set-Location $PSScriptRoot
    #}
}

& Publish-Site