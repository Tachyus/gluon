# creates a new tag
Param([Parameter(Mandatory=$True)] [string] $version)

$v1 = [System.Version]::Parse($version)
$v2 = New-Object -TypeName System.Version -ArgumentList $v1.Major,$v1.Minor,($v1.Build + 1)
$rj = cat meta.json
$j  = ConvertFrom-Json "$rj"
$j.version = "$v2"

function Write-Simple-File($path, $contents) {
    $enc = New-Object System.Text.UTF8Encoding($False)
    $contents = "$contents"
    $path = Resolve-Path $path
    $path = "$path"
    echo "writing $path to $contents"
    [System.IO.File]::WriteAllText($path, $contents, $enc)
}

echo "writing meta.json with version = $v2"
Write-Simple-File -path meta.json -contents (ConvertTo-Json $j)

echo "committing locally"
git add meta.json
git commit -m "tagging $v1 with tag.ps1"

echo "tagging locally as $v1"
git tag $v1

echo "now you can push it all out:"
echo "  git push"
echo "  git push --tags"
