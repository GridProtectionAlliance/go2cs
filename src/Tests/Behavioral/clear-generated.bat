@echo off
echo Starting cleanup of 'generated' folders...

FOR /D /R . %%d IN (Generated) DO (
    IF EXIST "%%d" (
        echo Removing: %%d
        rmdir /S /Q "%%d"
    )
)

echo Cleanup complete.
pause