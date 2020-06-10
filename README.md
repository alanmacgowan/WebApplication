# WebApplication
Sample Application for CI/CD

## Using:
* Asp.Net MVC (EF)
* Webpack
* JQuery & Bootstrap
* MSTest
* Moq
* Selenium
* Specflow

## Includes:
* Jenkins file: for multibranch pipeline on Jenkins, triggers from SCM on PR merged to develop branch, builds and deploys to local IIS Server.
* web-deploy.ps1: powershell script that builds, deploys to file system and package web application.
* web-publish.ps1: powershell script that builds, packages and deploysto IIS web application.
* azure-pipelines.yml: configuration file for Azure Devops pipeline.
