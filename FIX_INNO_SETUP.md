# Fix: Inno Setup Installer Creation Failure

## Issue

The "Create Installer" step in GitHub Actions was failing with:

```
Compiler line 27 in D:\a\Ping-Tool\Ping-Tool\InnoSetup.iss: 
The system cannot find the file specified.
```

## Root Causes

### 1. **Incorrect Icon File Reference (Line 27)**

**Problem**:
```ini
SetupIconFile=PingTool.WinUI3\Assets\Logo.ico
```

The file `Logo.ico` doesn't exist. The project only has `Logo.png`.

**Solution**: Commented out the line since it's optional:
```ini
; SetupIconFile=PingTool.WinUI3\Assets\Logo.png
```

### 2. **Incorrect Source File Path (Line 60)**

**Problem**:
```ini
Source: "PingTool.WinUI3\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\*"
```

This path is for MSBuild output, not `dotnet publish` output.

**Solution**: Changed to the correct publish output path:
```ini
Source: "Output\Publish\win-x64\*"
```

This matches where the GitHub Actions workflow publishes files:
```yaml
dotnet publish ... -o Output/Publish/win-x64
```

## Changes Made

### `InnoSetup.iss`

**Before**:
```ini
SetupIconFile=PingTool.WinUI3\Assets\Logo.ico  ? Line 27 error
Source: "PingTool.WinUI3\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\*"  ? Wrong path
Source: "PingTool.WinUI3\Assets\*"  ? Removed, included in publish output
```

**After**:
```ini
; SetupIconFile commented out (optional)
Source: "Output\Publish\win-x64\*"  ? Correct publish output path
```

## File Structure

After `dotnet publish`, the files are located at:

```
Output/
??? Publish/
    ??? win-x64/
        ??? PingTool.WinUI3.exe
        ??? PingTool.WinUI3.dll
        ??? Microsoft.UI.Xaml.dll
        ??? WinRT.Runtime.dll
        ??? Assets/          ? Assets are copied here during publish
        ??? [all other dependencies]
```

Inno Setup now correctly packages all files from this directory.

## Verification

### Local Testing

```powershell
# Run the build script
.\BuildInstaller.ps1

# Should complete with:
# ? Build completed
# ? Publish completed
# ? Installer created: Output\Installer\PingLegacy-2.0.4.0-Setup.exe
```

### GitHub Actions

The workflow will now:
1. ? Build x64 Release (without MSIX)
2. ? Publish to `Output/Publish/win-x64`
3. ? Run Inno Setup to create installer
4. ? Upload installer artifact

## Optional: Adding an Icon

If you want to add a setup icon in the future:

1. **Convert Logo.png to Logo.ico**:
   - Use an online converter or tool
   - Recommended sizes: 16x16, 32x32, 48x48, 256x256
   - Save as `PingTool.WinUI3\Assets\Logo.ico`

2. **Uncomment the line**:
   ```ini
   SetupIconFile=PingTool.WinUI3\Assets\Logo.ico
   ```

## Why This Matters

- **Correct paths** ensure Inno Setup finds all application files
- **Self-contained publish** includes all dependencies in one folder
- **Simple source reference** makes maintenance easier
- **No hardcoded version paths** (like `net8.0-windows10.0.19041.0`) prevents future breakage

## Related Files

- `InnoSetup.iss` - Installer configuration (FIXED)
- `.github/workflows/build-and-release.yml` - Publishes to `Output/Publish/win-x64`
- `BuildInstaller.ps1` - Local build script (uses same output path)

---

**Issue Resolved**: January 2026  
**Files Modified**: `InnoSetup.iss`  
**Fix**: Updated source paths to match publish output location
