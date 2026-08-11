# Quick Setup Instructions

## Step 1: Create GitHub Repository

**Important:** The repository must be created as **PUBLIC** for Unity Package Manager to work.

1. Go to: https://github.com/organizations/web2wave/repositories/new
2. Repository name: `web2wave_unity`
3. Description: "Web2Wave Unity SDK - Subscription and user properties management"
4. Visibility: **Public** ⚠️
5. **DO NOT** check any initialization options (no README, .gitignore, license)
6. Click "Create repository"

## Step 2: Push Code

```bash
cd web2wave_unity

# Verify remote is set
git remote -v

# If not set or incorrect:
git remote set-url origin https://github.com/web2wave/web2wave_unity.git

# Push code
git push -u origin main

# Push version tag
git push origin v1.1.1
```

## Step 3: Verify Public Access

1. Visit: https://github.com/web2wave/web2wave_unity
2. Verify repository is public (visible without login)
3. Check that all files are present

## How Users Will Install

Once the repository is public, users can install via Unity Package Manager:

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` > `Add package from git URL...`
3. Enter: `https://github.com/web2wave/web2wave_unity.git`
4. Click `Add`

**For specific version:**
```
https://github.com/web2wave/web2wave_unity.git#v1.1.1
```

## That's It!

Your Unity SDK is now publicly available. No additional publishing steps needed - Unity Package Manager works directly with public Git repositories.

