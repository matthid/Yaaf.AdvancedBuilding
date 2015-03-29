@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
  if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
)

set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*