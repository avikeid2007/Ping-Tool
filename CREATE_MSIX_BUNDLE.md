# Creating MSIX Bundle for Microsoft Store Submission

This guide explains how to create an `.msixbundle` file for your WinUI3 application to submit to the Microsoft Store.

## Overview

When migrating from UWP to WinUI3 (Windows App SDK), the Microsoft Store requires you to continue submitting `.msixbundle` files if your previous submissions used bundles. This document explains how to create these bundles for your Ping Tool application.

## Prerequisites

- Visual Studio 2022 with Windows App SDK development workload
- Windows SDK (10.0.26100.0 or later)
- .NET 8 SDK
- Valid package certificate for signing (if required)
- Microsoft Partner Center account (for Store submission)

## Project Configuration

The `PingTool.WinUI3.csproj` file has been configured with the following properties to support bundle generation:

```xml
<PropertyGroup>
  <Platforms>x86;x64;ARM64</Platforms>
  <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
  <AppxBundle>Always</AppxBundle>
  <AppxBundlePlatforms>x86|x64|ARM64</AppxBundlePlatforms>
  <UapAppxPackageBuildMode>StoreUpload</UapAppxPackageBuildMode>
  <WindowsPackageType>MSIX</WindowsPackageType>
  <EnableMsixTooling>true</EnableMsixTooling>
</PropertyGroup>
```

### Key Properties Explained

- **`AppxBundle=Always`**: Forces bundle creation for every build
- **`AppxBundlePlatforms`**: Specifies which architectures to include (x86, x64, ARM64)
- **`UapAppxPackageBuildMode=StoreUpload`**: Configures build for Store submission
- **`WindowsPackageType=MSIX`**: Specifies MSIX packaging format

## Method 1: Using PowerShell Script (Recommended)

### Script Location

The `BuildMsixBundle.ps1` script is located in the repository root directory.

### How to Use

1. **Open PowerShell** in the repository root directory:
   ```powershell
   cd "C:\Users\User\source\repos\avikeid2007\Ping-Tool"
   ```

2. **Run the script**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\BuildMsixBundle.ps1
   ```

3. **Wait for completion**. The script will:
   - Clean previous build outputs
   - Build MSIX packages for x86, x64, and ARM64
   - Create a combined `.msixbundle` file containing all architectures
   - Display the bundle location and size

### Script Output

Upon successful completion, you'll see:

```
================================================
MSIX Bundle created successfully!
================================================

Bundle location:
  PingTool.WinUI3\bin\Package\PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle

Bundle size: 150.2 MB

Upload this .msixbundle file to Microsoft Store Partner Center
```

### Output Location

The generated bundle will be located at:
```
PingTool.WinUI3\bin\Package\PingTool.WinUI3_<version>_x86_x64_ARM64.msixbundle
```

## Method 2: Using Visual Studio UI

### Steps

1. **Open the solution** in Visual Studio 2022

2. **Right-click** on the `PingTool.WinUI3` project in Solution Explorer

3. Select **Publish** ? **Create App Packages...**

4. Choose **Microsoft Store using existing app name**

5. **Sign in** with your Microsoft Partner Center account

6. **Select your app** (Ping Legacy) from the list

7. On the **Select and Configure Packages** screen:
   - ? Check **x86**
   - ? Check **x64**
   - ? Check **ARM64**
   - Set Configuration to **Release**
   - Ensure "Create App Bundle" is set to **Always**

8. Click **Create**

9. The wizard will generate the bundle in:
   ```
   PingTool.WinUI3\AppPackages\
   ```

## Method 3: Using MSBuild Command Line

### Build Individual Platforms

```powershell
# Build x86
msbuild PingTool.WinUI3\PingTool.WinUI3.csproj /p:Configuration=Release /p:Platform=x86 /t:Restore,Build,_GenerateAppxPackage

# Build x64
msbuild PingTool.WinUI3\PingTool.WinUI3.csproj /p:Configuration=Release /p:Platform=x64 /t:Restore,Build,_GenerateAppxPackage

# Build ARM64
msbuild PingTool.WinUI3\PingTool.WinUI3.csproj /p:Configuration=Release /p:Platform=ARM64 /t:Restore,Build,_GenerateAppxPackage
```

### Create Bundle

After building all platforms, the bundle is automatically created when all platforms are built together.

## Understanding the Bundle Structure

An `.msixbundle` file contains:

```
PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle
??? PingTool.WinUI3_2.0.4.0_x86.msix      (Windows on x86 devices)
??? PingTool.WinUI3_2.0.4.0_x64.msix      (Windows on x64 devices)
??? PingTool.WinUI3_2.0.4.0_ARM64.msix    (Windows on ARM64 devices)
```

When users install your app from the Microsoft Store, Windows automatically selects and installs the appropriate architecture for their device.

## Submitting to Microsoft Store

### Upload Process

1. **Navigate to** [Microsoft Partner Center](https://partner.microsoft.com)

2. Go to **Apps and games** ? **Ping Legacy**

3. Click **Start update** (or create new submission)

4. Navigate to **Packages**

5. **Drag and drop** the `.msixbundle` file or browse to select it:
   ```
   PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle
   ```

6. Wait for validation to complete

7. Complete other submission requirements (description, screenshots, etc.)

8. Click **Submit to the Store**

### Store Requirements

- ? Bundle must contain all three architectures (x86, x64, ARM64)
- ? Version number must be higher than previous submission
- ? Package identity must match your app's identity
- ? All packages in bundle must be signed with the same certificate

## Troubleshooting

### Issue: "Only individual MSIX files created, no bundle"

**Solution**: Use the `BuildMsixBundle.ps1` script which properly builds all platforms and creates the bundle.

### Issue: "MakeAppx.exe not found"

**Solution**: Ensure Windows SDK is installed. The script automatically locates the tool from:
- `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\makeappx.exe`
- NuGet packages: `%USERPROFILE%\.nuget\packages\microsoft.windows.sdk.buildtools`

### Issue: "Previous submission used bundle, but current doesn't"

**Solution**: This is exactly the error this guide addresses. Always use Method 1 (PowerShell script) or Method 2 (Visual Studio wizard with all platforms selected).

### Issue: "Bundle validation failed in Partner Center"

**Possible causes**:
- Missing architecture in bundle
- Version number not incremented
- Certificate mismatch
- Package identity mismatch

**Solution**: Verify bundle contents and ensure all three architectures are included:
```powershell
# Verify bundle contents
& "C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\makeappx.exe" unbundle /p PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle /d temp_extract /v
```

## Version Management

Update the version in `Package.appxmanifest` before creating a new bundle:

```xml
<Identity
  Name="34488AvnishKumar.PingLegacy"
  Publisher="CN=1494B4FB-4349-4083-8AFA-16D2A64B8D99"
  Version="2.0.5.0" />
```

**Important**: Microsoft Store requires each new submission to have a higher version number than the previous one.

## Build Artifacts

After running the script, the following files are generated:

```
PingTool.WinUI3\bin\Package\
??? PingTool.WinUI3_2.0.4.0_x86_Test\
?   ??? PingTool.WinUI3_2.0.4.0_x86.msix
?   ??? PingTool.WinUI3_2.0.4.0_x86.msixsym
??? PingTool.WinUI3_2.0.4.0_x64_Test\
?   ??? PingTool.WinUI3_2.0.4.0_x64.msix
?   ??? PingTool.WinUI3_2.0.4.0_x64.msixsym
??? PingTool.WinUI3_2.0.4.0_ARM64_Test\
?   ??? PingTool.WinUI3_2.0.4.0_ARM64.msix
?   ??? PingTool.WinUI3_2.0.4.0_ARM64.msixsym
??? BundleMap.txt
??? PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle  ? Upload this file
```

## Best Practices

1. **Always test locally** before submitting to the Store
2. **Increment version numbers** for each submission
3. **Keep certificates secure** and backed up
4. **Test on multiple architectures** if possible
5. **Document changes** in release notes

## Additional Resources

- [Windows App SDK Documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [MSIX Packaging Documentation](https://learn.microsoft.com/en-us/windows/msix/)
- [Microsoft Store Submission Guide](https://learn.microsoft.com/en-us/windows/uwp/publish/)
- [Package Identity and Bundle Requirements](https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps)

## Support

For issues related to:
- **Script**: Check the repository issues or create a new one
- **Microsoft Store**: Contact Partner Center support
- **Windows App SDK**: Visit the [Windows App SDK GitHub](https://github.com/microsoft/WindowsAppSDK)

---

**Last Updated**: January 2026  
**Script Version**: 1.0  
**Compatible With**: Windows App SDK 1.5+, .NET 8
