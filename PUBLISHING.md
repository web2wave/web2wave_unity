# Publishing Web2Wave Unity Package

This guide explains how to make the Web2Wave Unity SDK publicly available.

## Step 1: Create GitHub Repository

### Option A: Via GitHub Website

1. Go to https://github.com/organizations/web2wave/repositories/new
2. Repository name: `web2wave_unity`
3. Description: "Web2Wave Unity SDK - Subscription and user properties management"
4. Visibility: **Public** (required for Unity Package Manager)
5. **DO NOT** initialize with README, .gitignore, or license (we already have these)
6. Click "Create repository"

### Option B: Via GitHub CLI (if installed)

```bash
gh repo create web2wave/web2wave_unity --public --description "Web2Wave Unity SDK"
```

## Step 2: Push Code to GitHub

Once the repository is created, push the code:

```bash
cd web2wave_unity
git remote set-url origin https://github.com/web2wave/web2wave_unity.git
git push -u origin main
git push origin v1.1.1  # Push version tag
```

## Step 3: Make SDK Publicly Available

Unity packages can be distributed in several ways. Here are the recommended options:

### Method 1: Git URL (Recommended - Free)

This is the easiest method. Users can install directly from GitHub:

**Installation URL:**
```
https://github.com/web2wave/web2wave_unity.git
```

**Users install via:**
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` > `Add package from git URL...`
3. Enter: `https://github.com/web2wave/web2wave_unity.git`
4. Click `Add`

**Advantages:**
- ✅ Free
- ✅ Automatic updates (users can specify version tags)
- ✅ No build/export needed
- ✅ Direct from source

**Version-specific installation:**
```
https://github.com/web2wave/web2wave_unity.git#v1.1.1
```

### Method 2: Unity Package Manager (UPM) Registry

For better discoverability, you can publish to a Unity registry:

1. **Set up a scoped registry** (users add to `Packages/manifest.json`):
   ```json
   {
     "scopedRegistries": [
       {
         "name": "Web2Wave",
         "url": "https://package.openupm.com",
         "scopes": ["com.web2wave"]
       }
     ],
     "dependencies": {
       "com.web2wave.web2wave": "1.1.1"
     }
   }
   ```

2. **Publish to OpenUPM** (if desired):
   - Visit https://openupm.com/
   - Submit your package following their guidelines
   - They'll create an automated registry

### Method 3: Unity Asset Store

For maximum visibility:

1. **Prepare the package:**
   - Create a `.unitypackage` file
   - Include all Runtime files
   - Create screenshots and demo content

2. **Submit to Asset Store:**
   - Go to https://publisher.assetstore.unity3d.com/
   - Create a new asset
   - Upload package and documentation
   - Submit for review

**Note:** Asset Store has review process and revenue sharing.

### Method 4: Custom UPM Registry (Self-hosted)

For enterprise/private distribution:

1. Set up a simple HTTP server or use services like:
   - GitHub Pages
   - AWS S3
   - Azure Blob Storage

2. Host `package.json` and files following Unity's package structure

3. Users configure custom registry in Unity

## Recommended Approach

**For Open Source SDKs (like this):**

Use **Method 1 (Git URL)** because:
- ✅ Zero setup cost
- ✅ Works immediately after pushing to GitHub
- ✅ Users get latest code directly
- ✅ Easy version management with git tags

## Version Management

### Creating New Versions

1. **Update version in `package.json`:**
   ```json
   {
     "version": "1.1.2"
   }
   ```

2. **Update CHANGELOG.md**

3. **Commit and tag:**
   ```bash
   git add package.json CHANGELOG.md
   git commit -m "Release v1.1.2"
   git tag v1.1.2
   git push origin main
   git push origin v1.1.2
   ```

4. **Create GitHub Release** (optional but recommended):
   - Go to: https://github.com/web2wave/web2wave_unity/releases/new
   - Tag: `v1.1.2`
   - Title: `v1.1.2`
   - Description: Copy from CHANGELOG.md
   - Publish release

## Verification

After pushing to GitHub, verify it's publicly accessible:

1. **Check repository is public:**
   - Visit: https://github.com/web2wave/web2wave_unity
   - Should be accessible without login

2. **Test installation in Unity:**
   - Create new Unity project
   - Try installing via Package Manager with git URL
   - Verify package loads correctly

3. **Verify package.json is accessible:**
   - Visit: https://raw.githubusercontent.com/web2wave/web2wave_unity/main/package.json
   - Should show package.json content

## Documentation

Ensure your README.md includes:
- ✅ Installation instructions (git URL method)
- ✅ Quick start guide
- ✅ API documentation
- ✅ Examples
- ✅ Platform requirements

## Troubleshooting

### "Package not found" error in Unity

- Check repository is **public**
- Verify `package.json` is in the root
- Ensure Unity version compatibility (2020.3+)
- Check git URL format is correct

### Version-specific install not working

- Verify git tag exists: `git ls-remote --tags origin`
- Tag format should be: `v1.1.1` (matches version in package.json)

### Dependency issues

- Newtonsoft.Json is included as dependency in package.json
- Unity should auto-resolve via Package Manager

## Security Considerations

- Repository should be **public** for git URL method
- No sensitive data in code
- API keys should never be committed
- Use `.gitignore` properly

## Next Steps After Publishing

1. ✅ Update main project README with Unity SDK link
2. ✅ Add Unity SDK to organization's package list
3. ✅ Create example Unity project (optional but helpful)
4. ✅ Share on Unity forums/communities
5. ✅ Update documentation site with Unity installation steps

Your Unity SDK is now ready to be publicly available via GitHub!

