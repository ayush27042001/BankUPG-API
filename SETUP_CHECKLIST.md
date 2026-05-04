# BankUPG CI/CD Setup Checklist

## Quick Start (5 minutes)

### ☐ Step 1: Add 10 GitHub Secrets

Go to: https://github.com/ayush27042001/BankUPG-API/settings/secrets/actions

Add these secrets:

| # | Secret Name | Value |
|---|-------------|-------|
| 1 | `PLESK_FTP_HOST` | `103.205.142.34` |
| 2 | `PLESK_FTP_USERNAME` | `bankupg_cicd` |
| 3 | `PLESK_FTP_PASSWORD` | `Ayush@123123` |
| 4 | `PROD_CONNECTION_STRING` | `Data Source=103.205.142.34,1433;Initial Catalog=BankuPG;Persist Security Info=True;User ID=sa;Password=zUG93NOh6WE7BQIS` |
| 5 | `PROD_JWT_SECRET` | `BankUPG_SuperSecret_Production_Key_2024_HighSecurity_JWT_Token_Signing_Key_Minimum_32_Chars_Long` |
| 6 | `SMS_API_URL` | `http://123.108.46.13/sms-panel/api/http/index.php` |
| 7 | `SMS_USERNAME` | `INTSALITE` |
| 8 | `SMS_API_KEY` | `90852-AB6E3` |
| 9 | `SMS_TEMPLATE_ID` | `1707175178872908463` |
| 10 | `CASHFREE_CLIENT_ID` | `CF898769D0DKQJG3BM1S73FBE6OG` |
| 11 | `CASHFREE_CLIENT_SECRET` | `cfsk_ma_prod_7a7157c5ac1ae3a067ec8c23080ff94d_e50a26d4` |

### ☐ Step 2: Push Workflow Files to GitHub

```bash
git add .github/workflows/
git add DEPLOYMENT.md
git add SETUP_CHECKLIST.md
git commit -m "Add CI/CD pipeline for automatic deployment"
git push origin main
```

### ☐ Step 3: Verify Pipeline is Active

1. Go to https://github.com/ayush27042001/BankUPG-API/actions
2. You should see the workflow listed
3. Make a test change and push to trigger it

### ☐ Step 4: Test Deployment

1. Create a branch: `git checkout -b test/pipeline`
2. Make a small change (e.g., update a comment)
3. Push: `git push origin test/pipeline`
4. Create Pull Request on GitHub
5. Wait for checks to pass, then merge
6. Check deployment at: http://103.205.142.34/health

## Daily Workflow

```bash
# Start new feature
git checkout -b feature/my-feature

# Make changes, commit
git add .
git commit -m "Add new feature"

# Push and create PR
git push origin feature/my-feature

# On GitHub: Create Pull Request → Wait for checks → Merge
# Deployment happens automatically!
```

## Files Created

- `.github/workflows/pr-validation.yml` - Validates PRs
- `.github/workflows/ci-cd.yml` - Build, test, and deploy
- `BankUPG.API/web.config` - IIS configuration
- `DEPLOYMENT.md` - Full documentation
- `SETUP_CHECKLIST.md` - This file

## Support

If deployment fails:
1. Check GitHub Actions logs
2. Verify FTP credentials work in FileZilla
3. Check Plesk at `https://103.205.142.34:8443`
4. Test health endpoint: `http://103.205.142.34/health`
