set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework\v4.0.30319.

rem go to current folder
cd %~dp0

msbuild write_VersionInfo.proj
msbuild build_output.proj /property:BuildDir=..\Output /property:NugetFolder=..\NugetOutput