# Jenkins CI with declarative pipelines
Sample Application for CI/CD using Jenkins with declarative Pipelines as Continuous Integration server.

![Jenkins Pipeline](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/JENKINS%20BLUE%20OCEAN.png)

_Jenkins Pipeline Stages in Blue Ocean_

## Diagram

![CI Diagram](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/ci%20diagram.png)

## Tools:
Area |Tools
-----|------
CI | Jenkins 
Source Code | GitHub (Cloud)
Issue Management | GitHub (Cloud)
Tech stack | ASP.Net MVC, EF, JQuery, Bootstrap, Webpack
Database | SQL Server
Web Server | IIS
Testing | MSTest, Specflow, Selenium, Moq
Build Tools |MSBuild, MSDeploy
Dependencies | Nuget, npm

## Setup

### Jenkins

### Plugins:
* github
* git
* global-slack-notifier
* pollscm
* timestamper
* mstest
* msbuild
* nodejs
* vstestrunner
* workflow-aggregator

### Config File Management
Added web.config files for Dev, QA and Prod.

![Config files](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/JENKINS%20CONFIG%20FILES.png)

### Gihub
Need to add a webhook that points to the Jenkins server.

![Github webhooks setup](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/GITHUB%20WEBHOOK.png)

_Github webhooks setup_

## jenkinsfile:

### Environment section
Sets RELEASE_VERSION and tools paths.
```
environment{
    RELEASE_VERSION = "1.0.1"
    VSTest = tool 'vstest'	
    MSBuild = tool 'msbuild'
    Nuget = 'C:\\Program Files (x86)\\Jenkins\\nuget.exe'
    MSDeploy = "C:\\Program Files (x86)\\IIS\\Microsoft Web Deploy V3\\msdeploy.exe"
}
```
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
![Github Pending Status](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/GITHUB%20NOTIF%20PENDING.png)

_Github Pending status_

### Stage Restore dependencies
This stage runs 3 parallel stages: Nuget Restore, NodeJS and Set Assembly Version.

### Nuget
Restores nuget packages, runs in parallel.
```
stage('Restore'){
    steps{
        bat "\"${Nuget}\" restore WebApplication.sln"
    }
}
```
### Stage NodeJS
Installs npm dependencies and runs webpack, runs in parallel.
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
Changes Assemblyversion number with current Build number, using Powershell script, runs in parallel.
```
stage('Set Assembly Version') {
  steps {
        dir('WebApplication\\Properties')
        {
            setAssemblyVersion()
        }
   }
}
...
void setAssemblyVersion(){
	powershell ("""
	  \$PatternVersion = '\\[assembly: AssemblyVersion\\("(.*)"\\)\\]'
	  \$AssemblyFiles = Get-ChildItem . AssemblyInfo.cs -rec

	  Foreach (\$File in \$AssemblyFiles)
	  {
		(Get-Content \$File.PSPath) | ForEach-Object{
			If(\$_ -match \$PatternVersion){
				'[assembly: AssemblyVersion("{0}")]' -f "$RELEASE_VERSION.$BUILD_NUMBER"
			} Else {
				\$_
			}
		} | Set-Content \$file.PSPath
	  }
    """)
}

```
![Version](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/APP%20VERSION.png)

_Version number shown in footer_

### Stage Build & Package
Build and packages WebApplication for latter deployment. The artifact is saved as part of the build in Jenkins.
```
stage('Build & Package') {
    steps {
	bat "\"${MSBuild}\" WebApplication.sln /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /t:build /p:Configuration=QA /p:Platform=\"Any CPU\" /p:DesktopBuildPackageLocation=\"%WORKSPACE%\\artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip\""
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
            bat "\"${VSTest}\" \"WebApplication.Tests.Unit.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_NUMBER}.trx /Framework:Framework45"
        }
        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
    }
}
```
![Unit and Acceptance Tests results](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/JENKINS%20TESTS.png)

_Unit and Acceptance Tests results_

### Stage Deploy to QA
Deploys package to QA IIS site, web.config is replaced by config file in Jenkins.
```
stage('Deploy to QA') {
  steps {
      bat "\"${MSDeploy}\" -source:package=\"%WORKSPACE%\\artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip\"  -verb:sync -dest:auto -allowUntrusted=true -setParam:name=\"IIS Web Application Name\",value=\"TestAppQA\""
      configFileProvider([configFile(fileId: 'web.QA.config', targetLocation: 'C:\\Jenkins_builds\\sites\\qa\\web.config')]) {}
  }
}
```
### Stage Smoke Test QA
Smoke Test QA IIS site, using Powershell script.
```
stage('Smoke Test QA') {
  steps {
    smokeTest("http://localhost:8091/")
  }
}
...
void smokeTest(String url){
    def status = powershell (returnStatus: true, script: powershell ("""
		\$result = Invoke-WebRequest $url
		if (\$result.StatusCode -ne 200) {
			Write-Error \"Did not get 200 OK\"
			exit 1
		} else{
			Write-Host \"Successfully connect.\"
		}
    """)
    if (status != 0) {
       error "This pipeline stops here!"
    }
}
```
### Stage Acceptance test
Runs acceptance tests using QA site.
```
stage('Acceptance test') {
    steps {
        dir('WebApplication.Tests.Acceptance\\bin\\Release')
        {
            bat "\"${VSTest}\" \"WebApplication.Tests.Acceptance.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_NUMBER}.trx /Framework:Framework45"
        }
        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
    }
}
```
### Stage Deploy to Prod
This stage waits for input from user to deploy.
Deploys package to Prod IIS site, web.config is replaced by config file in Jenkins.
```
stage('Deploy to Prod') {
  input {
    message 'Deploy to Prod?'
    ok 'Yes'
  }
  steps {
      bat "\"${MSDeploy}\" -source:package=\"%WORKSPACE%\\artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip\"  -verb:sync -dest:auto -allowUntrusted=true -setParam:name=\"IIS Web Application Name\",value=\"TestAppProd\""
      configFileProvider([configFile(fileId: 'web.Prod.config', targetLocation: 'C:\\Jenkins_builds\\sites\\prod\\web.config')]) {}
  }
} 
```
![Input for Deploy to Prod stage](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/JENKINS%20INPUT%20STEP.png)

### Stage Smoke Test Prod
Smoke Test Prod IIS site, using Powershell script.
```
stage('Smoke Test Prod') {
  steps {
    smokeTest("http://localhost:8092/")
  }
}
```
### Post Section
Notification and archiving.
```
post { 
  failure { 
    slackSend (color: '#FF0000', message: "FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
    setBuildStatus("FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})", "FAILURE");
  }
  success{
    slackSend (color: '#00FF00', message: "SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
    setBuildStatus("SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})", "SUCCESS");
  }
  always {
    archiveArtifacts "artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip"
  }
}
```
![Artifacts](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/JENKINS%20ARTIFACTS.png)

_Artifacts stored in Jenkins_

![Version](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/GITHUB%20NOTIF%20SUCCESS.png)

_Github successful notification_

![Version](https://github.com/alanmacgowan/alanmacgowan.github.io/blob/master/SLACK%20NOTIF.png)

_Slack notifications_


## Includes:
* Jenkinsfile: for declarative pipeline on Jenkins, triggers from SCM on push to master branch, builds and deploys to local IIS Server.
* web-deploy.ps1: powershell script that builds, deploys to file system and package web application.
* web-publish.ps1: powershell script that builds, packages and deploys to IIS web application.
* Docker/Dockerfile: Dockerfile for linux image with jenkins installed and plugins.
* Docker/Dockerfile_Windows: Dockerfile for windows image(based on: https://blog.alexellis.io/continuous-integration-docker-windows-containers/)

## Resources:
### Pluralsight:
[Using Declarative Jenkins Pipelines](https://app.pluralsight.com/library/courses/using-declarative-jenkins-pipelines/table-of-contents)

[Getting Started with Jenkins 2](https://app.pluralsight.com/library/courses/jenkins-2-getting-started)

[Building a Modern CI/CD Pipeline with Jenkins](https://app.pluralsight.com/library/courses/building-modern-ci-cd-pipeline-jenkins)


