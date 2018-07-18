@echo off
echo =======================================================
echo 说明：本示例示范从命令行调用Unity自动化构建。
echo       C#源码参考BuildFromCLI.cs
echo.
echo 注意1：修改本文件的相关路径：
echo        UNITY_PATH：unity.exe的完整路径
echo        PROJECT_PATH：工程根目录的完整路径
echo        LOG_PATH：构建日志完整路径
echo 注意2：执行请先关闭Unity
echo =======================================================

set UNITY_PATH="D:\Program Files (x86)\Unity 2017.4.3f1\Editor\Unity.exe"
set PROJECT_PATH="D:\work\xLua_forsakenyang"
set LOG_PATH="D:\work\xLua_forsakenyang\output\log.txt"

echo start...

rem 确保日志目录存在
for %%a in (%LOG_PATH%) do (
    set log_root=%%~dpa
)    
if not exist %log_root% mkdir %log_root%

%UNITY_PATH% -batchmode -quit -projectPath %PROJECT_PATH% -logFile %LOG_PATH% -executeMethod BuildFromCLI.Build

echo done.
pause