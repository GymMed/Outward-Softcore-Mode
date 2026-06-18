@echo off
setlocal enabledelayedexpansion

:: Source paths
set "sourceDll=C:\Users\pc\source\repos\OutwardSoftcoreMode\OutwardSoftcoreMode\Release\OutwardSoftcoreMode.dll"

:: Profiles array (quoted entries for readability)
set profiles="Main" "Development"

:: Base destination folder
set "baseProfilePath=F:\r2modmanPlus-local\OutwardDe\profiles"

:: --- Copy DLL into each profile ---
if exist "%sourceDll%" (
    for %%p in (%profiles%) do (
        set "destinationDll=%baseProfilePath%\%%~p\BepInEx\plugins\gymmed-OutwardSoftcoreMode"
        echo Copying "%sourceDll%" to "!destinationDll!"
        if not exist "!destinationDll!" mkdir "!destinationDll!"
        copy /Y "%sourceDll%" "!destinationDll!"
    )
) else (
    echo Source dll file does not exist!
)

pause
