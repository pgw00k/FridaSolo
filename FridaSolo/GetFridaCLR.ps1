$fridaVersion="16.0.8"
$fridaFile="frida-clr-$fridaVersion-windows-x86_64.dll"
$url = "https://github.com/frida/frida/releases/download/$fridaVersion/$fridaFile.xz"
$output = "$PSScriptRoot\$fridaFile.xz"

if (Test-Path -Path $output) {
} else {
echo "Downloading frida release $url"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri $url -OutFile $output
}

echo "Extracting frida release"
7z x -aos $output

if (Test-Path -Path Frida.dll) {
Remove-Item Frida.dll
}
Rename-Item -NewName Frida.dll -Path $fridaFile -Force

echo "Finish!"
[Console]::ReadKey('');