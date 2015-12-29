function check-exitcode {
  if ($lastexitcode -ne 0) {
    throw "build failed"
  }
}

.\tools\paket.bootstrapper.exe
check-exitcode
.\tools\paket.exe restore
check-exitcode
.\packages\FAKE\tools\FAKE.exe build.fsx @args
check-exitcode
