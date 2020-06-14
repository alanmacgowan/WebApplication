
pipeline {
			agent any
			environment{
                VSTest = tool 'vstest'	
				MSBuild = tool 'msbuild'
				Nuget = 'C:\\Program Files (x86)\\Jenkins\\nuget.exe'
				MSDeploy = "C:\\Program Files (x86)\\IIS\\Microsoft Web Deploy V3\\msdeploy.exe"
            }
			stages {
				//stage('Source'){
				//	when { branch 'develop' }
				//	steps{
				//	    bitbucketStatusNotify(buildState: 'INPROGRESS')
				//		checkout([$class: 'GitSCM', branches: [[name: 'develop']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[credentialsId: '1c95fa66-e00f-462f-b2f2-c1dfd0b7c795', url: 'https://alanmacgowan@bitbucket.org/alanmacgowan/web.git']]])
				//	}
				//}
				stage('Restore'){
					//when { branch 'develop' }
				    steps{
						bitbucketStatusNotify(buildState: 'INPROGRESS')
				        bat "\"${Nuget}\" restore WebApplication.sln"
				    }
				}
				stage('NodeJS'){
					steps{
                        nodejs(nodeJSInstallationName: 'Node') {
                            bat 'cd "WebApplication" && npm install && npm run build:prod'
                        }
					}
				}
				stage('Build & Package') {
					//when { branch 'develop' }
					steps {
					    bat "\"${MSBuild}\" WebApplication.sln /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /t:build /p:Configuration=Production /p:Platform=\"Any CPU\" /p:DesktopBuildPackageLocation=c:\\Jenkins_builds\\files\\WebApp_${env.BUILD_ID}.zip /p:DeployIisAppPath=\"TestAppDev\""
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
					    bat "\"${MSDeploy}\" -source:package=\"c:\\Jenkins_builds\\files\\WebApp_${env.BUILD_ID}.zip\"  -verb:sync -dest:auto -allowUntrusted=true "
						}
				}
				stage('Smoke Test') {
					//when { branch 'develop' }
					steps {
							powershell ("""
								$result = Invoke-WebRequest http://localhost:8090/
								if ($result.StatusCode -ne 200) {
								  Write-Error "Did not get 200 OK"
								}
							""")
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
                    bitbucketStatusNotify(buildState: 'FAILED')
                }
                success{
                   bitbucketStatusNotify(buildState: 'SUCCESSFUL')
                }
				always {
                    jiraSendBuildInfo site: 'alanmacgowan.atlassian.net'
                }
			}
}
