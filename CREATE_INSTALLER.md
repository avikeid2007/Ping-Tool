# Building Traditional Installer for Direct Download

This guide explains how to create a traditional Windows installer for Ping Legacy that users can download directly from your website or GitHub releases.

## Overview

While the Microsoft Store provides automatic updates and easy installation, some users prefer traditional installers for:
- Offline installation
- Corporate/enterprise deployment
- No Microsoft account required
- Full control over installation location

## Prerequisites

### Required Software

1. **Visual Studio 2022**
   - Windows App SDK development workload
   - .NET 8 SDK

2. **Inno Setup 6.0 or later**
   - Download from: https://jrsoftware.org/isdl.php
   - Install to default location: `C:\Program Files (x86)\Inno Setup 6\`

3. **Optional: Code Signing Certificate**
   - For signing the installer executable
   - Recommended for distribution to avoid Windows SmartScreen warnings

## Building the Installer

### Method 1: Using PowerShell Script (Recommended)

1. **Open PowerShell** in the repository root:
   ```powershell
   cd "C:\Users\User\source\repos\avikeid2007\Ping-Tool"
   ```

2. **Run the build script**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\BuildInstaller.ps1
   ```

3. **Script will**:
   - Publish self-contained x64 version
   - Create installer using Inno Setup
   - Output installer to `Output\Installer\`

### Method 2: Manual Build

#### Step 1: Publish the Application

```powershell
dotnet publish PingTool.WinUI3\PingTool.WinUI3.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -p:PublishReadyToRun=true `
  -o Output/Publish/win-x64
```

#### Step 2: Compile Installer

1. Open Inno Setup Compiler
2. Open `InnoSetup.iss`
3. Click **Build** ? **Compile**
4. Installer will be created in `Output\Installer\`

## Installer Features

### What's Included

- ? Self-contained .NET 8 Desktop Runtime
- ? All application dependencies
- ? Desktop shortcut (optional)
- ? Start Menu shortcuts
- ? Uninstaller
- ? Automatic detection of .NET 8 Runtime
- ? x64 architecture support

### Installer Configuration

The `InnoSetup.iss` file contains all installer settings:

```ini
App Name: Ping Legacy
Version: 2.0.4.0
Publisher: Avnish Kumar
Default Installation: C:\Program Files\Ping Legacy
Compression: LZMA2 (Maximum)
Minimum Windows: Windows 10 1809 (Build 17763)
Architecture: x64
```

## Output Files

After successful build:

```
Output\
??? Installer\
?   ??? PingLegacy-2.0.4.0-Setup.exe  ? Distribute this file
??? Publish\
    ??? win-x64\
        ??? [All application files]
```

### Installer File Details

- **Filename**: `PingLegacy-2.0.4.0-Setup.exe`
- **Size**: ~150-200 MB (includes .NET runtime)
- **Type**: Self-extracting installer
- **Compression**: LZMA2 (high compression)

## Customizing the Installer

### Changing Application Version

Update version in `InnoSetup.iss`:

```ini
#define MyAppVersion "2.0.5.0"
```

### Adding/Removing Features

Edit the `[Tasks]` section in `InnoSetup.iss`:

```ini
[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"
Name: "quicklaunch"; Description: "Create Quick Launch icon"
```

### Custom Installation Options

Modify `[Setup]` section:

```ini
DefaultDirName={autopf}\{#MyAppName}  ; Change default install path
AllowNoIcons=yes                       ; Allow no Start Menu folder
DisableProgramGroupPage=yes            ; Skip program group page
```

### Including Additional Files

Edit `[Files]` section:

```ini
[Files]
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
```

## Code Signing the Installer

### Why Sign?

- Reduces Windows SmartScreen warnings
- Builds user trust
- Required for some enterprise deployments

### How to Sign

1. **Obtain a code signing certificate**
   - DigiCert
   - Sectigo
   - GlobalSign

2. **Add signing to Inno Setup**:

Edit `InnoSetup.iss`:

```ini
[Setup]
SignTool=signtool
SignedUninstaller=yes
```

Create `signtool` configuration:
```ini
SignTool=signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com $f
```

Or sign after build:
```powershell
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com Output\Installer\PingLegacy-2.0.4.0-Setup.exe
```

## Distribution

### GitHub Releases

Upload the installer to GitHub Releases using the automated workflow:

1. **Create a version tag**:
   ```bash
   git tag -a v2.0.4.0 -m "Release version 2.0.4.0"
   git push origin v2.0.4.0
   ```

2. **GitHub Actions will**:
   - Build MSIX bundle
   - Build traditional installer
   - Create GitHub Release
   - Upload both packages

3. **Users can download from**:
   ```
   https://github.com/avikeid2007/Ping-Tool/releases/latest
   ```

### Website Distribution

Host the installer on your website:

```html
<a href="https://yoursite.com/downloads/PingLegacy-2.0.4.0-Setup.exe">
  Download Ping Legacy (Installer)
</a>
```

**Add SHA256 checksum for verification**:

```powershell
# Generate checksum
Get-FileHash Output\Installer\PingLegacy-2.0.4.0-Setup.exe -Algorithm SHA256
```

Display on website:
```
SHA256: [checksum here]
```

## GitHub Actions Integration

The repository includes automated builds via GitHub Actions:

### Workflows

**`.github/workflows/build-and-release.yml`**

Triggers on:
- Push to main/migration branches
- Version tags (v*)
- Manual workflow dispatch

Creates:
- ? MSIX Bundle for Microsoft Store
- ? Traditional installer for direct download
- ? GitHub Release with both packages

### Triggering a Release

#### Automatic (Recommended)

1. Update version in files:
   - `Package.appxmanifest`
   - `InnoSetup.iss`
   - `PingTool.WinUI3.csproj`

2. Commit changes:
   ```bash
   git add .
   git commit -m "Bump version to 2.0.5.0"
   ```

3. Create and push tag:
   ```bash
   git tag -a v2.0.5.0 -m "Release version 2.0.5.0"
   git push origin v2.0.5.0
   ```

4. GitHub Actions will automatically:
   - Build MSIX bundle
   - Build installer
   - Create release
   - Upload artifacts

#### Manual

1. Go to repository on GitHub
2. Click **Actions**
3. Select **Build and Release** workflow
4. Click **Run workflow**
5. Download artifacts when complete

## Testing the Installer

### Pre-Distribution Checklist

- [ ] Test on clean Windows 10 machine
- [ ] Test on Windows 11
- [ ] Verify .NET runtime detection
- [ ] Check desktop shortcut creation
- [ ] Test uninstallation
- [ ] Verify file associations (if any)
- [ ] Check Start Menu shortcuts
- [ ] Test on machine without .NET 8

### Automated Testing

GitHub Actions includes basic installer verification:

```yaml
test-installer:
  name: Test Installer
  needs: [build-installer]
  runs-on: windows-latest
```

For comprehensive testing, use:
- Virtual machines
- Windows Sandbox
- Clean test machines

## Installer vs MSIX Bundle Comparison

| Feature | Traditional Installer | MSIX Bundle |
|---------|----------------------|-------------|
| Installation | Standard .exe | Microsoft Store / Sideload |
| Updates | Manual download | Automatic (Store) |
| Uninstall | Programs & Features | Settings > Apps |
| Offline | ? Yes | ? No |
| Corporate Deployment | ? Easy (Group Policy) | Moderate (InTune) |
| Code Signing | Optional | Required |
| Sandboxing | ? No | ? Yes |
| File Access | Full access | Limited |
| Installation Size | ~150-200 MB | ~50 MB + Runtime |

## Troubleshooting

### Issue: Inno Setup not found

**Error**: `Inno Setup not found at: C:\Program Files (x86)\Inno Setup 6\ISCC.exe`

**Solution**:
1. Download from https://jrsoftware.org/isdl.php
2. Install to default location
3. Or update path in `BuildInstaller.ps1`

### Issue: .NET Runtime detection fails

**Solution**: Update the detection code in `InnoSetup.iss`:

```pascal
function IsDotNet8Installed: Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;
```

### Issue: Large installer size

**Solutions**:
1. Enable ReadyToRun compilation (already enabled)
2. Use framework-dependent deployment (smaller but requires .NET installed)
3. Enable trimming (may break reflection-based code)

### Issue: Windows SmartScreen warning

**Causes**:
- Unsigned installer
- New/unknown publisher

**Solutions**:
1. Sign the installer with code signing certificate
2. Build reputation over time
3. Add instructions for users to bypass warning

## Version Management

### Updating Version Numbers

When releasing a new version, update in these files:

1. **Package.appxmanifest**:
   ```xml
   <Identity Version="2.0.5.0" />
   ```

2. **InnoSetup.iss**:
   ```ini
   #define MyAppVersion "2.0.5.0"
   ```

3. **Create new git tag**:
   ```bash
   git tag -a v2.0.5.0 -m "Release 2.0.5.0"
   ```

## Best Practices

1. ? **Always test on clean machine** before distributing
2. ? **Sign installers** for production releases
3. ? **Provide checksums** for verification
4. ? **Keep installer size reasonable** (under 200 MB)
5. ? **Document system requirements** clearly
6. ? **Provide uninstall instructions**
7. ? **Test upgrade scenarios** from previous versions

## Resources

- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
- [.NET 8 Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
- [Code Signing Best Practices](https://learn.microsoft.com/en-us/windows/security/threat-protection/windows-defender-application-control/use-code-signing-for-better-control-and-protection)
- [GitHub Actions for Windows](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)

## Support

For issues with:
- **Installer build**: Check GitHub Actions logs
- **Installation problems**: Review system requirements
- **Distribution**: Verify checksums and signatures

---

**Last Updated**: January 2026  
**Installer Version**: 2.0.4.0  
**Compatible With**: Windows 10 1809+, Windows 11
