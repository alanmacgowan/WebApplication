pipeline {
			agent any
			environment{
				RELEASE_VERSION = "1.0.1"
                VSTest = tool 'vstest'	
				MSBuild = tool 'msbuild'
				Nuget = 'C:\\Program Files (x86)\\Jenkins\\nuget.exe'
				MSDeploy = "C:\\Program Files (x86)\\IIS\\Microsoft Web Deploy V3\\msdeploy.exe"
            }
			stages {
				stage('Get Source'){
				//	when { branch 'develop' }
					steps{
						slackSend (color: '#FFFF00', message: "STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
						setBuildStatus("PENDING: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})", "PENDING");
						checkout([$class: 'GitSCM', branches: [[name: 'master']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'github', url: 'https://github.com/alanmacgowan/WebApplication.git']]])
					}
				}
				stage('Restore dependencies'){
					parallel {
						stage('Restore'){
							//when { branch 'develop' }
							steps{
								bat "\"${Nuget}\" restore WebApplication.sln"
							}
						}
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
						stage('Set Assembly Version') {
							//when { branch 'develop' }
							steps {
								dir('WebApplication\\Properties')
								{
									setAssemblyVersion()
								}
							}
						}
					}
				}
				stage('Build & Package') {
					//when { branch 'develop' }
					steps {
					    bat "\"${MSBuild}\" WebApplication.sln /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /t:build /p:Configuration=QA /p:Platform=\"Any CPU\" /p:DesktopBuildPackageLocation=\"%WORKSPACE%\\artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip\" /p:DeployIisAppPath=\"TestAppDev\""
					}
				}
				stage('Unit test') {
					//when { branch 'develop' }
				    steps {
				        dir('WebApplication.Tests.Unit\\bin\\Release')
                        {
                            bat "\"${VSTest}\" \"WebApplication.Tests.Unit.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_ID}.trx /Framework:Framework45"
                        }
                        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
                    }
                }
				stage('Deploy') {
					//when { branch 'develop' }
					steps {
					    bat "\"${MSDeploy}\" -source:package=\"%WORKSPACE%\\artifacts\\WebApp_${env.RELEASE_VERSION}.${env.BUILD_NUMBER}.zip\"  -verb:sync -dest:auto -allowUntrusted=true "
                        configFileProvider([configFile(fileId: '8e4e7923-d1b1-4175-90f3-0cfa1362ac6e', targetLocation: 'C:\\Jenkins_builds\\sites\\dev\\web.config')]) {}
					}
				}
				stage('Smoke Test') {
					//when { branch 'develop' }
					steps {
						smokeTest("http://localhost:8090/")
					}
				}
				stage('Acceptance test') {
					//when { branch 'develop' }
				    steps {
				        dir('WebApplication.Tests.Acceptance\\bin\\Release')
                        {
                            bat "\"${VSTest}\" \"WebApplication.Tests.Acceptance.dll\" /Logger:trx;LogFileName=Results_${env.BUILD_ID}.trx /Framework:Framework45"
                        }
                        step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
                    }
                }
			}
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
}

void setBuildStatus(String message, String state) {
  step([
      $class: "GitHubCommitStatusSetter",
      reposSource: [$class: "ManuallyEnteredRepositorySource", url: "https://github.com/alanmacgowan/WebApplication"],
      contextSource: [$class: "ManuallyEnteredCommitContextSource", context: "ci/jenkins/build-status"],
      errorHandlers: [[$class: "ChangingBuildStatusErrorHandler", result: "UNSTABLE"]],
      statusResultSource: [ $class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", message: message, state: state]] ]
  ]);
}

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

void smokeTest(String url){
	powershell ("""
		\$result = Invoke-WebRequest $url
		if (\$result.StatusCode -ne 200) {
			Write-Error \"Did not get 200 OK\"
		} else{
			Write-Host \"Successfully connect.\"
		}
	""")
}
