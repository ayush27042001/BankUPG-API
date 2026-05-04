# BankUPG CI/CD Deployment Guide

## Overview

This repository includes GitHub Actions workflows for continuous integration and deployment.

## Workflows

### 1. `pr-validation.yml` - Pull Request Validation
**Triggers:** On every PR to `main`, `develop`, or `release/*` branches

**What it does:**
- Builds the .NET solution
- Runs code analysis
- Posts status as PR comment

### 2. `ci-cd.yml` - Full CI/CD Pipeline (FTP)
**Triggers:** 
- PR events (build + test only)
- Push to `main` (build + test + deploy)

**What it does:**
- Builds and tests on every PR
- Deploys to production on merge to main via FTP to `103.205.142.34`

### 3. `deploy-ssh.yml` - SSH Deployment (Alternative)
**Triggers:** Manual trigger or push to main

**What it does:**
- Builds and deploys via SCP/SSH
- Can restart IIS application pool

## Required GitHub Secrets

### For FTP Deployment (`ci-cd.yml`)

| Secret Name | Value |
|-------------|-------|
| `PLESK_FTP_HOST` | `103.205.142.34` |
| `PLESK_FTP_USERNAME` | `bankupg_cicd` |
| `PLESK_FTP_PASSWORD` | `Ayush@123123` |

### Application Configuration Secrets

| Secret Name | Value |
|-------------|-------|
| `PROD_CONNECTION_STRING` | `Data Source=103.205.142.34,1433;Initial Catalog=BankuPG;Persist Security Info=True;User ID=sa;Password=zUG93NOh6WE7BQIS` |
| `PROD_JWT_SECRET` | `BankUPG_SuperSecret_Production_Key_2024_HighSecurity_JWT_Token_Signing_Key_Minimum_32_Chars_Long` |
| `SMS_API_URL` | `http://123.108.46.13/sms-panel/api/http/index.php` |
| `SMS_USERNAME` | `INTSALITE` |
| `SMS_API_KEY` | `90852-AB6E3` |
| `SMS_TEMPLATE_ID` | `1707175178872908463` |
| `CASHFREE_CLIENT_ID` | `CF898769D0DKQJG3BM1S73FBE6OG` |
| `CASHFREE_CLIENT_SECRET` | `cfsk_ma_prod_7a7157c5ac1ae3a067ec8c23080ff94d_e50a26d4` |

## Setup Instructions

### Step 1: Add Secrets to GitHub (REQUIRED)

You must add **10 secrets** before the pipeline will work:

1. Go to https://github.com/ayush27042001/BankUPG-API/settings/secrets/actions
2. Click **New repository secret**
3. Add each secret from the tables above (10 total)

**⚠️ IMPORTANT:** The pipeline will fail if any of these secrets are missing.

### Step 2: Verify FTP Access

1. Log in to Plesk: `https://103.205.142.34:8443`
2. Go to **Websites & Domains** → **FTP Access**
3. Verify the `bankupg_cicd` account exists and is active
4. Test FTP connection to `103.205.142.34:21` with credentials

### Step 3: Enable Branch Protection

1. Go to **Settings** → **Branches**
2. Add rule for `main` branch:
   - ✅ Require pull request reviews
   - ✅ Require status checks to pass
   - ✅ Require branches to be up to date before merging
   - Select `build-and-test` status check

### Step 4: First-Time Deployment (Manual)

Before using the automatic pipeline, do a manual deploy to verify everything works:

```bash
# Clone your repo locally
git clone https://github.com/ayush27042001/BankUPG-API.git
cd BankUPG-API

# Build and publish locally
dotnet publish BankUPG.API/BankUPG.API.csproj -c Release -o ./publish

# Use any FTP client (FileZilla, WinSCP) to upload to:
# Host: 103.205.142.34:21
# Username: bankupg_cicd
# Password: Ayush@123123
# Remote path: /httpdocs/
```

### Step 5: Use Automatic Pipeline

Once manual deploy works:

**Merge Request Flow:**
1. Create a feature branch: `git checkout -b feature/my-feature`
2. Make changes and push: `git push origin feature/my-feature`
3. Create Pull Request on GitHub
4. Wait for checks to pass
5. Merge to `main` → **Auto-deployment triggers**

**Check deployment status:**
- Go to **Actions** tab in GitHub
- Look for green checkmark on the latest run
- Check your API at: `http://103.205.142.34/health`

## Troubleshooting

### Deployment Failures

**FTP Connection Issues:**
- Verify FTP credentials in Plesk: `103.205.142.34:8443`
- Ensure FTP service is running on the server
- Check if port 21 is open in firewall
- Plesk FTP uses the account format: `bankupg_cicd`

**500 Internal Server Error after deploy:**
- Verify `web.config` is deployed correctly
- Check IIS application pool is running .NET 9.0
- Check `httpdocs/web.config` exists on server

**Database Connection Errors:**
- Verify SQL Server is running on `103.205.142.34:1433`
- Check firewall allows SQL Server connections
- Test connection string locally first

### Database Migration

If using Entity Framework migrations, add this step to deployment:

```yaml
- name: Run EF Migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update \
      --project ./BankUPG.Infrastructure \
      --startup-project ./BankUPG.API \
      --connection "${{ secrets.PROD_CONNECTION_STRING }}"
```

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Developer     │────▶│   Feature       │────▶│   Pull Request  │
│   Workstation   │     │   Branch        │     │   Created       │
└─────────────────┘     └─────────────────┘     └────────┬────────┘
                                                         │
                              ┌──────────────────────────┘
                              ▼
                    ┌─────────────────┐
                    │   PR Validation │
                    │   (Build/Test)  │
                    └────────┬────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
    ┌─────────────────┐          ┌─────────────────┐
    │   Tests Pass    │          │   Tests Fail    │
    │   Merge Allowed │          │   Fix Required  │
    └────────┬────────┘          └─────────────────┘
             │
             ▼
    ┌─────────────────┐
    │   Merge to Main │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │   Auto Deploy   │
    │   to Production │
    └─────────────────┘
```

## Support

For issues with the pipeline, check:
1. GitHub Actions logs (Actions tab)
2. Plesk logs (if deployment fails)
3. Application logs on the server
