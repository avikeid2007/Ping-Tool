# PowerShell script to build traditional installer using .NET publish

Write-Host "Building Traditional Installer for Direct Download..." -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green

# Configuration
$ProjectPath = "PingTool.WinUI3\PingTool.WinUI3.csproj"
$Configuration = "Release"
$Platform = "x64"
$RuntimeIdentifier = "win-x64"
$OutputDir = "Output\Publish"

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Cyan
Remove-Item -Path $OutputDir -Recurse -Force -ErrorAction SilentlyContinue

# Build the project first (without MSIX packaging)
Write-Host "`nBuilding project for $Platform..." -ForegroundColor Cyan

msbuild $ProjectPath `
    /p:Configuration=$Configuration `
    /p:Platform=$Platform `
    /p:WindowsPackageType=None `
    /p:AppxPackageSigningEnabled=false `
    /p:GenerateAppxPackageOnBuild=false `
    /t:Restore,Build

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

# Publish the application (self-contained)
Write-Host "`nPublishing self-contained application for $RuntimeIdentifier..." -ForegroundColor Cyan

dotnet publish $ProjectPath `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    /p:Platform=$Platform `
    /p:WindowsPackageType=None `
    /p:PublishSingleFile=false `
    /p:PublishReadyToRun=true `
    /p:PublishTrimmed=false `
    -o "$OutputDir\$RuntimeIdentifier"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n====================================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host "`nPublished files location:" -ForegroundColor Cyan
Write-Host "  $OutputDir\$RuntimeIdentifier" -ForegroundColor Yellow

# Check if Inno Setup is installed
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (Test-Path $InnoSetupPath) {
    Write-Host "`nInno Setup found. Creating installer..." -ForegroundColor Cyan
    
    # Run Inno Setup compiler
    & $InnoSetupPath "InnoSetup.iss"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n====================================================" -ForegroundColor Green
        Write-Host "Installer created successfully!" -ForegroundColor Green
        Write-Host "====================================================" -ForegroundColor Green
        
        $installerPath = "Output\Installer"
        if (Test-Path $installerPath) {
            Write-Host "`nInstaller files:" -ForegroundColor Cyan
            Get-ChildItem -Path $installerPath -Filter "*.exe" | ForEach-Object {
                $size = [math]::Round($_.Length / 1MB, 2)
                Write-Host "  - $($_.Name) ($size MB)" -ForegroundColor Yellow
                Write-Host "    Location: $($_.FullName)" -ForegroundColor DarkGray
            }
        }
    } else {
        Write-Host "`nInno Setup compilation failed!" -ForegroundColor Red
    }
} else {
    Write-Host "`nInno Setup not found at: $InnoSetupPath" -ForegroundColor Yellow
    Write-Host "Download from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "`nYou can manually create the installer by:" -ForegroundColor Cyan
    Write-Host "  1. Install Inno Setup" -ForegroundColor White
    Write-Host "  2. Open InnoSetup.iss" -ForegroundColor White
    Write-Host "  3. Click Build > Compile" -ForegroundColor White
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "  - Test the installer on a clean machine" -ForegroundColor White
Write-Host "  - Upload to GitHub Releases" -ForegroundColor White
Write-Host "  - Update website with download link" -ForegroundColor White
