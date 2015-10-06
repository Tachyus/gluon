. .\tools\lib.ps1

$v = version
echo "Versioning as $v"
Update-AppveyorBuild -Version $v

echo 'Restoring Paket'
.\tools\paket.bootstrapper.exe
check
paket install
check

echo 'Building CLR code'
.\tools\clr-build.ps1
check

echo 'Packaging artifacts'
.\tools\pack.ps1
check