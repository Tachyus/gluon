mono tools/paket.bootstrapper.exe
exit_code=$?
if [ $exit_code -ne 0 ]; then
	exit $exit_code
fi

mono tools/paket.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
	exit $exit_code
fi

mono packages/build/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx 
