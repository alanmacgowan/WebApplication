# Before running:
# Check if PS execution policy is ok
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
# Check MSBuild, Nuget, MSDeploy and VSTest exe paths are valid


<#
.Synopsis
Script to build, deploy, test and package web application.

.DESCRIPTION
Script to build, deploy, test and package web application.
1. Get latest source
2. Run webpack
3. Restore packages
4. Change AssemblyVersion
5. Package site
6. Run Tests
7. Deploy - IIS
8. Run Acceptance Tests
9. Optional: clean artifacts

#other:
# run code metrics
# run code coverage
# smoke tests


.EXAMPLE
./web-publish -Version "1.0.0.0" -Branch "master" -CleanEnvironment $true

#>

#Script web-publish
#Creator Alan Macgowan
#Date 06/07/2020
#Updated
#References


param(
    [Parameter(Mandatory=$true)][String]$Version,
    [String]$GitRepository = "https://github.com/alanmacgowan/WebApplication.git",
    [String]$Branch = "master",
    [String]$PublishUrl = "c:\Jenkins_builds\files",
    [String]$IISSite = "TestAppDev",
    [bool]$CleanEnvironment = $false
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $Arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $Arguments
    Break
}

$MSBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
$NugetPath = "C:\Program Files (x86)\Jenkins\nuget.exe"
$VSTestPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$MSDeployPath = "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
$SourcesFolder = $PSScriptRoot + "\sources"
$DateStamp = $((Get-Date).ToString("yyyyMMdd_HHmmss"))

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
        git checkout -f $Branch
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

Function Update-AssemblyVersion{
  Write-Host "Updating AssemblyVersion to $Version" -ForegroundColor Green  
  $SourcePath = $SourcesFolder + "\WebApplication\WebApplication"
  Set-Location $SourcePath
  $PatternVersion = '\[assembly: AssemblyVersion\("(.*)"\)\]'
  $PatternFileVersion = '\[assembly: AssemblyFileVersion\("(.*)"\)\]'
  $AssemblyFiles = Get-ChildItem . AssemblyInfo.cs -rec

  Foreach ($File in $AssemblyFiles)
  {
    (Get-Content $File.PSPath) | ForEach-Object{
        If($_ -match $PatternVersion){
            $FileVersion = [version]$Matches[1]
            '[assembly: AssemblyVersion("{0}")]' -f $Version
        } ElseIf($_ -match $PatternFileVersion){
            $FileVersion = [version]$Matches[1]
            '[assembly: AssemblyFileVersion("{0}")]' -f $Version
        } Else {
            $_
        }
    } | Set-Content $file.PSPath
  }
}

Function Package-Site{
    Write-Host "Building Solution" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication"
    Set-Location $SourcePath
    $Args = @("WebApplication.sln", "/p:DeployOnBuild=true", "/p:WebPublishMethod=Package", "/p:PackageAsSingleFile=true", "/p:SkipInvalidConfigurations=true", "/t:build", "/p:Configuration=Release", "/p:Platform=`"Any CPU`"", "/p:DesktopBuildPackageLocation=`"$PublishUrl\WebApp_$DateStamp.zip`"", "/p:DeployIisAppPath=`"$IISSite`"");

    & $MSBuildPath $Args
}

Function Run-Tests{
    Param($ProjectName)
    Write-Host "Running Tests $ProjectName" -ForegroundColor Green
    $SourcePath = $SourcesFolder + "\WebApplication\$ProjectName\bin\Release"
    Set-Location $SourcePath
    $Args = @("$ProjectName.dll", "/Logger:trx;LogFileName=Results.trx", "/Framework:Framework45");

    & $VSTestPath $Args
}

Function Deploy-Site{
    Write-Host "Deploying Site $IISSite" -ForegroundColor Green
    Set-Location $SourcesFolder
    $Args = @("-source:package=`"$PublishUrl\WebApp_$DateStamp.zip`"", "-verb:sync", "-dest:auto");

    & $MSDeployPath $Args
}


Function Smoke-Test{
    $Result = Invoke-WebRequest http://localhost:8090/
    If ($Result.StatusCode -ne 200) {
      Write-Error "Did not get 200 OK"
    } Else{
      Write-Host "Successfully connect." -ForegroundColor Green  
    }
}


Function Publish-Site{
    $ErrorActionPreference = 'Stop'
    #Try{
        Initialize-Directory

        Get-SourceCode
    
        Get-Packages
    
        Build-Webpack

        Update-AssemblyVersion

        Package-Site
    
        Run-Tests "WebApplication.Tests.Unit"

        Deploy-Site

        Smoke-Test

        Run-Tests "WebApplication.Tests.Acceptance"
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
