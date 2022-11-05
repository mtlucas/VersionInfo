pipeline {
  agent any
  stages {
    stage('Build, Pack and Push') {
      steps {
        sh ''':; chmod +x ./app/build.cmd
:; chmod +x ./app/build.sh
./app/build.sh -Target Push -BuildVersion 1.0.0 -NugetApiKey 63d803c3-e0be-4120-8fc7-09a41d86b0bc -NugetApiUrl https://nuget.lucasnet.int/'''
      }
    }

  }
}