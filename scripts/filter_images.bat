@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

:: Display help if no arguments or --help is used
if "%~1"=="" goto :show_help
if "%~1"=="--help" goto :show_help

:: Validate the first argument
if /i not "%~1"=="--folder" (
    echo Error: The first argument must be --folder
    goto :end
)

:: Get the target folder
set "target_folder=%~2"

if not exist "%target_folder%" (
    echo Error: The specified folder does not exist: %target_folder%
    goto :end
)

:: Validate the --hour or --day flag
if /i "%~3"=="--hour" (
    set "mode=hour"
) else if /i "%~3"=="--day" (
    set "mode=day"
) else (
    echo Error: The flag must be --hour or --day
    goto :end
)

:: Change to the target directory
pushd "%target_folder%" || (
    echo Error: Could not access the folder: %target_folder%
    goto :end
)

:: First pass: Delete files that do not contain "_12-"
if "%mode%"=="day" (
    echo Deleting files that do not contain "_12-"...
    for %%F in (*.jpg) do (
        echo %%F | findstr /i "_12-" >nul
        if errorlevel 1 (
            echo Deleting file: %%F
            del "%%F"
        )
    )

    :: Create a list of unique days
    set "days="
    for %%F in (*_12-*.jpg) do (
        set "filename=%%~nF"
        for /f "tokens=1 delims=_" %%A in ("%%~nF") do (
            echo !days! | find "%%A" >nul || set "days=!days! %%A"
        )
    )

    :: Second pass: For each day, select the file with the lowest minutes
    for %%D in (!days!) do (
        set "best_file="
        set "lowest_minutes=99"

        for %%F in (*_12-*.jpg) do (
            set "filename=%%~nF"
            for /f "tokens=1,2 delims=_" %%A in ("%%~nF") do (
                set "file_date=%%A"
                set "file_time=%%B"
            )
            for /f "tokens=2 delims=-" %%M in ("!file_time!") do (
                set /a "minutes=1%%M - 100"  :: Remove leading zeros
            )

            if "!file_date!"=="%%D" if !minutes! lss !lowest_minutes! (
                set "lowest_minutes=!minutes!"
                set "best_file=%%F"
            )
        )

        :: Delete all files of the day except the selected one
        for %%F in (*_12-*.jpg) do (
            set "filename=%%~nF"
            for /f "tokens=1 delims=_" %%A in ("%%~nF") do (
                set "file_date=%%A"
            )

            if "!file_date!"=="%%D" (
                if "%%F"=="!best_file!" (
                    echo Keeping file: %%F
                ) else (
                    echo Deleting file: %%F
                    del "%%F"
                )
            )
        )
    )
)

:: Process files for --hour (no changes)
if "%mode%"=="hour" (
    set "current_hour="
    for %%F in (*.jpg) do (
        set "filename=%%~nF"
        for /f "tokens=2 delims=_" %%A in ("%%~nF") do (
            set "file_hour=%%A"
            set "file_hour=!file_hour:~0,2!"
        )

        if "!current_hour!" neq "!file_hour!" (
            set "current_hour=!file_hour!"
            echo Keeping file: %%F
        ) else (
            echo Deleting file: %%F
            del "%%F"
        )
    )
)

:: Return to the previous directory
popd
echo Process completed.
goto :end

:show_help
echo Usage: filter_images.bat --folder "RelativePathToFolder" [--hour ^| --day]
echo.
echo Flags:
echo   --folder: Specifies the relative path to the folder containing the images
echo   --hour:   Keeps the first image of each hour
echo   --day:    Keeps the daily image containing "_12-" with the lowest minutes
echo   --help:   Displays this help information
goto :end

:end
endlocal
exit /b
