# Release Checklist for Ping Legacy

Use this checklist when preparing a new release.

## Pre-Release Preparation

### Version Update
- [ ] Update version in `Package.appxmanifest`
  - [ ] Update `<Identity Version="X.X.X.X" />`
- [ ] Update version in `InnoSetup.iss`
  - [ ] Update `#define MyAppVersion "X.X.X.X"`
- [ ] Update version in `PingTool.WinUI3.csproj` (if applicable)
- [ ] Update version references in documentation
  - [ ] `CREATE_MSIX_BUNDLE.md`
  - [ ] `CREATE_INSTALLER.md`
  - [ ] `DOWNLOAD.md`

### Code Quality
- [ ] All unit tests passing
- [ ] No critical bugs in issue tracker
- [ ] Code reviewed and merged to main branch
- [ ] All dependencies up to date
- [ ] No security vulnerabilities detected

### Documentation
- [ ] Update CHANGELOG.md with new features/fixes
- [ ] Update README.md if needed
- [ ] Update screenshots if UI changed
- [ ] Review and update help documentation

## Local Build Testing

### MSIX Bundle
- [ ] Run `BuildMsixBundle.ps1` successfully
- [ ] Verify bundle contains all three architectures (x86, x64, ARM64)
- [ ] Check bundle size is reasonable (~150 MB)
- [ ] Inspect bundle contents:
  ```powershell
  makeappx unbundle /p bundle.msixbundle /d temp /v
  ```

### Traditional Installer
- [ ] Run `BuildInstaller.ps1` successfully
- [ ] Installer created in `Output\Installer\`
- [ ] Installer size is reasonable (~150-200 MB)
- [ ] Test installer on clean VM (Windows 10)
- [ ] Test installer on clean VM (Windows 11)
- [ ] Test .NET runtime detection
- [ ] Test desktop shortcut creation
- [ ] Test Start Menu shortcuts
- [ ] Test uninstallation
- [ ] Verify no leftover files after uninstall

## GitHub Actions

### Trigger Release Build
- [ ] Create version tag:
  ```bash
  git tag -a vX.X.X.X -m "Release version X.X.X.X"
  git push origin vX.X.X.X
  ```
- [ ] Wait for GitHub Actions to complete
- [ ] Check Actions tab for any errors
- [ ] Verify all jobs completed successfully:
  - [ ] `build-msix-bundle`
  - [ ] `build-installer`
  - [ ] `create-release`
  - [ ] `test-installer`

### Verify Artifacts
- [ ] Download MSIX bundle from Actions artifacts
- [ ] Download installer from Actions artifacts
- [ ] Verify file sizes match expectations
- [ ] Test downloaded artifacts locally

## GitHub Release

### Release Page
- [ ] Verify GitHub Release was created automatically
- [ ] Check release title is correct
- [ ] Review release notes
- [ ] Verify both packages are attached:
  - [ ] `.msixbundle` file
  - [ ] `.exe` installer file
- [ ] Add SHA256 checksums to release notes
- [ ] Mark as pre-release if appropriate
- [ ] Publish release (if draft)

### Post-Release Verification
- [ ] Download packages from release page
- [ ] Verify download links work
- [ ] Test installation from downloaded packages
- [ ] Check file sizes and checksums

## Microsoft Store Submission

### Upload to Partner Center
- [ ] Sign in to [Partner Center](https://partner.microsoft.com)
- [ ] Navigate to Ping Legacy app
- [ ] Start new submission
- [ ] Upload `.msixbundle` from GitHub Release
- [ ] Wait for validation to complete
- [ ] Fix any validation errors

### Submission Details
- [ ] Update "What's new in this version"
- [ ] Update screenshots if needed
- [ ] Review app description
- [ ] Check pricing & availability
- [ ] Review age ratings
- [ ] Submit for certification

### Certification Monitoring
- [ ] Monitor certification status
- [ ] Respond to any certification issues
- [ ] Wait for approval (typically 24-48 hours)

## Post-Release Tasks

### Documentation Updates
- [ ] Update website with new version number
- [ ] Update direct download links
- [ ] Update changelog on website
- [ ] Post announcement on GitHub Discussions

### Social Media / Communication
- [ ] Post release announcement (if applicable)
- [ ] Notify users of new version
- [ ] Update any external documentation

### Monitoring
- [ ] Monitor GitHub Issues for new bug reports
- [ ] Monitor crash reports in AppCenter (if configured)
- [ ] Check Microsoft Store reviews
- [ ] Monitor download statistics

### Housekeeping
- [ ] Close completed milestone in GitHub
- [ ] Archive old release artifacts
- [ ] Update project board
- [ ] Plan next release features

## Rollback Plan (If Needed)

If critical issues are discovered:

### Microsoft Store
- [ ] Submit hotfix or rollback to previous version
- [ ] Update store description with known issues

### GitHub Release
- [ ] Mark release as pre-release
- [ ] Add warning to release notes
- [ ] Remove download links if severe

### Communication
- [ ] Post issue notification
- [ ] Provide workarounds if available
- [ ] Set timeline for fix

## Automation Verification

### GitHub Actions Checks
- [ ] Workflow triggers on tags
- [ ] Builds complete without errors
- [ ] Artifacts uploaded correctly
- [ ] Release created automatically
- [ ] Release notes generated properly

### Future Improvements
- [ ] Consider adding automated tests
- [ ] Add installer smoke tests
- [ ] Implement automated deployment
- [ ] Add rollback automation

## Sign-off

- [ ] Release Manager: _________________ Date: _______
- [ ] QA Lead: _________________ Date: _______
- [ ] Project Owner: _________________ Date: _______

---

## Version History

| Version | Release Date | Released By | Notes |
|---------|--------------|-------------|-------|
| 2.0.4.0 | 2026-01-XX   | Avnish Kumar | Initial WinUI3 release |
| 2.0.5.0 | TBD          | TBD          | Future release |

---

## Quick Reference Commands

### Create Release Tag
```bash
VERSION="2.0.5.0"
git tag -a v$VERSION -m "Release version $VERSION"
git push origin v$VERSION
```

### Generate Checksums
```powershell
# MSIX Bundle
Get-FileHash "PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle" -Algorithm SHA256

# Installer
Get-FileHash "PingLegacy-2.0.4.0-Setup.exe" -Algorithm SHA256
```

### Test Installer
```powershell
# Silent install (testing)
.\PingLegacy-2.0.4.0-Setup.exe /VERYSILENT /SUPPRESSMSGBOXES

# Silent uninstall
"C:\Program Files\Ping Legacy\unins000.exe" /VERYSILENT /SUPPRESSMSGBOXES
```

### Verify MSIX Bundle
```powershell
# List contents
makeappx unbundle /p bundle.msixbundle /d temp_extract /v

# Verify package
MakeCert.exe -r -h 0 -n "CN=Test" -eku 1.3.6.1.5.5.7.3.3 -pe -sv TestCert.pvk TestCert.cer
```

---

**Template Version**: 1.0  
**Last Updated**: January 2026  
**Owner**: Avnish Kumar
