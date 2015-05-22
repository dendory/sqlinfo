@echo off
%SYSTEMROOT%\Microsoft.NET\Framework\v3.5\csc.exe /out:%~dp0..\sqlinfo.exe %~dp0sqlinfo.cs
