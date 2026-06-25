import os, json, urllib.request

def post(url, hdrs={}, data=None):
    req = urllib.request.Request(url, data=data, headers=hdrs, method='POST')
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())

def get(url, hdrs={}):
    req = urllib.request.Request(url, headers=hdrs)
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())

def get_zoho_token():
    url = (
        "https://accounts.zoho.in/oauth/v2/token"
        f"?refresh_token={os.environ['ZOHO_REFRESH_TOKEN']}"
        f"&client_id={os.environ['ZOHO_CLIENT_ID']}"
        f"&client_secret={os.environ['ZOHO_CLIENT_SECRET']}"
        "&grant_type=refresh_token"
    )
    return post(url)['access_token']

def build_html(status, details):
    is_success = status == 'success'

    accent       = '#059669' if is_success else '#dc2626'
    accent_light = '#d1fae5' if is_success else '#fee2e2'
    accent_dark  = '#065f46' if is_success else '#7f1d1d'
    row_alt      = '#f0fdf4' if is_success else '#fff5f5'
    status_icon  = '&#9989;' if is_success else '&#10060;'
    badge_label  = 'SUCCESS' if is_success else 'FAILED'
    title        = details.get('title', 'Deployment Notification')
    subtitle     = ('Your deployment completed without errors and is now live in production.'
                    if is_success else
                    'An error occurred during the pipeline. Immediate attention required.')

    rows_html = ''
    for i, (k, v) in enumerate(details.get('rows', {}).items()):
        bg = row_alt if i % 2 == 0 else '#ffffff'
        rows_html += (
            f'<tr>'
            f'<td style="padding:11px 16px;background:{bg};border-bottom:1px solid #f1f5f9;'
            f'font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.6px;'
            f'color:#64748b;width:130px;white-space:nowrap;">{k}</td>'
            f'<td style="padding:11px 16px;background:{bg};border-bottom:1px solid #f1f5f9;'
            f'font-size:13px;color:#1e293b;word-break:break-all;">{v}</td>'
            f'</tr>'
        )

    btns_html = ''
    items = list(details.get('buttons', {}).items())
    for i, (label, url) in enumerate(items):
        mr = 'margin-right:10px;' if i < len(items) - 1 else ''
        btns_html += (
            f'<a href="{url}" style="display:inline-block;padding:13px 28px;background:#7c3aed;'
            f'color:#ffffff;text-decoration:none;border-radius:4px;font-size:13px;'
            f'font-weight:700;letter-spacing:0.5px;{mr}">{label}</a>'
        )

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<meta http-equiv="Content-Type" content="text/html;charset=UTF-8"/>
<style>
  @media only screen and (max-width:620px){{
    .card{{width:100%!important;border-radius:0!important;}}
    .pad{{padding:24px 20px!important;}}
    .hero-title{{font-size:22px!important;}}
    .btn-wrap{{display:block!important;margin-bottom:10px!important;}}
  }}
</style>
</head>
<body style="margin:0;padding:0;background:#e8edf5;
  font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;">
<table width="100%" cellpadding="0" cellspacing="0" border="0"
  style="background:#e8edf5;padding:40px 0;">
<tr><td align="center" style="padding:0 16px;">

  <table class="card" width="600" cellpadding="0" cellspacing="0" border="0"
    style="max-width:600px;width:100%;background:#ffffff;border-radius:24px;
    overflow:hidden;box-shadow:0 24px 64px rgba(15,23,42,0.14);">

    <!-- HEADER -->
    <tr>
      <td style="background:linear-gradient(135deg,#0f172a 0%,#1e3a8a 55%,#2563eb 100%);
                 padding:36px 40px 28px;text-align:center;">
        <img src="https://www.banku.co.in/assets/images/org/1.png"
             alt="BankU" width="120"
             style="display:block;margin:0 auto 20px;max-width:120px;"/>
        <span style="display:inline-block;background:{accent_light};color:{accent_dark};
                     padding:5px 20px;border-radius:50px;font-size:10px;font-weight:800;
                     letter-spacing:2.5px;text-transform:uppercase;">{badge_label}</span>
      </td>
    </tr>

    <!-- HERO -->
    <tr>
      <td class="pad" style="padding:40px 40px 28px;text-align:center;
                              background:linear-gradient(180deg,#f8faff 0%,#ffffff 100%);
                              border-bottom:1px solid #f1f5f9;">
        <div style="font-size:56px;line-height:1;margin-bottom:18px;">{status_icon}</div>
        <h1 class="hero-title" style="margin:0 0 12px;font-size:28px;font-weight:800;
                                       color:#0f172a;letter-spacing:-0.5px;">{title}</h1>
        <p style="margin:0 auto;font-size:14px;color:#64748b;line-height:1.7;max-width:420px;">
          {subtitle}</p>
      </td>
    </tr>

    <!-- DETAILS -->
    <tr>
      <td class="pad" style="padding:28px 40px 20px;">
        <p style="margin:0 0 14px;font-size:10px;font-weight:800;letter-spacing:2px;
                  text-transform:uppercase;color:{accent};">&#9632;&nbsp; Pipeline Details</p>
        <table width="100%" cellpadding="0" cellspacing="0" border="0"
          style="border-radius:14px;overflow:hidden;border:1.5px solid #e2e8f0;">
          {rows_html}
        </table>
      </td>
    </tr>

    <!-- TIMELINE BADGE -->
    <tr>
      <td class="pad" style="padding:4px 40px 28px;text-align:center;">
        <span style="display:inline-block;background:#f1f5f9;color:#64748b;padding:6px 18px;
                     border-radius:50px;font-size:11px;font-weight:600;">
          &#128336;&nbsp; Automated via BankUPG CI/CD Pipeline
        </span>
      </td>
    </tr>

    <!-- CTA BUTTONS -->
    <tr>
      <td class="pad" style="padding:0 40px 40px;text-align:center;">
        {btns_html}
      </td>
    </tr>

    <!-- FOOTER -->
    <tr>
      <td style="background:#0f172a;padding:26px 40px;text-align:center;
                 border-radius:0 0 24px 24px;">
        <p style="margin:0 0 8px;font-size:14px;font-weight:700;color:#ffffff;
                  letter-spacing:0.3px;">BankU &mdash; Secure Banking Platform</p>
        <p style="margin:0;font-size:12px;color:#475569;line-height:2;">
          &copy; 2026 BankU. All rights reserved.<br/>
          <span style="color:#334155;">
            Secure Banking &nbsp;&bull;&nbsp; Trusted Services &nbsp;&bull;&nbsp; Built with &#x2764;
          </span>
        </p>
      </td>
    </tr>

  </table>

  <p style="margin:20px 0 0;font-size:11px;color:#94a3b8;text-align:center;">
    This is an automated notification from BankUPG CI/CD. Do not reply to this email.
  </p>

</td></tr>
</table>
</body>
</html>"""

def send_email(subject, html):
    token = get_zoho_token()
    auth = {"Authorization": f"Zoho-oauthtoken {token}"}
    acct_id = get("https://mail.zoho.in/api/accounts", hdrs=auth)['data'][0]['accountId']

    author_email = os.environ.get('AUTHOR_EMAIL', '')
    committer_email = os.environ.get('COMMITTER_EMAIL', '')
    to = ','.join({e for e in [author_email, committer_email,
                                'developers@banku.co.in', 'ak.mehra@banku.co.in'] if e})

    payload = json.dumps({
        "fromAddress": os.environ['ZOHO_FROM_EMAIL'],
        "toAddress": to,
        "subject": subject,
        "content": html,
        "mailFormat": "html"
    }).encode()

    auth['Content-Type'] = 'application/json'
    post(f"https://mail.zoho.in/api/accounts/{acct_id}/messages", hdrs=auth, data=payload)
    print(f"Email sent: {subject} → {to}")

if __name__ == '__main__':
    status = os.environ.get('EMAIL_STATUS', 'failure')
    is_success = status == 'success'

    if os.environ.get('EMAIL_JOB') == 'build':
        title = 'Build & Test Passed' if is_success else 'Build & Test Failed'
        subject = f"{'✅' if is_success else '❌'} [BankUPG] {title} — {os.environ.get('BRANCH','')}"
        details = {
            'title': title,
            'rows': {
                'Branch': os.environ.get('BRANCH', ''),
                'Commit': os.environ.get('COMMIT_SHA', ''),
                'Author': f"{os.environ.get('AUTHOR_NAME','')} ({os.environ.get('AUTHOR_EMAIL','')})",
                'Triggered by': os.environ.get('ACTOR', ''),
                'Event': os.environ.get('EVENT', ''),
            },
            'buttons': {'&#128279; View Run on GitHub': os.environ.get('RUN_URL', '')}
        }
    else:
        title = 'Deployed to Production' if is_success else 'Production Deployment FAILED'
        subject = f"{'✅' if is_success else '❌'} [BankUPG] {title} — {os.environ.get('COMMIT_SHA','')[:7]}"
        btns = {'&#128279; View Live API': 'https://apipg.banku.co.in/swagger/index.html',
                '&#128279; View Run': os.environ.get('RUN_URL', '')} if is_success else \
               {'&#128279; View Failed Run': os.environ.get('RUN_URL', '')}
        details = {
            'title': title,
            'rows': {
                'Time': os.environ.get('COMMIT_TIME', ''),
                'Commit': os.environ.get('COMMIT_SHA', ''),
                'Author': f"{os.environ.get('AUTHOR_NAME','')} ({os.environ.get('AUTHOR_EMAIL','')})",
                'Merged by' if is_success else 'Triggered by': os.environ.get('ACTOR', ''),
                'Message': os.environ.get('COMMIT_MSG', ''),
            },
            'buttons': btns
        }

    html = build_html(status, details)
    send_email(subject, html)
