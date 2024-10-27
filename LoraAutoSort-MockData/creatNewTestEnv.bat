@echo off

:: Check if LAB folder exists, then delete it
if exist "LAB" (
    echo Deleting folder LAB...
    rmdir /s /q "LAB"
)

:: Check if Targer folder exists, then delete it
if exist "Target" (
    echo Deleting folder Target...
    rmdir /s /q "Target"
)

:: Check if _FreshLab folder exists, then copy it to create LAB
if exist "_FreshLab" (
    echo Creating a copy of _FreshLab as LAB...
    xcopy "_FreshLab" "LAB" /e /i
) else (
    echo The folder _FreshLab does not exist. Cannot create LAB.
)

:: Create a new Targer folder
echo Creating new Targer folder...
mkdir "%basePath%\Targer"


echo Operation completed.
pause
