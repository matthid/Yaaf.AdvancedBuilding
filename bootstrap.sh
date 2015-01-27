#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  MONO=""
  DEFINE="WIN64"
  FSI="C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe"
else
  # use mono
  command -v mono >/dev/null 2>&1 || { echo >&2 "Please install mono dependency."; exit 1; }
  myMono="mono --debug --runtime=v4.0"
  FSI="fsharpi"

  MONO="$myMono"
  DEFINE="MONO"
fi

nuget_packages="packages"
paket_packages="packages"

nuget_path="NuGet.CommandLine/tools/NuGet.exe"
fake_path="FAKE/tools/FAKE.exe"

fake=$paket_packages/$fake_path

echo "restoring with paket"
$MONO .paket/paket.bootstrapper.exe
$MONO .paket/paket.exe restore

echo "starting fake for bootstrap."
$MONO $fake "Build" --fsiargs -d:$DEFINE bootstrap.fsx