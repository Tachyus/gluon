set-alias paket .\tools\paket.exe

# Checks and fails if the last process exited with a bad code.
function check() {
  if ($LASTEXITCODE -ne 0) {
    echo 'Build failed'
    exit 1
  }
}

# Determines the current software version.
function version() {
  # the version in progress, used by pre-release builds
  $rawJson = cat meta.json
  $meta = ConvertFrom-Json -InputObject "$rawJson"
  $version = $meta.version
  $v = $version
  if ($env:appveyor_build_number) {
    $v = $v + '-b' + [int]::Parse($env:appveyor_build_number).ToString('000')
  }
  if ($env:appveyor_repo_tag -eq 'true'){
    $v = $env:appveyor_repo_tag_name
  }
  return $v
}