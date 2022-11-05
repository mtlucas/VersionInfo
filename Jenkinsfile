pipeline {
  agent any
  stages {
    stage('Build, Pack and Push') {
      steps {
        sh '''export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export PATH=$PATH:/var/jenkins
_home/workspace/VersionInfo_main/app/.nuke/temp/dotnet-unix/
:; chmod +x ./app/build.cmd
:; chmod +x ./app/build.sh
./app/build.sh -root ./app -Target Push -BuildVersion 1.0.0 -NugetApiKey 63d803c3-e0be-4120-8fc7-09a41d86b0bc -NugetApiUrl https://nuget.lucasnet.int/'''
      }
    }

  }
}