pipeline {
  agent any
  stages {
    stage('Build, Pack and Push') {
      steps {
        script {
          VERSION_NUMBER = VersionNumber(versionNumberString: '${BUILD_YEAR}.${BUILD_MONTH}.${BUILD_NUMBER}'
          currentBuild.displayName = "${VERSION_NUMBER}"
          sh '''export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export PATH=$PATH:/var/jenkins_home/.dotnet/dotnet-unix
:; chmod +x ./app/build.cmd
:; chmod +x ./app/build.sh
./app/build.sh -root ./app -Target Push -BuildVersion ${VERSION_NUMBER} -NugetApiKey 63d803c3-e0be-4120-8fc7-09a41d86b0bc'''
        }
      }
    }
  }
}