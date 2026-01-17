# PowerShell script to create a GitHub release

param(
    [Parameter(Mandatory=$true)]
    [string]$Version  # e.g., "2.0.4.0"
)

$TagName = "v$Version"
$Message = "Release version $Version"

Write-Host "Creating release for version $Version..." -ForegroundColor Green

# Check if there are uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Host "Error: You have uncommitted changes. Please commit or stash them first." -ForegroundColor Red
    git status
    exit 1
}

# Check if tag already exists
$existingTag = git tag -l $TagName
if ($existingTag) {
    Write-Host "Error: Tag $TagName already exists!" -ForegroundColor Red
    exit 1
}

# Get current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "Current branch: $currentBranch" -ForegroundColor Cyan

# Confirm
$confirm = Read-Host "Create and push tag $TagName from branch $currentBranch? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Cancelled" -ForegroundColor Yellow
    exit 0
}

# Create annotated tag
Write-Host "`nCreating tag $TagName..." -ForegroundColor Cyan
git tag -a $TagName -m $Message

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create tag" -ForegroundColor Red
    exit 1
}

# Push tag to origin
Write-Host "Pushing tag to origin..." -ForegroundColor Cyan
git push origin $TagName

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n================================================" -ForegroundColor Green
    Write-Host "Success! Tag $TagName pushed to GitHub" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "`nGitHub Actions will now:" -ForegroundColor Cyan
    Write-Host "  1. Build MSIX bundle" -ForegroundColor White
    Write-Host "  2. Build traditional installer" -ForegroundColor White
    Write-Host "  3. Create GitHub Release" -ForegroundColor White
    Write-Host "  4. Upload both packages" -ForegroundColor White
    Write-Host "`nMonitor progress at:" -ForegroundColor Cyan
    Write-Host "  https://github.com/avikeid2007/Ping-Tool/actions" -ForegroundColor Yellow
    Write-Host "`nRelease will be available at:" -ForegroundColor Cyan
    Write-Host "  https://github.com/avikeid2007/Ping-Tool/releases/tag/$TagName" -ForegroundColor Yellow
} else {
    Write-Host "`nFailed to push tag" -ForegroundColor Red
    Write-Host "You may need to delete the local tag:" -ForegroundColor Yellow
    Write-Host "  git tag -d $TagName" -ForegroundColor White
    exit 1
}
