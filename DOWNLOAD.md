# Ping Legacy - Download & Installation

## Choose Your Installation Method

### ?? Method 1: Microsoft Store (Recommended)

**Benefits:**
- ? Automatic updates
- ? Sandboxed security
- ? Easy installation and management
- ? No manual downloads required

**Get it from Microsoft Store:**

[![Get it from Microsoft Store](https://developer.microsoft.com/store/badges/images/English_get-it-from-MS.png)](https://www.microsoft.com/store/apps/9NBLGGH4VQ4Q)

---

### ?? Method 2: Traditional Installer (Direct Download)

**Benefits:**
- ? Offline installation
- ? No Microsoft account required
- ? Corporate deployment friendly
- ? Full control over installation location

**System Requirements:**
- Windows 10 version 1809 (Build 17763) or later
- Windows 11 (all versions)
- .NET 8 Desktop Runtime (installer will prompt if not installed)
- 250 MB free disk space

**Download:**

Latest version: **2.0.4.0**

[?? Download PingLegacy-2.0.4.0-Setup.exe](https://github.com/avikeid2007/Ping-Tool/releases/latest/download/PingLegacy-2.0.4.0-Setup.exe)

**File Information:**
- Size: ~150 MB
- Type: Windows Installer (.exe)
- Architecture: x64

**Installation Steps:**
1. Download the installer
2. Run `PingLegacy-2.0.4.0-Setup.exe`
3. Follow the installation wizard
4. Launch Ping Legacy from Start Menu or Desktop

**Verification (Optional):**

To verify the installer integrity, check the SHA256 hash:

```powershell
Get-FileHash PingLegacy-2.0.4.0-Setup.exe -Algorithm SHA256
```

Expected hash: `[Will be added after build]`

---

### ?? Method 3: MSIX Bundle (Advanced Users)

**For IT Administrators or Sideloading:**

**Requirements:**
- Developer Mode enabled OR
- Valid code signing certificate installed

**Download:**

[?? Download PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle](https://github.com/avikeid2007/Ping-Tool/releases/latest)

**Installation:**
1. Download the `.msixbundle` file
2. Right-click ? Install
3. Or use PowerShell:
   ```powershell
   Add-AppxPackage -Path "PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle"
   ```

---

## ?? What's New in Version 2.0.4.0

- Migrated to WinUI 3 / Windows App SDK
- Improved performance and stability
- Modern Windows 11 UI
- Multi-architecture support (x86, x64, ARM64)
- Bug fixes and improvements

See [CHANGELOG.md](CHANGELOG.md) for complete release notes.

---

## ?? Troubleshooting

### Installer Issues

**"Windows protected your PC" SmartScreen warning:**
1. Click "More info"
2. Click "Run anyway"
3. This warning appears because the installer is not signed with a code signing certificate

**".NET 8 Runtime not found":**
1. The installer should automatically prompt you to download it
2. Or manually download from: https://dotnet.microsoft.com/download/dotnet/8.0
3. Install ".NET Desktop Runtime 8.0.x (x64)"

**"Can't install on Windows 10":**
- Ensure you're running Windows 10 version 1809 or later
- Check: Settings ? System ? About ? Windows specifications

### MSIX Bundle Issues

**"This app package is not supported for installation by App Installer":**
- Enable Developer Mode: Settings ? Privacy & Security ? For developers
- Or install the certificate included in the package

**"Package could not be registered":**
- Ensure you have the latest Windows updates
- Try installing via PowerShell (see Method 3)

### General Issues

For support and bug reports:
- [GitHub Issues](https://github.com/avikeid2007/Ping-Tool/issues)
- [Discussions](https://github.com/avikeid2007/Ping-Tool/discussions)

---

## ?? License

This software is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## ?? Links

- **Source Code**: https://github.com/avikeid2007/Ping-Tool
- **Issues**: https://github.com/avikeid2007/Ping-Tool/issues
- **Releases**: https://github.com/avikeid2007/Ping-Tool/releases
- **Microsoft Store**: https://www.microsoft.com/store/apps/9NBLGGH4VQ4Q

---

## ??? Security

If you discover a security vulnerability, please email: [your-email@example.com]

---

**Made with ?? by Avnish Kumar**
