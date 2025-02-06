@echo off
echo Starting cleanup of 'package_info.cs' files...

FOR /R . %%f IN (package_info.cs) DO (
    IF EXIST "%%f" (
        echo Removing: %%f
        del /F /Q "%%f"
    )
)

echo Cleanup complete.
pause