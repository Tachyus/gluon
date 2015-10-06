$env:PATH = "$env:PATH;${env:ProgramFiles(x86)}\MSBuild\14.0\bin"
msbuild.exe Gluon.sln /p:Configuration=Release /v:m
