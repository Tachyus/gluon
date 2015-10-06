. .\tools\lib.ps1

.\tools\paket.bootstrapper.exe
check

.\tools\paket.exe restore
check

copyXunit
check

.\tools\clr-build.ps1
check
