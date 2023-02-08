$fridaVersion="16.0.8"
$fridaFile="frida-core-devkit-$fridaVersion-windows-x86_64.exe"
$url = "https://github.com/frida/frida/releases/download/$fridaVersion/$fridaFile"
$fridaLibDir = "FridaDevKit\lib\x64"
$output = "$PSScriptRoot\$fridaLibDir\$fridaFile"

if (Test-Path -Path $PSScriptRoot\$fridaLibDir) {
} else {
mkdir $PSScriptRoot\$fridaLibDir
}

if (Test-Path -Path $output) {
} else {
echo "Downloading frida release $url"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri $url -OutFile $output
}

echo "Extracting frida release"
pushd
cd $fridaLibDir
Invoke-Expression $output
popd
echo "Finish!"
[Console]::ReadKey('');