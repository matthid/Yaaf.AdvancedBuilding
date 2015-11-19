@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
  if %ERRORLEVEL% NEQ 0 (
    echo Exited with %ERRORLEVEL%
    exit /b %ERRORLEVEL%
  )
)

set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*