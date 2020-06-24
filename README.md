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
```
stage('Get Source'){
  steps{
    slackSend (color: '#FFFF00', message: "STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
    setBuildStatus("PENDING: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})", "PENDING");
    checkout([$class: 'GitSCM', branches: [[name: 'master']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'github', url: 'https://github.com/alanmacgowan/WebApplication.git']]])
  }
}
```
## Includes:
* Jenkinsfile: for multibranch pipeline on Jenkins, triggers from SCM on PR merged to develop branch, builds and deploys to local IIS Server.
* web-deploy.ps1: powershell script that builds, deploys to file system and package web application.
* web-publish.ps1: powershell script that builds, packages and deploysto IIS web application.
* azure-pipelines.yml: configuration file for Azure Devops pipeline.
