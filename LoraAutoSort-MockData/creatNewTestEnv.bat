@echo off
set "basePath=D:\SoftwareTest"
cd /D "%basePath%"

:: Check if LAB folder exists, then delete it
if exist "%basePath%\LAB" (
    echo Deleting folder LAB...
    rmdir /s /q "%basePath%\LAB"
) else (
    echo Folder LAB does not exist, skipping delete step.
)

:: Check if Targer folder exists, then delete it
if exist "%basePath%\Targer" (
    echo Deleting folder Targer...
    rmdir /s /q "%basePath%\Targer"
) else (
    echo Folder Targer does not exist, skipping delete step.
)

:: Create a new Targer folder
echo Creating new Target folder...
mkdir "%basePath%\Target"

:: Check if _FreshLab folder exists, then copy it to create LAB
if exist "%basePath%\_FreshLab" (
    echo Creating a copy of _FreshLab as LAB...
    xcopy "%basePath%\_FreshLab" "%basePath%\LAB" /e /i
) else (
    echo The folder _FreshLab does not exist. Cannot create LAB.
)

echo Operation completed.
pause
