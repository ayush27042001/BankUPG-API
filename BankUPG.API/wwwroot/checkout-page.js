/* ==========================================================
   BankUPG Hosted Checkout Page — checkout-page.js
   cfg is read from <script type="application/json" id="cfg-data">.
   That block is never executed by the browser, so it does NOT
   require 'unsafe-inline' in script-src.
   Default logo: https://paymentgateway.banku.co.in/assets/images/bankulogo.png
========================================================== */

/* Read checkout session config from the inert JSON data block.
   This replaces the old inline <script>var cfg = ...</script>
   approach that required 'unsafe-inline' in script-src. */
var cfg = null, activeMd = null, selEmiMonths = 3;
(function () {
  try {
    var el = document.getElementById('cfg-data');
    if (el) cfg = JSON.parse(el.textContent);
  } catch (e) { cfg = null; }
})();

var DEFAULT_LOGO = 'https://paymentgateway.banku.co.in/assets/images/bankulogo.png';

/* ── SVG icons for payment methods ── */
var ICONS = {
  UPI: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>',
  Card: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="5" width="20" height="14" rx="3"/><line x1="2" y1="10" x2="22" y2="10"/></svg>',
  NetBanking: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11M8 10v11M12 10v11M16 10v11"/></svg>',
  Wallet: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H4a2 2 0 00-2 2v10a2 2 0 002 2h16a2 2 0 002-2V9a2 2 0 00-2-2z"/><path d="M16 14a1 1 0 100-2 1 1 0 000 2z" fill="currentColor" stroke="none"/><path d="M16 7V5a2 2 0 00-2-2H6a2 2 0 00-2 2v2"/></svg>',
  EMI: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/><path d="M8 14h2l1 2 2-4 1 2h2"/></svg>',
  PayLater: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>',
  _default: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="5" width="20" height="14" rx="3"/><line x1="2" y1="10" x2="22" y2="10"/></svg>'
};

/* ── Payment mode metadata ── */
var modeInfo = {
  UPI:        { lbl: 'UPI',          sub: 'GPay, PhonePe, Paytm & more' },
  Card:       { lbl: 'Cards',        sub: 'Credit & Debit — Visa, MC, RuPay' },
  NetBanking: { lbl: 'Net Banking',  sub: 'All major banks supported' },
  Wallet:     { lbl: 'Wallet',       sub: 'Paytm, PhonePe, MobiKwik & more' },
  EMI:        { lbl: 'EMI',          sub: 'Easy monthly instalments' },
  PayLater:   { lbl: 'Pay Later',    sub: 'Buy now, pay within 30 days' }
};

/* ── Chevron SVG ── */
var CHEV_SVG = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>';

/* ─────────────────────────────────────────────────────── */

/** Darken/lighten a hex colour (negative percent = darker). */
function shadeColor(hex, pct) {
  try {
    var h = hex.replace('#', '');
    if (h.length === 3) h = h[0]+h[0]+h[1]+h[1]+h[2]+h[2];
    var r = Math.min(255, Math.max(0, parseInt(h.slice(0,2), 16) + Math.round(255 * pct / 100)));
    var g = Math.min(255, Math.max(0, parseInt(h.slice(2,4), 16) + Math.round(255 * pct / 100)));
    var b = Math.min(255, Math.max(0, parseInt(h.slice(4,6), 16) + Math.round(255 * pct / 100)));
    return '#' + [r, g, b].map(function(x) { return x.toString(16).padStart(2, '0'); }).join('');
  } catch(e) { return hex; }
}

/** Convert #rrggbb to "r,g,b" string for CSS rgba(). */
function hexToRgb(hex) {
  try {
    var h = hex.replace('#', '');
    if (h.length === 3) h = h[0]+h[0]+h[1]+h[1]+h[2]+h[2];
    return [parseInt(h.slice(0,2),16), parseInt(h.slice(2,4),16), parseInt(h.slice(4,6),16)].join(',');
  } catch(e) { return '0,150,136'; }
}

/* ─────────────────────────────────────────────────────── */

function init() {
  if (!cfg) {
    var ol = document.getElementById('optList');
    if (ol) ol.innerHTML = '<div style="padding:32px 20px;text-align:center;color:#dc2626;font-size:13px;">Unable to load payment session.<br>Please go back and try again.</div>';
    return;
  }

  var pc  = cfg.primaryColor  || '#009688';
  var pcd = shadeColor(pc, -20);

  /* ── Apply primary colour throughout ── */
  document.documentElement.style.setProperty('--pc',      pc);
  document.documentElement.style.setProperty('--pc-dark', pcd);
  document.documentElement.style.setProperty('--pc-rgb',  hexToRgb(pc));

  /* ── Left panel gradient ── */
  var lp = document.querySelector('.pg-left');
  if (lp) lp.style.background = 'linear-gradient(160deg,' + pc + ' 0%,' + pcd + ' 100%)';

  /* ── Logo — use addEventListener instead of onerror attribute ── */
  var lw = document.getElementById('logoWrap');
  if (lw) {
    var logoSrc = cfg.logoUrl || DEFAULT_LOGO;
    var img = document.createElement('img');
    img.className = 'm-logo';
    img.alt = 'logo';
    img.src = logoSrc;
    img.addEventListener('error', function () {
      var ph = document.createElement('div');
      ph.className = 'm-logo-ph';
      ph.textContent = (cfg.merchantName || 'B').charAt(0).toUpperCase();
      img.parentNode && img.parentNode.replaceChild(ph, img);
    });
    lw.appendChild(img);
  }

  /* ── Merchant name ── */
  var mn = document.getElementById('mName');
  if (mn) mn.textContent = cfg.merchantName || 'BankUPG';

  /* ── Amount ── */
  var amtStr = (cfg.amountSymbol || '') + (cfg.amount || '');

  var amtEl = document.getElementById('amtVal');
  if (amtEl) amtEl.textContent = amtStr;

  /* ── Mobile amount card ── */
  var mobAmt = document.getElementById('mobAmtVal');
  if (mobAmt) mobAmt.textContent = amtStr;

  /* ── Order details (expanded section) ── */
  var ordDtl = document.getElementById('ordDtl');
  if (ordDtl) ordDtl.innerHTML = 'Ref: <strong>' + (cfg.orderRef || '') + '</strong>';

  /* ── Customer info ── */
  var ci = document.getElementById('custInfo');
  if (ci && (cfg.customerName || cfg.customerEmail)) {
    var ch = '';
    if (cfg.customerName)  ch += '<div class="cust-row"><span>&#x1F464;</span>' + cfg.customerName + '</div>';
    if (cfg.customerEmail) ch += '<div class="cust-row"><span>&#x2709;</span>'  + cfg.customerEmail + '</div>';
    if (cfg.customerPhone) ch += '<div class="cust-row"><span>&#x1F4F1;</span>' + cfg.customerPhone + '</div>';
    ci.innerHTML = ch;
  }

  /* ── Transaction label ── */
  var txnLbl = document.getElementById('txnLbl');
  if (txnLbl && cfg.orderId) txnLbl.textContent = 'Transaction Id : ' + cfg.orderId;

  /* ── Wire up static element event listeners (no inline onclick needed) ── */
  var amtChev = document.getElementById('amtChev');
  if (amtChev) amtChev.addEventListener('click', togDtl);

  var backBtn = document.getElementById('backBtn');
  if (backBtn) backBtn.addEventListener('click', handleBack);

  var retryBtn = document.getElementById('retryBtn');
  if (retryBtn) retryBtn.addEventListener('click', function () { location.reload(); });

  /* ── Build payment method list ── */
  var modes = (cfg.modes && cfg.modes.length > 0) ? cfg.modes : ['UPI', 'Card', 'NetBanking'];
  buildList(modes, pc);
}

/* ─────────────────────────────────────────────────────── */

function buildList(modes, pc) {
  var ol = document.getElementById('optList');
  if (!ol) return;
  ol.innerHTML = '';
  modes.forEach(function(m) {
    var mi  = modeInfo[m] || { lbl: m, sub: '' };
    var ico = ICONS[m]    || ICONS._default;
    var row = document.createElement('div');
    row.className = 'opt-row';
    row.id = 'row-' + m;
    row.innerHTML =
      '<div class="opt-hdr" onclick="togMode(\'' + m + '\')">' +
        '<div class="opt-info">' +
          '<div class="opt-ico-wrap">' + ico + '</div>' +
          '<div class="opt-meta">' +
            '<span class="opt-lbl">' + mi.lbl + '</span>' +
            (mi.sub ? '<span class="opt-sub">' + mi.sub + '</span>' : '') +
          '</div>' +
        '</div>' +
        '<span class="opt-chev">' + CHEV_SVG + '</span>' +
      '</div>' +
      '<div class="opt-form" id="form-' + m + '">' + buildForm(m, pc) + '</div>';
    ol.appendChild(row);
  });
}

function buildForm(m, pc) {
  var sym = cfg.amountSymbol || '', amt = cfg.amount || '', p = sym + amt;
  if (m === 'Card') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">Card Number</span>' +
    '<div class="cin-wrap"><input class="finp" type="text" id="cardNum" placeholder="0000 0000 0000 0000" maxlength="19" oninput="fmtCard(this)" autocomplete="cc-number">' +
    '<span class="cbrand" id="cbrand"></span></div></div>' +
    '<div class="fld"><span class="flbl">Name on Card</span>' +
    '<input class="finp" type="text" id="cardName" placeholder="JOHN SMITH" value="' + (cfg.customerName || '') + '" autocomplete="cc-name"></div>' +
    '<div class="frow2">' +
    '<div class="fld"><span class="flbl">Expiry (MM/YY)</span>' +
    '<input class="finp" type="text" id="cardExp" placeholder="MM / YY" maxlength="7" oninput="fmtExp(this)" autocomplete="cc-exp"></div>' +
    '<div class="fld"><span class="flbl">CVV</span>' +
    '<input class="finp" type="password" id="cardCvv" placeholder="•••" maxlength="4" autocomplete="cc-csc"></div></div>' +
    '<div class="err-box" id="cardErr"></div>' +
    '<button class="pay-btn" id="payCardBtn" onclick="payCard()" style="background:' + pc + '">Pay ' + p + '</button></div>';

  if (m === 'UPI') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">UPI ID / VPA</span>' +
    '<input class="finp" type="text" id="upiVpa" placeholder="name@okhdfc">' +
    '<span class="fhint">Enter your UPI ID linked to any bank account</span></div>' +
    '<div class="upi-logos"><span style="font-size:11px;color:#94a3b8">Pay via:</span>' +
    '<span class="upi-logo">GPay</span><span class="upi-logo">PhonePe</span>' +
    '<span class="upi-logo">Paytm</span><span class="upi-logo">BHIM</span></div>' +
    '<div class="err-box" id="upiErr"></div>' +
    '<button class="pay-btn" id="payUpiBtn" onclick="payUpi()" style="background:' + pc + '">Pay ' + p + '</button></div>';

  if (m === 'NetBanking') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">Select Your Bank</span>' +
    '<select class="fsel" id="bankCode"><option value="">-- Select Bank --</option>' +
    '<option value="HDFCBANK">HDFC Bank</option><option value="SBIN">State Bank of India</option>' +
    '<option value="ICICIBANK">ICICI Bank</option><option value="AXISBANK">Axis Bank</option>' +
    '<option value="KOTAKBANK">Kotak Mahindra Bank</option><option value="INDUSIND">IndusInd Bank</option>' +
    '<option value="YESBANK">Yes Bank</option><option value="PNBRETAIL">Punjab National Bank</option>' +
    '<option value="BOBIRETAIL">Bank of Baroda</option><option value="UNIONBANK">Union Bank of India</option>' +
    '</select></div>' +
    '<div class="err-box" id="nbErr"></div>' +
    '<button class="pay-btn" id="payNbBtn" onclick="payNb()" style="background:' + pc + '">Pay ' + p + '</button></div>';

  if (m === 'Wallet') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">Select Wallet</span>' +
    '<div class="wallet-grid">' +
    '<div class="w-item" onclick="payWallet(this,\'Paytm\')">Paytm</div>' +
    '<div class="w-item" onclick="payWallet(this,\'PhonePe\')">PhonePe</div>' +
    '<div class="w-item" onclick="payWallet(this,\'MobiKwik\')">MobiKwik</div>' +
    '<div class="w-item" onclick="payWallet(this,\'Airtel\')">Airtel</div>' +
    '<div class="w-item" onclick="payWallet(this,\'Ola\')">Ola Money</div>' +
    '<div class="w-item" onclick="payWallet(this,\'Amazon\')">Amazon Pay</div>' +
    '</div></div>' +
    '<div class="err-box" id="walletErr"></div></div>';

  if (m === 'EMI') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">Card Number</span>' +
    '<div class="cin-wrap"><input class="finp" type="text" id="emiCard" placeholder="0000 0000 0000 0000" maxlength="19" oninput="fmtCard(this)">' +
    '<span class="cbrand">💳</span></div></div>' +
    '<div class="fld"><span class="flbl">Select Tenure</span>' +
    '<div class="emi-grid">' +
    '<div class="emi-item sel" onclick="selEmiF(this,3)">3 Mo</div>' +
    '<div class="emi-item" onclick="selEmiF(this,6)">6 Mo</div>' +
    '<div class="emi-item" onclick="selEmiF(this,9)">9 Mo</div>' +
    '<div class="emi-item" onclick="selEmiF(this,12)">12 Mo</div>' +
    '</div></div>' +
    '<div class="err-box" id="emiErr"></div>' +
    '<button class="pay-btn" id="payEmiBtn" onclick="payEmi()" style="background:' + pc + '">Pay ' + p + ' via EMI</button></div>';

  if (m === 'PayLater') return '<div class="fb">' +
    '<div class="pl-box">' +
    '<div class="pl-ico">⏰</div>' +
    '<p>Pay now &amp; settle within 30 days with zero interest.</p>' +
    '<p class="pl-prov">Available via <strong>Simpl</strong>, <strong>LazyPay</strong>, <strong>ZestMoney</strong></p></div>' +
    '<div class="err-box" id="plErr"></div>' +
    '<button class="pay-btn" id="payPlBtn" onclick="payPL()" style="background:' + pc + '">Pay ' + p + ' via Pay Later</button></div>';

  return '<div class="fb"><div class="err-box" id="genErr"></div>' +
    '<button class="pay-btn" onclick="payGen(\'' + m + '\')" style="background:' + pc + '">Pay ' + p + '</button></div>';
}

/* ─────────────────────────────────────────────────────── */

function togDtl() {
  var d = document.getElementById('ordDtl');
  var c = document.getElementById('amtChev');
  var show = d.style.display !== 'block';
  d.style.display = show ? 'block' : 'none';
  if (c) c.style.transform = show ? 'rotate(180deg)' : '';
}

function togMode(m) {
  var row = document.getElementById('row-' + m);
  if (!row) return;
  var isOpen = row.classList.contains('open');
  if (activeMd) {
    var prev = document.getElementById('row-' + activeMd);
    if (prev) prev.classList.remove('open');
  }
  activeMd = isOpen ? null : m;
  row.classList.toggle('open', !isOpen);
}

function handleBack() {
  if (window.parent !== window) {
    /* Inside the BankUPG modal iframe — tell the SDK to close */
    window.parent.postMessage({ source: 'BankUPG', event: 'payment.dismiss' }, '*');
  } else {
    /* Standalone hosted checkout page — go to previous page */
    if (window.history.length > 1) {
      window.history.back();
    } else {
      window.close();
    }
  }
}

/* ─────────────────────────────────────────────────────── */

function setLoading(id, on, rst) {
  var b = document.getElementById(id);
  if (!b) return;
  b.disabled = on;
  if (on)       b.innerHTML = '<span class="spin"></span> Processing...';
  else if (rst) b.innerHTML = rst;
}

function showErr(id, msg) {
  var e = document.getElementById(id);
  if (e) { e.textContent = msg; e.classList.add('show'); }
}

function hideErr(id) {
  var e = document.getElementById(id);
  if (e) e.classList.remove('show');
}

async function doPay(payload, btnId, errId, rst) {
  hideErr(errId);
  setLoading(btnId, true, null);
  try {
    var r = await fetch('/api/checkout/pay', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    var j = await r.json();
    var d = j.data;
    if (j.success && d && d.success) {
      showResult(true, d.paymentId, d.paymentMode, null, d.redirectUrl, d.signature);
    } else {
      showErr(errId, (d && d.message) || j.message || 'Payment failed. Please try again.');
      setLoading(btnId, false, rst);
    }
  } catch(e) {
    showErr(errId, 'Network error. Please try again.');
    setLoading(btnId, false, rst);
  }
}

/* ─────────────────────────────────────────────────────── */

function payCard() {
  var num = document.getElementById('cardNum').value.replace(/\s/g, '');
  var nm  = document.getElementById('cardName').value.trim();
  var exp = document.getElementById('cardExp').value.trim();
  var cvv = document.getElementById('cardCvv').value.trim();
  if (num.length < 15) return showErr('cardErr', 'Please enter a valid card number.');
  if (!nm)             return showErr('cardErr', 'Please enter the name on card.');
  if (!/^\d{2}\s*\/\s*\d{2}$/.test(exp)) return showErr('cardErr', 'Please enter expiry as MM/YY.');
  if (cvv.length < 3)  return showErr('cardErr', 'Please enter a valid CVV.');
  var p = (cfg.amountSymbol || '') + cfg.amount;
  doPay({ checkoutToken: cfg.token, paymentMode: 'Card', cardNumber: num, cardName: nm, cardExpiry: exp, cardCvv: cvv }, 'payCardBtn', 'cardErr', 'Pay ' + p);
}

function payUpi() {
  var vpa = document.getElementById('upiVpa').value.trim();
  if (!vpa.includes('@')) return showErr('upiErr', 'Please enter a valid UPI ID (e.g. name@okhdfc).');
  var p = (cfg.amountSymbol || '') + cfg.amount;
  doPay({ checkoutToken: cfg.token, paymentMode: 'UPI', upiVpa: vpa }, 'payUpiBtn', 'upiErr', 'Pay ' + p);
}

function payNb() {
  var bk = document.getElementById('bankCode').value;
  if (!bk) return showErr('nbErr', 'Please select your bank.');
  var p = (cfg.amountSymbol || '') + cfg.amount;
  doPay({ checkoutToken: cfg.token, paymentMode: 'NetBanking', bankCode: bk }, 'payNbBtn', 'nbErr', 'Pay ' + p);
}

function payWallet(el, nm) {
  document.querySelectorAll('.w-item').forEach(function(w) { w.classList.remove('sel'); });
  el.classList.add('sel');
  doPay({ checkoutToken: cfg.token, paymentMode: 'Wallet', bankCode: nm }, null, 'walletErr', '');
}

function selEmiF(el, m) {
  document.querySelectorAll('.emi-item').forEach(function(e) { e.classList.remove('sel'); });
  el.classList.add('sel');
  selEmiMonths = m;
}

function payEmi() {
  var num = document.getElementById('emiCard').value.replace(/\s/g, '');
  if (num.length < 15) return showErr('emiErr', 'Please enter a valid card number.');
  var p = (cfg.amountSymbol || '') + cfg.amount + ' via EMI';
  doPay({ checkoutToken: cfg.token, paymentMode: 'EMI', cardNumber: num, emiTenure: selEmiMonths }, 'payEmiBtn', 'emiErr', 'Pay ' + p);
}

function payPL() {
  var p = (cfg.amountSymbol || '') + cfg.amount + ' via Pay Later';
  doPay({ checkoutToken: cfg.token, paymentMode: 'PayLater' }, 'payPlBtn', 'plErr', 'Pay ' + p);
}

function payGen(m) {
  doPay({ checkoutToken: cfg.token, paymentMode: m }, null, 'genErr', '');
}

/* ─────────────────────────────────────────────────────── */

function fmtCard(el) {
  var v = el.value.replace(/\D/g, '').substring(0, 16);
  el.value = v.match(/.{1,4}/g) ? v.match(/.{1,4}/g).join(' ') : v;
  var b = document.getElementById('cbrand');
  if (b) {
    if (v.startsWith('4'))                              b.textContent = 'VISA';
    else if (v.startsWith('5'))                         b.textContent = 'MC';
    else if (v.startsWith('34') || v.startsWith('37')) b.textContent = 'AMEX';
    else if (v.startsWith('6'))                         b.textContent = 'RUPAY';
    else                                                b.textContent = '';
  }
}

function fmtExp(el) {
  var v = el.value.replace(/\D/g, '');
  if (v.length >= 2) v = v.substring(0, 2) + ' / ' + v.substring(2, 4);
  el.value = v;
}

/* ─────────────────────────────────────────────────────── */

function showResult(ok, pid, mode, msg, rdUrl, sig) {
  var ol    = document.getElementById('optList');
  var lbl   = document.querySelector('.opt-label');
  var hdr   = document.querySelector('.pg-hdr');
  var ov    = document.getElementById('resOv');
  if (ol)  ol.style.display  = 'none';
  if (lbl) lbl.style.display = 'none';
  if (hdr) hdr.style.display = 'none';
  if (ov)  ov.style.display  = 'flex';

  var pc = (cfg && cfg.primaryColor) ? cfg.primaryColor : '#009688';

  var ico    = document.getElementById('resIco');
  var ttl    = document.getElementById('resTtl');
  var amt    = document.getElementById('resAmt');
  var sub    = document.getElementById('resSub');
  var idEl   = document.getElementById('resId');
  var retryBtn = document.getElementById('retryBtn');

  if (ico) { ico.className = 'res-ico ' + (ok ? 'ok' : 'fail'); ico.textContent = ok ? '✓' : '✕'; }
  if (ttl) ttl.textContent = ok ? 'Payment Successful!' : 'Payment Failed';
  if (amt) { amt.textContent = ok ? ((cfg.amountSymbol || '') + cfg.amount) : ''; amt.style.color = pc; }
  if (sub) sub.textContent = ok ? ('via ' + mode) : (msg || 'Please try another method.');
  if (idEl)    { idEl.style.display = (ok && pid) ? 'block' : 'none'; if (ok && pid) idEl.textContent = 'Payment ID: ' + pid; }
  if (retryBtn) retryBtn.style.display = ok ? 'none' : 'inline-block';

  if (ok) {
    if (window.parent !== window) {
      /* Inside modal iframe — notify the SDK via postMessage; SDK will close the modal */
      window.parent.postMessage({
        source: 'BankUPG', event: 'payment.success',
        payment_id:   pid,
        order_id:     cfg.orderId,
        signature:    sig,
        amount:       cfg.amount,
        payment_mode: mode,
        paid_at:      new Date().toISOString()
      }, '*');
    } else {
      /* Standalone page — redirect to callbackUrl after a short delay */
      setTimeout(function() { if (rdUrl) window.location.href = rdUrl; }, 2500);
    }
  }
}

/* ─────────────────────────────────────────────────────── */

window.addEventListener('DOMContentLoaded', init);
