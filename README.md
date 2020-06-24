# WebApplication
Sample Application for CI/CD


## Tools:
* CI: Jenkins 
* Source Code: GitHub (Cloud)
* Issue Management: GitHub (Cloud)
* Tech stack: ASP.Net MVC, EF, JQuery, Bootstrap, Webpack
* Database: SQL Server
* Web Server: IIS
* Testing: MSTest, Specflow, Selenium, Moq
* Build Tools: MSBuild, Nuget, MSDeploy

## jenkinsfile:

### Stage Get Source
Gets source code and notifies to Slack and Github.
```
stage('Get Source'){
  steps{
    slackSend (color: '#FFFF00', message: "STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
    setBuildStatus("PENDING: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})", "PENDING");
    checkout([$class: 'GitSCM', branches: [[name: 'master']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'github', url: 'https://github.com/alanmacgowan/WebApplication.git']]])
  }
}
```
### Stage Restore
Restores nuget packages.
```
stage('Restore'){
    steps{
        bat "\"${Nuget}\" restore WebApplication.sln"
    }
}
```
### Stage NodeJS
Installs npm dependencies and runs webpack.
```
stage('NodeJS'){
    steps{
        nodejs(nodeJSInstallationName: 'Node') {
            dir('WebApplication')
            {
                bat 'npm install && npm run build:prod'
            }
        }
    }
}	
```
### Stage Set Assembly Version
Changes Assemblyversion number with current Build number, using Powershell script.
```
stage('Set Assembly Version') {
  steps {
        dir('WebApplication\\Properties')
        {
          powershell ("""
            \$Pa  tternVersion = '\\[assembly: AssemblyVersion\\("(.*)"\\)\\]'
            \$AssemblyFiles = Get-ChildItem . AssemblyInfo.cs -rec
            \$BuildNumber = ${env.BUILD_ID}

            Foreach (\$File in \$AssemblyFiles)
            {
              (Get-Content \$File.PSPath) | ForEach-Object{
                If(\$_ -match \$PatternVersion){
                  \$fileVersion = [version]\$matches[1]
                  \$newVersion = "{0}.{1}.{2}.{3}" -f \$fileVersion.Major, \$fileVersion.Minor, \$BuildNumber, '*'
                  '[assembly: AssemblyVersion("{0}")]' -f \$newVersion
                } Else {
                  \$_
                }
              } | Set-Content \$file.PSPath
            }
        """)
        }
   }
}
```
### Stage Build & Package
Build and packages WebApplication for latter deployment.
```
stage('Build & Package') {
  steps {
      bat "\"${MSBuild}\" WebApplication.sln /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /t:build /p:Configuration=Production /p:Platform=\"Any CPU\" /p:DesktopBuildPackageLocation=c:\\Jenkins_builds\\files\\WebApp_${env.BUILD_ID}.zip /p:DeployIisAppPath=\"TestAppDev\""
    }
}
```
### Stage Unit test
Runs unit tests.
```
stage('Unit test') {
    steps {
        dir('WebApplication.Tests.Unit\\bin\\Release')
        {
            bat "\"${VSTest}\" \"WebApplication.Tests.Unit.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_ID}.trx /Framework:Framework45"
        }
        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
    }
}
```
### Stage Deploy
Deploys package to a local IIS site.
```
stage('Deploy') {
  steps {
      bat "\"${MSDeploy}\" -source:package=\"c:\\Jenkins_builds\\files\\WebApp_${env.BUILD_ID}.zip\"  -verb:sync -dest:auto -allowUntrusted=true "
    }
}
```
### Stage Smoke Test
Smoke Test local IIS site, using Powershell script.
```
stage('Smoke Test') {
  steps {
      powershell ("""
        \$result = Invoke-WebRequest http://localhost:8090/
        if (\$result.StatusCode -ne 200) {
          Write-Error \"Did not get 200 OK\"
        } else{
          Write-Host \"Successfully connect.\"
        }
      """)
      }
}
```
### Stage Acceptance test
Runs acceptance testsusing local IIS site.
```
stage('Acceptance test') {
    steps {
        dir('WebApplication.Tests.Acceptance\\bin\\Release')
        {
            bat "\"${VSTest}\" \"WebApplication.Tests.Acceptance.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_ID}.trx /Framework:Framework45"
        }
        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
    }
}
```

## Includes:
* Jenkinsfile: for multibranch pipeline on Jenkins, triggers from SCM on PR merged to develop branch, builds and deploys to local IIS Server.
* web-deploy.ps1: powershell script that builds, deploys to file system and package web application.
* web-publish.ps1: powershell script that builds, packages and deploys to IIS web application.
* azure-pipelines.yml: configuration file for Azure Devops pipeline.
