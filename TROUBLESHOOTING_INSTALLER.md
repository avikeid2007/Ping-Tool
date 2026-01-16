# Troubleshooting: WinUI3 Traditional Installer Build

## Issue: dotnet publish fails for WinUI3 projects

### Symptoms
- MSIX bundle builds successfully
- Traditional installer build fails during `dotnet publish` step
- Errors related to:
  - `GenerateAppxPackageOnBuild`
  - MSBuild targets (Microsoft.WindowsAppSDK.Selfcontained.targets)
  - File access issues in build directories
  - SafeFileHandle or FileStream errors

### Root Cause

WinUI3/Windows App SDK projects are designed primarily for MSIX packaging. When using `dotnet publish`, the project tries to generate MSIX packages by default, which conflicts with traditional installer builds.

### Solution

Disable MSIX packaging when building for traditional installer by setting `WindowsPackageType=None`:

#### In GitHub Actions (build-and-release.yml):

```yaml
- name: Build x64 Release
  run: |
    msbuild ${{ env.PROJECT_PATH }} `
      /p:Configuration=Release `
      /p:Platform=x64 `
      /p:WindowsPackageType=None `
      /p:AppxPackageSigningEnabled=false `
      /p:GenerateAppxPackageOnBuild=false `
      /t:Restore,Build

- name: Publish x64
  run: |
    dotnet publish ${{ env.PROJECT_PATH }} `
      -c Release `
      -r win-x64 `
      --self-contained true `
      /p:Platform=x64 `
      /p:WindowsPackageType=None `
      /p:PublishSingleFile=false `
      /p:PublishReadyToRun=true `
      /p:PublishTrimmed=false `
      -o Output/Publish/win-x64
```

#### In BuildInstaller.ps1:

```powershell
# Build first
msbuild $ProjectPath `
    /p:Configuration=$Configuration `
    /p:Platform=$Platform `
    /p:WindowsPackageType=None `
    /p:AppxPackageSigningEnabled=false `
    /p:GenerateAppxPackageOnBuild=false `
    /t:Restore,Build

# Then publish
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
```

### Key MSBuild Properties Explained

| Property | Value | Purpose |
|----------|-------|---------|
| `WindowsPackageType` | `None` | Disables MSIX packaging entirely |
| `AppxPackageSigningEnabled` | `false` | Disables package signing (not needed for traditional installer) |
| `GenerateAppxPackageOnBuild` | `false` | Prevents automatic MSIX generation during build |
| `Platform` | `x64` | Specifies target architecture |
| `PublishSingleFile` | `false` | Creates standard multi-file deployment (required for WinUI3) |
| `PublishReadyToRun` | `true` | Enables ahead-of-time compilation for better performance |
| `PublishTrimmed` | `false` | Disables trimming (can break WinUI3 reflection) |

### Alternative: Project File Configuration

You can also create a separate publish profile or configuration in the project file:

```xml
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|x64' AND '$(BuildingInsideVisualStudio)'!='true'">
  <WindowsPackageType>None</WindowsPackageType>
  <AppxPackageSigningEnabled>false</AppxPackageSigningEnabled>
  <GenerateAppxPackageOnBuild>false</GenerateAppxPackageOnBuild>
</PropertyGroup>
```

However, this approach is **not recommended** because it affects all non-Visual Studio builds.

### Common Errors and Solutions

#### Error: "The GenerateAppxPackageOnBuild task failed unexpectedly"

**Cause**: Project is trying to create MSIX package during publish

**Solution**: Add `/p:WindowsPackageType=None` to build/publish commands

#### Error: "Could not find a part of the path '...\Microsoft.UI.Xaml\content\'"

**Cause**: MSIX packaging trying to access files that don't exist in publish context

**Solution**: Build project with MSBuild first, then publish with `WindowsPackageType=None`

#### Error: "System.IO.DirectoryNotFoundException" or "FileStream errors"

**Cause**: Conflicting file operations between MSIX packaging and traditional publish

**Solution**: Ensure `WindowsPackageType=None` is set in **both** build and publish steps

### Verification

After applying the fix, you should see:

? **Successful build** of x64 Release configuration  
? **Successful publish** to `Output/Publish/win-x64`  
? **No MSIX-related errors** in build log  
? **Installer created** successfully  

### Testing Locally

Test the fix locally before pushing to GitHub:

```powershell
# Run the updated script
.\BuildInstaller.ps1

# Verify output
Get-ChildItem Output\Publish\win-x64
Get-ChildItem Output\Installer
```

Expected output structure:
```
Output/
??? Publish/
?   ??? win-x64/
?       ??? PingTool.WinUI3.exe
?       ??? Microsoft.ui.xaml.dll
?       ??? WinRT.Runtime.dll
?       ??? [other dependencies]
??? Installer/
    ??? PingLegacy-2.0.4.0-Setup.exe
```

### Why MSBuild + dotnet publish?

WinUI3 projects require a two-step process:

1. **MSBuild**: Properly resolves WinUI3/Windows App SDK dependencies
2. **dotnet publish**: Creates self-contained deployment

Using `dotnet publish` alone doesn't properly handle WinUI3 build targets.

### Related Documentation

- [Windows App SDK deployment](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-overview)
- [.NET application publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
- [MSBuild properties](https://learn.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-properties)

---

**Issue Resolved**: January 2026  
**Solution**: Set `WindowsPackageType=None` for traditional installer builds  
**Affected Files**: `.github/workflows/build-and-release.yml`, `BuildInstaller.ps1`
