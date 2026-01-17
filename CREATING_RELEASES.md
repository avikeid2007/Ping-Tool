# Creating GitHub Releases

This guide explains how to create GitHub Releases for Ping Legacy.

## Understanding the Workflow

The GitHub Actions workflow (`build-and-release.yml`) runs different jobs based on the trigger:

| Trigger | Build MSIX | Build Installer | Test Installer | Create Release |
|---------|-----------|-----------------|----------------|----------------|
| Push to branch | ? Yes | ? Yes | ? Yes | ?? **Skipped** |
| Pull request | ? Yes | ? Yes | ? Yes | ?? **Skipped** |
| **Push version tag** | ? Yes | ? Yes | ? Yes | ? **YES!** |

## Why Was "Create GitHub Release" Skipped?

The workflow contains this condition:

```yaml
create-release:
  if: startsWith(github.ref, 'refs/tags/v')  # Only runs on version tags!
```

**Regular commits don't trigger releases** - only version tags do.

## Creating a Release

### Prerequisites

Before creating a release:

1. ? **All builds passing** (MSIX bundle and installer)
2. ? **Version numbers updated** in:
   - `Package.appxmanifest`
   - `InnoSetup.iss`
3. ? **CHANGELOG updated** (optional but recommended)
4. ? **All changes committed** to your branch

### Quick Method: Use the Script

```powershell
# Run the release script
.\CreateRelease.ps1 -Version "2.0.4.0"

# Follow the prompts
```

The script will:
1. Check for uncommitted changes
2. Create annotated tag (`v2.0.4.0`)
3. Push tag to GitHub
4. Trigger the release workflow

### Manual Method: Step by Step

#### Step 1: Update Version Numbers

**Update `Package.appxmanifest`:**
```xml
<Identity
  Name="34488AvnishKumar.PingLegacy"
  Publisher="CN=1494B4FB-4349-4083-8AFA-16D2A64B8D99"
  Version="2.0.4.0" />  <!-- Update this! -->
```

**Update `InnoSetup.iss`:**
```ini
#define MyAppVersion "2.0.4.0"  <!-- Update this! -->
```

#### Step 2: Commit Version Changes

```sh
git add Package.appxmanifest InnoSetup.iss
git commit -m "Bump version to 2.0.4.0"
git push origin avnish/migrateWinUI
```

Wait for builds to complete successfully.

#### Step 3: Create Version Tag

```sh
# Create annotated tag
git tag -a v2.0.4.0 -m "Release version 2.0.4.0"

# Push tag to GitHub
git push origin v2.0.4.0
```

#### Step 4: Monitor GitHub Actions

1. Go to: https://github.com/avikeid2007/Ping-Tool/actions
2. You'll see a new workflow run triggered by the tag
3. Watch the progress:
   - ? Build MSIX Bundle (~10 min)
   - ? Build Traditional Installer (~15 min)
   - ? Test Installer (~1 min)
   - ? **Create GitHub Release** (~2 min)

#### Step 5: Verify Release

Once complete, the release will be available at:
```
https://github.com/avikeid2007/Ping-Tool/releases/tag/v2.0.4.0
```

The release will contain:
- ?? `PingTool.WinUI3_2.0.4.0_x86_x64_ARM64.msixbundle` (~150 MB)
- ?? `PingLegacy-2.0.4.0-Setup.exe` (~150-200 MB)
- ?? Auto-generated release notes

## Release Naming Convention

Use semantic versioning with 4 parts:

```
v[MAJOR].[MINOR].[PATCH].[BUILD]

Examples:
- v2.0.4.0 - Current version
- v2.0.5.0 - Bug fix release
- v2.1.0.0 - Minor feature release
- v3.0.0.0 - Major release
```

**Important**: 
- Git tag: `v2.0.4.0` (with 'v' prefix)
- Version in files: `2.0.4.0` (without 'v')

## Workflow Triggers Explained

### Push to Branch (e.g., `avnish/migrateWinUI`)

```yaml
on:
  push:
    branches: [ main, avnish/migrateWinUI ]
```

**Runs**: Build jobs only (no release)  
**Purpose**: Verify builds work  
**Result**: Artifacts available for 30 days

### Push Version Tag (e.g., `v2.0.4.0`)

```yaml
on:
  push:
    tags:
      - 'v*'
```

**Runs**: All jobs including release  
**Purpose**: Create public release  
**Result**: GitHub Release with downloadable packages

### Pull Request

```yaml
on:
  pull_request:
    branches: [ main ]
```

**Runs**: Build jobs only (no release)  
**Purpose**: Verify PR doesn't break builds  
**Result**: Status check on PR

### Manual Trigger

```yaml
on:
  workflow_dispatch:
```

**How**: Go to Actions tab ? Select workflow ? Run workflow  
**Runs**: Build jobs only (no release unless on tag)  
**Purpose**: Test workflow manually

## Common Issues

### Issue: Tag Already Exists

**Error**: `tag 'v2.0.4.0' already has this tag`

**Solution**: Delete and recreate tag:
```sh
# Delete local tag
git tag -d v2.0.4.0

# Delete remote tag
git push origin :refs/tags/v2.0.4.0

# Recreate and push
git tag -a v2.0.4.0 -m "Release version 2.0.4.0"
git push origin v2.0.4.0
```

### Issue: Release Created But Empty

**Cause**: Workflow completed before artifacts were uploaded

**Solution**: 
1. Delete the release from GitHub
2. Delete the tag: `git push origin :refs/tags/v2.0.4.0`
3. Re-create and push the tag

### Issue: Release Job Still Skipped

**Check**:
1. Did you push a **tag** (not just a commit)?
2. Does the tag start with 'v'? (e.g., `v2.0.4.0`)
3. Check workflow logs for the condition evaluation

### Issue: Wrong Version in Release

**Cause**: Version numbers not updated before tagging

**Solution**:
1. Delete the release and tag
2. Update version numbers
3. Commit changes
4. Create new tag

## Testing Before Release

Before creating a release tag:

### 1. Test Builds Locally

```powershell
# Test MSIX bundle
.\BuildMsixBundle.ps1

# Test installer
.\BuildInstaller.ps1
```

### 2. Push to Branch First

```sh
git push origin avnish/migrateWinUI
```

Watch GitHub Actions to ensure builds succeed.

### 3. Download and Test Artifacts

1. Go to Actions tab
2. Click on latest workflow run
3. Download artifacts:
   - `msix-bundle`
   - `installer`
4. Test installation on clean VM

### 4. Only Then Create Release Tag

Once everything works, create the version tag.

## Release Checklist

Use this checklist before creating a release:

- [ ] Version updated in `Package.appxmanifest`
- [ ] Version updated in `InnoSetup.iss`
- [ ] CHANGELOG.md updated
- [ ] All changes committed and pushed
- [ ] Branch builds passing on GitHub Actions
- [ ] Artifacts downloaded and tested
- [ ] Ready to create tag

Then:

- [ ] Create version tag: `git tag -a v2.0.4.0 -m "Release version 2.0.4.0"`
- [ ] Push tag: `git push origin v2.0.4.0`
- [ ] Monitor GitHub Actions workflow
- [ ] Verify release created successfully
- [ ] Test download links
- [ ] Announce release (if applicable)

## Automating Releases

For automated releases in CI/CD pipelines, use the `CreateRelease.ps1` script:

```powershell
# In your CI/CD pipeline
.\CreateRelease.ps1 -Version $env:VERSION_NUMBER
```

Or directly:

```sh
git tag -a "v${VERSION}" -m "Release version ${VERSION}"
git push origin "v${VERSION}"
```

## Release Notes Customization

The workflow auto-generates release notes. To customize:

Edit `.github/workflows/build-and-release.yml`:

```yaml
- name: Create Release Notes
  id: release_notes
  run: |
    $notes = @"
    ## Your Custom Release Notes
    
    ### What's New in ${{ steps.get_version.outputs.VERSION }}
    - Feature 1
    - Feature 2
    - Bug fix 3
    
    [Full Changelog](https://github.com/avikeid2007/Ping-Tool/compare/v2.0.3.0...v${{ steps.get_version.outputs.VERSION }})
    "@
```

## Resources

- [GitHub Releases Documentation](https://docs.github.com/en/repositories/releasing-projects-on-github)
- [Git Tagging](https://git-scm.com/book/en/v2/Git-Basics-Tagging)
- [Semantic Versioning](https://semver.org/)

---

**Quick Reference**:
```sh
# Create and push release tag
git tag -a v2.0.4.0 -m "Release version 2.0.4.0"
git push origin v2.0.4.0

# Watch progress
# https://github.com/avikeid2007/Ping-Tool/actions

# View release
# https://github.com/avikeid2007/Ping-Tool/releases
```
