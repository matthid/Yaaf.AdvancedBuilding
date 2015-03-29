@echo off
set nuget_packages=packages
set paket_packages=packages

set nuget_path=NuGet.CommandLine/tools/NuGet.exe
set fake_path=FAKE/tools/FAKE.exe

set fake=%paket_packages%/%fake_path%


echo restoring with paket
.paket\paket.bootstrapper.exe
.paket\paket.exe restore

echo starting fake for bootstrap.
"%fake%" "Build" --fsiargs -d:WIN64 bootstrap.fsx
