@echo off
cls
if not exist ".\.nuget" mkdir ".\.nuget"
if not exist ".\.nuget\nuget.exe" powershell -Command "Invoke-WebRequest https://www.nuget.org/nuget.exe -OutFile .\.nuget\nuget.exe"
".\.nuget\nuget.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
".\.nuget\nuget.exe" "Install" "NUnit.Runners" "-OutputDirectory" "packages"
"packages\FAKE\tools\Fake.exe" build.fsx %*
pause
