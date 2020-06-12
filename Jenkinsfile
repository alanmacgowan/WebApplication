
pipeline {
			agent any
			environment{
                VSTest = tool 'vstest'	
				MSBuild = tool 'msbuild'
				Nuget = 'C:\\Program Files (x86)\\Jenkins\\nuget.exe'
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
				stage('Build') {
					//when { branch 'develop' }
					steps {
					    bat "\"${MSBuild}\" WebApplication.sln /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:SkipInvalidConfigurations=true /t:build /p:Configuration=Release /p:Platform=\"Any CPU\" /p:DeleteExistingFiles=True /p:publishUrl=c:\\Jenkins_builds\\sites\\dev"
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
				stage('Package') {
					//when { branch 'develop' }
					steps {
					   script{
							zip archive: true, dir: 'c:\\Jenkins_builds\\sites\\dev', glob: '', zipFile: "c:\\Jenkins_builds\\files\\webapp_${env.BUILD_ID}.zip"
						}
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
