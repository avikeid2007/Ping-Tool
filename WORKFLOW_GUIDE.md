# GitHub Actions Workflow Guide

This document explains the automated build and release workflows for Ping Legacy.

## Overview

The repository uses GitHub Actions to automate:
1. **MSIX Bundle Creation** - For Microsoft Store submissions
2. **Traditional Installer Creation** - For direct downloads
3. **Release Publishing** - Automatic GitHub Releases
4. **Installer Testing** - Basic verification

## Workflow File

**Location**: `.github/workflows/build-and-release.yml`

## Triggering Workflows

### Automatic Triggers

#### On Push to Main Branches
```bash
git push origin main
# or
git push origin avnish/migrateWinUI
```

**What happens:**
- Builds MSIX bundle
- Builds traditional installer
- Uploads artifacts (but doesn't create release)

#### On Pull Request
```yaml
Pull request to main branch
```

**What happens:**
- Builds both packages for verification
- Runs tests
- No release created

#### On Version Tag (Creates Release)
```bash
git tag -a v2.0.4.0 -m "Release version 2.0.4.0"
git push origin v2.0.4.0
```

**What happens:**
- Builds MSIX bundle
- Builds traditional installer  
- Runs tests
- **Creates GitHub Release**
- Uploads both packages to release

### Manual Trigger

1. Go to repository on GitHub
2. Click **Actions** tab
3. Select **Build and Release** workflow
4. Click **Run workflow** button
5. Select branch
6. Click green **Run workflow** button

## Jobs Overview

### Job 1: `build-msix-bundle`

**Purpose**: Creates MSIX bundle for Microsoft Store

**Steps**:
1. Checkout code
2. Setup .NET 8
3. Setup MSBuild
4. Restore NuGet packages
5. Run `BuildMsixBundle.ps1`
6. Upload bundle as artifact
7. Upload symbols as artifact

**Outputs**:
- `PingTool.WinUI3_*.msixbundle` (~150 MB)
- Symbol packages (`.msixsym`)

**Artifact Name**: `msix-bundle`

### Job 2: `build-installer`

**Purpose**: Creates traditional Windows installer

**Steps**:
1. Checkout code
2. Setup .NET 8
3. Restore NuGet packages
4. Publish self-contained x64 app
5. Install Inno Setup via Chocolatey
6. Compile installer
7. Upload installer as artifact

**Outputs**:
- `PingLegacy-*-Setup.exe` (~150-200 MB)

**Artifact Name**: `installer`

### Job 3: `create-release`

**Purpose**: Creates GitHub Release with packages

**When**: Only on version tags (`v*`)

**Dependencies**: 
- `build-msix-bundle`
- `build-installer`

**Steps**:
1. Download MSIX bundle artifact
2. Download installer artifact
3. Extract version from tag
4. Generate release notes
5. Create GitHub Release
6. Upload both packages

**Permissions Required**: `contents: write`

### Job 4: `test-installer`

**Purpose**: Basic verification of installer

**Dependencies**: `build-installer`

**Steps**:
1. Download installer artifact
2. List files
3. Verify installer exists
4. Check file size

**Note**: Full installation testing requires VM

## Downloading Build Artifacts

### From GitHub Actions

1. Go to **Actions** tab
2. Click on a workflow run
3. Scroll to **Artifacts** section
4. Download:
   - `msix-bundle` - Contains `.msixbundle` file
   - `installer` - Contains `.exe` installer
   - `msix-symbols` - Contains debug symbols

**Note**: Artifacts are kept for 30 days

### From GitHub Releases

1. Go to **Releases** section
2. Click on latest release
3. Download from **Assets** section:
   - `PingTool.WinUI3_*_x86_x64_ARM64.msixbundle`
   - `PingLegacy-*-Setup.exe`

## Environment Variables

Configured in workflow file:

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  PROJECT_PATH: 'PingTool.WinUI3/PingTool.WinUI3.csproj'
  SOLUTION_PATH: 'Ping-Tool.sln'
```

## Secrets Required

**None currently required** for basic builds.

### For Code Signing (Future)

If adding code signing, you'll need:

```yaml
secrets:
  CERTIFICATE_BASE64: <base64 encoded .pfx>
  CERTIFICATE_PASSWORD: <certificate password>
```

Add via: Settings ? Secrets and variables ? Actions ? New repository secret

## Modifying Workflows

### Change .NET Version

Edit in `.github/workflows/build-and-release.yml`:

```yaml
env:
  DOTNET_VERSION: '9.0.x'  # Change version
```

### Add Additional Platforms

To build x86 installer:

```yaml
- name: Publish x86
  run: |
    dotnet publish ${{ env.PROJECT_PATH }} `
      -c Release `
      -r win-x86 `
      --self-contained true `
      -o Output/Publish/win-x86
```

### Modify Release Notes

Edit in workflow file under `Create Release Notes` step:

```yaml
- name: Create Release Notes
  id: release_notes
  run: |
    $notes = @"
    ## Your Custom Release Notes Here
    "@
```

### Change Artifact Retention

Default is 30 days. To change:

```yaml
- name: Upload MSIX Bundle
  uses: actions/upload-artifact@v4
  with:
    retention-days: 90  # Keep for 90 days
```

## Troubleshooting

### Build Fails: "Inno Setup not found"

**Cause**: Chocolatey installation failed or timed out

**Solution**: Check Chocolatey installation step in logs

### Build Fails: ".NET SDK not found"

**Cause**: .NET setup action failed

**Solution**: Verify `DOTNET_VERSION` matches available versions

### Release Not Created

**Cause**: Workflow triggered by push, not tag

**Solution**: Ensure you pushed a version tag:
```bash
git tag -a v2.0.4.0 -m "Release"
git push origin v2.0.4.0
```

### Artifact Upload Failed

**Cause**: File path doesn't match expected location

**Solution**: Check build output paths in workflow logs

### Permission Denied on Release

**Cause**: Missing `contents: write` permission

**Solution**: Verify permissions in workflow:
```yaml
permissions:
  contents: write
```

## Workflow Status Badge

Add to README.md:

```markdown
[![Build and Release](https://github.com/avikeid2007/Ping-Tool/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/avikeid2007/Ping-Tool/actions/workflows/build-and-release.yml)
```

## Local Testing of Workflow

### Act (GitHub Actions Local Runner)

Install [Act](https://github.com/nektos/act):

```bash
choco install act-cli
```

Run workflow locally:

```bash
# Test push event
act push

# Test tag event
act push -e event.json
```

Create `event.json` for tag testing:
```json
{
  "ref": "refs/tags/v2.0.4.0"
}
```

**Note**: Some actions may not work locally (e.g., artifact uploads)

## Workflow Optimization

### Current Build Time

Approximate times:
- `build-msix-bundle`: ~10-15 minutes
- `build-installer`: ~15-20 minutes
- `create-release`: ~2-3 minutes
- `test-installer`: ~1 minute

**Total**: ~30-40 minutes

### Optimization Ideas

1. **Cache NuGet packages**:
```yaml
- name: Cache NuGet
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```

2. **Parallel builds**:
   - Already implemented (jobs run in parallel)

3. **Reusable workflows**:
   - Extract common steps to reusable workflow

## Monitoring and Notifications

### Email Notifications

GitHub sends emails on:
- Workflow failures (if you're the author)
- Pull request checks

### Slack/Discord Integration

Add webhook notification:

```yaml
- name: Notify on failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

## Best Practices

1. ? **Always test locally** before pushing tags
2. ? **Use semantic versioning** (vMAJOR.MINOR.PATCH.BUILD)
3. ? **Review workflow logs** for warnings
4. ? **Keep secrets secure** (never commit certificates)
5. ? **Test artifacts** before public release
6. ? **Update documentation** when changing workflows

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions)
- [Action Marketplace](https://github.com/marketplace?type=actions)
- [Act - Local Testing](https://github.com/nektos/act)

---

**Last Updated**: January 2026  
**Workflow Version**: 1.0  
**Maintainer**: Avnish Kumar
