set-alias fsi "C:\Program Files (x86)\Microsoft SDKs\F#\4.0\Framework\v4.0\Fsi.exe"
rm -r -errorAction Ignore docs/output
$d = mkdir docs/files -errorAction Ignore
fsi --exec --define:RELEASE --define:HELP --define:REFERENCE .\docs\tools\generate.fsx
rm *.svclog

