@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
  if %errorlevel% neq 0 exit /b %errorlevel%
)

set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*
if %errorlevel% neq 0 exit /b %errorlevel%