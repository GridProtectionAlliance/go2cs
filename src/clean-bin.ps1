# Clean-Bin.ps1
# Script to remove all bin, obj and Generated folders from the current directory and subdirectories

# Set the current location as the root directory for the search
$rootDirectory = Get-Location

# Count variables to track progress
$totalFoldersFound = 0
$totalFoldersRemoved = 0
$totalErrors = 0

Write-Host "Searching for bin, obj and Generated folders in: $rootDirectory" -ForegroundColor Cyan

# Get all bin and obj directories
$foldersToDelete = Get-ChildItem -Path $rootDirectory -Include "bin", "obj", "Generated" -Directory -Recurse -ErrorAction SilentlyContinue

$totalFoldersFound = $foldersToDelete.Count

if ($totalFoldersFound -eq 0) {
    Write-Host "No bin or obj folders found." -ForegroundColor Green
    exit
}

Write-Host "Found $totalFoldersFound folders to delete." -ForegroundColor Yellow

# Ask for confirmation
$confirmation = Read-Host "Do you want to proceed with deletion? (Y/N)"
if ($confirmation -ne "Y") {
    Write-Host "Operation canceled." -ForegroundColor Red
    exit
}

# Process each folder
foreach ($folder in $foldersToDelete) {
    try {
        $relativePath = $folder.FullName.Substring($rootDirectory.Path.Length + 1)
        Write-Host "Removing: $relativePath" -ForegroundColor Yellow -NoNewline
        
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        
        Write-Host " - Success" -ForegroundColor Green
        $totalFoldersRemoved++
    }
    catch {
        Write-Host " - Failed: $($_.Exception.Message)" -ForegroundColor Red
        $totalErrors++
    }
}

# Display summary
Write-Host "`nCleanup Summary:" -ForegroundColor Cyan
Write-Host "Folders found: $totalFoldersFound" -ForegroundColor White
Write-Host "Folders removed: $totalFoldersRemoved" -ForegroundColor Green
if ($totalErrors -gt 0) {
    Write-Host "Folders failed: $totalErrors" -ForegroundColor Red
}

Write-Host "`nCleanup completed!" -ForegroundColor Cyan