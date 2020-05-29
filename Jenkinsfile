pipeline {
			agent any
			stages {
				stage('Source'){
					steps{
					    //bitbucketStatusNotify(buildState: 'INPROGRESS')
						checkout([$class: 'GitSCM', branches: [[name: 'develop']], doGenerateSubmoduleConfigurations: false, extensions: [], submoduleCfg: [], userRemoteConfigs: [[credentialsId: '1c95fa66-e00f-462f-b2f2-c1dfd0b7c795', url: 'https://github.com/alanmacgowan/WebApplication.git']]])
					}
				}
				stage('Restore'){
				    steps{
				        bat '"C:\\Program Files (x86)\\Jenkins\\nuget.exe" restore WebApplication.sln'
				    }
				}
				stage('Build') {
					steps {
					    bat "\"${tool 'msbuild'}\" WebApplication.sln /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:SkipInvalidConfigurations=true /t:build /p:Configuration=Release /p:Platform=\"Any CPU\" /p:DeleteExistingFiles=True /p:publishUrl=c:\\Jenkins_builds\\sites\\dev"
						}
				}
				stage('Unit test') {
				    steps {
				        dir('WebApplication.Tests\\bin\\Release')
                        {
                            bat "\"${tool 'mstest'}\"  /testcontainer:WebApplication.Tests.dll /resultsfile:Results.trx"
                        }
                        // step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: true, keepLongStdio: true])
					}
                }
				stage('Package') {
					steps {
					   script{
							zip archive: true, dir: 'c:\\Jenkins_builds\\sites\\dev', glob: '', zipFile: "c:\\Jenkins_builds\\files\\webapp_${env.BUILD_ID}.zip"
						}
					}
				}
			}
			//post { 
            //    failure { 
            //        bitbucketStatusNotify(buildState: 'FAILED')
            //    }
            //    success{
            //        bitbucketStatusNotify(buildState: 'SUCCESSFUL')
            //    }
			//	always {
            //        jiraSendBuildInfo site: 'alanmacgowan.atlassian.net'
            //    }
			//}
}