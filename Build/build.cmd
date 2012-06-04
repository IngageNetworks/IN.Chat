@echo Off

rd artifacts /S /Q
md artifacts
md artifacts\bin

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild ..\IN.Chat.sln /p:Configuration=release;Platform=x86 /p:OutDir=..\Build\artifacts\bin\ /verbosity:m /flp:logfile=artifacts\msbuild.log;verbosity:m
::pause