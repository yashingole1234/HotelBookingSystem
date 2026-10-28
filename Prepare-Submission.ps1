# ============================================================================
# Submission Packaging Script for Pure Technology .NET Intern Task
# ============================================================================

$root = "C:\Users\ingol\source\repos\HotelBookingSystem"
$tempDir = Join-Path $env:TEMP "HotelBookingSystem_Package"
$submissionFolder = Join-Path $tempDir "HotelBookingSystem"
$zipOutput = Join-Path $root "HotelBookingSystem_Submission.zip"

Write-Host "Creating submission directory structure..." -ForegroundColor Cyan

if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path $zipOutput) {
    Remove-Item -Path $zipOutput -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Path (Join-Path $submissionFolder "src") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $submissionFolder "database") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $submissionFolder "screenshots") -Force | Out-Null

# 1. Copy src/ (solution + project without bin/obj)
Write-Host "Copying source code (excluding bin and obj)..." -ForegroundColor Yellow
Copy-Item (Join-Path $root "HotelBookingSystem.sln") -Destination (Join-Path $submissionFolder "src") -Force

$destProject = Join-Path $submissionFolder "src\HotelBookingSystem"
New-Item -ItemType Directory -Path $destProject -Force | Out-Null

# Robocopy to exclude bin and obj cleanly
& robocopy (Join-Path $root "HotelBookingSystem") $destProject /E /XD bin obj /NFL /NDL /NJH /NJS /nc /ns /np

# 2. Copy database files
Write-Host "Copying database artifacts..." -ForegroundColor Yellow
Copy-Item (Join-Path $root "database\*") -Destination (Join-Path $submissionFolder "database") -Recurse -Force

# 3. Copy screenshots
Write-Host "Copying screenshots..." -ForegroundColor Yellow
Copy-Item (Join-Path $root "screenshots\*") -Destination (Join-Path $submissionFolder "screenshots") -Recurse -Force

# 4. Copy Markdown deliverables
Write-Host "Copying README.md and AI_TOOLS_USED.md..." -ForegroundColor Yellow
Copy-Item (Join-Path $root "README.md") -Destination $submissionFolder -Force
Copy-Item (Join-Path $root "AI_TOOLS_USED.md") -Destination $submissionFolder -Force

# 5. Create ZIP Archive
Write-Host "Compressing into HotelBookingSystem_Submission.zip..." -ForegroundColor Green
Compress-Archive -Path $submissionFolder -DestinationPath $zipOutput -CompressionLevel Optimal

# Cleanup temp
Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue

$zipItem = Get-Item $zipOutput
$zipSizeMb = [math]::Round($zipItem.Length / 1MB, 2)
Write-Host "SUCCESS! Submission ZIP created:" -ForegroundColor Green
Write-Host "Location: $zipOutput" -ForegroundColor White
Write-Host "File Size: $zipSizeMb MB" -ForegroundColor White
