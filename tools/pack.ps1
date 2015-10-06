. .\tools\lib.ps1
$v = version
paket pack output bin version $v
check
