# Get all directories in the current location
$directories = Get-ChildItem -Directory

# Loop through each directory
foreach ($dir in $directories) {
    # Skip the BehavioralTests directory
    if ($dir.Name -eq "BehavioralTests") {
        Write-Host "Skipping BehavioralTests directory" -ForegroundColor Yellow
        continue
    }
	
    Write-Host "Processing directory: $($dir.FullName)"
	
    # Change to the directory
    Push-Location $dir.FullName
    
    try {
		del go.mod
		del *.csproj
		
        # Run go mod init with the directory name as the module name
        # You can modify this line if you want a different naming convention
        $moduleName = "go2cs/$($dir.Name)"
        Write-Host "Running: go mod init $moduleName"
        go mod init $moduleName
        
        # Check if the command was successful
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully initialized Go module in $($dir.Name)" -ForegroundColor Green
        } else {
            Write-Host "Failed to initialize Go module in $($dir.Name)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Error occurred: $_" -ForegroundColor Red
    }
    finally {
        # Return to the original directory
        Pop-Location
    }
}

Write-Host "All directories processed." -ForegroundColor Cyan