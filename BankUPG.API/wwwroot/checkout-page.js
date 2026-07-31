/* ==========================================================
   BankUPG Hosted Checkout Page — checkout-page.js
   All runtime logic. cfg is defined by the inline config
   script injected by the server before this file loads.
========================================================== */

var modeInfo = {
  'UPI':        { lbl: 'UPI',                  sub: 'GPay, PhonePe, Paytm & more', ico: '⚡', bg: '#fff9e6' },
  'Card':       { lbl: 'Credit / Debit Card',  sub: 'Visa, Mastercard, RuPay',     ico: '💳', bg: '#e3f2fd' },
  'NetBanking': { lbl: 'Net Banking',           sub: 'All major banks supported',   ico: '🏦', bg: '#e8f5e9' },
  'Wallet':     { lbl: 'Wallet',                sub: 'Paytm, PhonePe, MobiKwik',    ico: '👛', bg: '#fce4ec' },
  'EMI':        { lbl: 'EMI',                   sub: 'Easy monthly installments',   ico: '📅', bg: '#ede7f6' },
  'PayLater':   { lbl: 'Pay Later',             sub: 'Buy now, pay in 30 days',     ico: '⏰', bg: '#e0f7fa' }
};

function init() {
  if (!cfg) {
    var ol = document.getElementById('optList');
    if (ol) ol.innerHTML = '<div style="padding:24px 20px;text-align:center;color:#d32f2f;font-size:13px;">Unable to load payment session.<br>Please go back and try again.</div>';
    return;
  }
  var modes = (cfg.modes && cfg.modes.length > 0) ? cfg.modes : ['UPI', 'Card', 'NetBanking'];
  var pc = cfg.primaryColor || '#009688';

  var mName = document.getElementById('mName');
  if (mName) mName.textContent = cfg.merchantName || 'BankUPG';

  var lw = document.getElementById('logoWrap');
  if (lw) {
    if (cfg.logoUrl) {
      lw.innerHTML = '<img class="m-logo" src="' + cfg.logoUrl + '" alt="logo">';
    } else {
      lw.innerHTML = '<div class="m-logo-ph">' + (cfg.merchantName || 'B').charAt(0).toUpperCase() + '</div>';
    }
  }

  var amtEl = document.getElementById('amtVal');
  if (amtEl) amtEl.innerHTML = (cfg.amountSymbol || '') + (cfg.amount || '');

  var ordDtl = document.getElementById('ordDtl');
  if (ordDtl) ordDtl.innerHTML = 'Order Ref: <strong>' + (cfg.orderRef || '') + '</strong>';

  var ci = document.getElementById('custInfo');
  if (ci && (cfg.customerName || cfg.customerEmail)) {
    var ch = '';
    if (cfg.customerName)  ch += '<div class="cust-row"><span>&#x1F464;</span>' + cfg.customerName + '</div>';
    if (cfg.customerEmail) ch += '<div class="cust-row"><span>&#x2709;</span>'  + cfg.customerEmail + '</div>';
    if (cfg.customerPhone) ch += '<div class="cust-row"><span>&#x1F4F1;</span>' + cfg.customerPhone + '</div>';
    ci.innerHTML = ch;
  }

  var txnLbl = document.getElementById('txnLbl');
  if (txnLbl) txnLbl.textContent = 'Ref: ' + (cfg.orderRef || '');

  buildList(modes, pc);
}

function buildList(modes, pc) {
  var ol = document.getElementById('optList');
  if (!ol) return;
  ol.innerHTML = '';
  modes.forEach(function(m) {
    var mi = modeInfo[m] || { lbl: m, sub: '', ico: '💰', bg: '#f5f5f5' };
    var row = document.createElement('div');
    row.className = 'opt-row';
    row.id = 'row-' + m;
    row.innerHTML =
      '<div class="opt-hdr" onclick="togMode(\'' + m + '\')">' +
        '<div class="opt-info">' +
          '<div class="opt-ico-wrap" style="background:' + mi.bg + '">' + mi.ico + '</div>' +
          '<div class="opt-meta">' +
            '<span class="opt-lbl">' + mi.lbl + '</span>' +
            '<span class="opt-sub">' + mi.sub + '</span>' +
          '</div>' +
        '</div>' +
        '<span class="opt-chev" id="chev-' + m + '">&#9658;</span>' +
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
    '<span class="cbrand" id="cbrand">💳</span></div></div>' +
    '<div class="fld"><span class="flbl">Name on Card</span>' +
    '<input class="finp" type="text" id="cardName" placeholder="JOHN SMITH" value="' + (cfg.customerName || '') + '" autocomplete="cc-name"></div>' +
    '<div class="frow2">' +
    '<div class="fld"><span class="flbl">Expiry (MM/YY)</span>' +
    '<input class="finp" type="text" id="cardExp" placeholder="MM / YY" maxlength="7" oninput="fmtExp(this)" autocomplete="cc-exp"></div>' +
    '<div class="fld"><span class="flbl">CVV</span>' +
    '<input class="finp" type="password" id="cardCvv" placeholder="&#x2022;&#x2022;&#x2022;" maxlength="4" autocomplete="cc-csc"></div></div>' +
    '<div class="err-box" id="cardErr"></div>' +
    '<button class="pay-btn" id="payCardBtn" onclick="payCard()" style="background:' + pc + '">Pay ' + p + '</button></div>';
  if (m === 'UPI') return '<div class="fb">' +
    '<div class="fld"><span class="flbl">UPI ID / VPA</span>' +
    '<input class="finp" type="text" id="upiVpa" placeholder="name@okhdfc">' +
    '<span class="fhint">Enter your UPI ID linked to any bank</span></div>' +
    '<div class="upi-logos"><span style="font-size:11px;color:#aaa">Pay via:</span>' +
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
    '<div class="pl-box"><div class="pl-ico">&#x23F0;</div>' +
    '<p>Pay now &amp; settle within 30 days with zero interest.</p>' +
    '<p class="pl-prov">Available via <strong>Simpl</strong>, <strong>LazyPay</strong>, <strong>ZestMoney</strong></p></div>' +
    '<div class="err-box" id="plErr"></div>' +
    '<button class="pay-btn" id="payPlBtn" onclick="payPL()" style="background:' + pc + '">Pay ' + p + ' via Pay Later</button></div>';
  return '<div class="fb"><button class="pay-btn" onclick="payGen(\'' + m + '\')" style="background:' + pc + '">Pay ' + p + '</button></div>';
}

function togDtl() {
  var d = document.getElementById('ordDtl'), c = document.getElementById('amtChev');
  var show = d.style.display !== 'block';
  d.style.display = show ? 'block' : 'none';
  if (c) c.style.transform = show ? 'rotate(180deg)' : '';
}

function togMode(m) {
  var row = document.getElementById('row-' + m);
  if (!row) return;
  var isOpen = row.classList.contains('open');
  if (activeMd) { var prev = document.getElementById('row-' + activeMd); if (prev) prev.classList.remove('open'); }
  activeMd = isOpen ? null : m;
  row.classList.toggle('open', !isOpen);
}

function handleBack() {
  if (window.parent !== window) { window.parent.postMessage({ source: 'BankUPG', event: 'payment.dismiss' }, '*'); }
  else { history.back(); }
}

function setLoading(id, on, rst) {
  var b = document.getElementById(id); if (!b) return;
  b.disabled = on;
  if (on) b.innerHTML = '<span class="spin"></span> Processing...';
  else if (rst) b.innerHTML = rst;
}

function showErr(id, msg) { var e = document.getElementById(id); if (e) { e.textContent = msg; e.classList.add('show'); } }
function hideErr(id) { var e = document.getElementById(id); if (e) e.classList.remove('show'); }

async function doPay(payload, btnId, errId, rst) {
  hideErr(errId); setLoading(btnId, true, null);
  try {
    var r = await fetch('/api/checkout/pay', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
    var j = await r.json(), d = j.data;
    if (j.success && d && d.success) { showResult(true, d.paymentId, d.paymentMode, null, d.redirectUrl, d.signature); }
    else { showErr(errId, (d && d.message) || j.message || 'Payment failed. Please try again.'); setLoading(btnId, false, rst); }
  } catch(e) { showErr(errId, 'Network error. Please try again.'); setLoading(btnId, false, rst); }
}

function payCard() {
  var num = document.getElementById('cardNum').value.replace(/\s/g,'');
  var nm  = document.getElementById('cardName').value.trim();
  var exp = document.getElementById('cardExp').value.trim();
  var cvv = document.getElementById('cardCvv').value.trim();
  if (num.length < 15) return showErr('cardErr', 'Please enter a valid card number.');
  if (!nm) return showErr('cardErr', 'Please enter the name on card.');
  if (!/^\d{2}\s*\/\s*\d{2}$/.test(exp)) return showErr('cardErr', 'Please enter expiry as MM/YY.');
  if (cvv.length < 3) return showErr('cardErr', 'Please enter a valid CVV.');
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
function selEmiF(el, m) { document.querySelectorAll('.emi-item').forEach(function(e) { e.classList.remove('sel'); }); el.classList.add('sel'); selEmiMonths = m; }
function payEmi() {
  var num = document.getElementById('emiCard').value.replace(/\s/g,'');
  if (num.length < 15) return showErr('emiErr', 'Please enter a valid card number.');
  var p = (cfg.amountSymbol || '') + cfg.amount + ' via EMI';
  doPay({ checkoutToken: cfg.token, paymentMode: 'EMI', cardNumber: num, emiTenure: selEmiMonths }, 'payEmiBtn', 'emiErr', 'Pay ' + p);
}
function payPL() {
  var p = (cfg.amountSymbol || '') + cfg.amount + ' via Pay Later';
  doPay({ checkoutToken: cfg.token, paymentMode: 'PayLater' }, 'payPlBtn', 'plErr', 'Pay ' + p);
}
function payGen(m) { doPay({ checkoutToken: cfg.token, paymentMode: m }, null, 'genErr', ''); }

function fmtCard(el) {
  var v = el.value.replace(/\D/g,'').substring(0,16);
  el.value = v.match(/.{1,4}/g) ? v.match(/.{1,4}/g).join(' ') : v;
  var b = document.getElementById('cbrand');
  if (b) {
    if (v.startsWith('4'))                              b.textContent = 'Visa';
    else if (v.startsWith('5'))                         b.textContent = 'MC';
    else if (v.startsWith('34') || v.startsWith('37')) b.textContent = 'Amex';
    else if (v.startsWith('6'))                         b.textContent = 'RuPay';
    else                                                b.textContent = '💳';
  }
}
function fmtExp(el) { var v = el.value.replace(/\D/g,''); if (v.length >= 2) v = v.substring(0,2) + ' / ' + v.substring(2,4); el.value = v; }

function showResult(ok, pid, mode, msg, rdUrl, sig) {
  var ol = document.getElementById('optList'), lbl = document.querySelector('.opt-label'), ov = document.getElementById('resOv');
  if (ol) ol.style.display = 'none';
  if (lbl) lbl.style.display = 'none';
  if (ov) ov.style.display = 'flex';
  var pc = (cfg && cfg.primaryColor) ? cfg.primaryColor : '#009688';
  var ico = document.getElementById('resIco'), ttl = document.getElementById('resTtl');
  var amt = document.getElementById('resAmt'), sub = document.getElementById('resSub');
  var idEl = document.getElementById('resId'), retryBtn = document.getElementById('retryBtn');
  if (ico) { ico.className = 'res-ico ' + (ok ? 'ok' : 'fail'); ico.textContent = ok ? '\u2713' : '\u2715'; }
  if (ttl) ttl.textContent = ok ? 'Payment Successful!' : 'Payment Failed';
  if (amt) { amt.textContent = ok ? ((cfg.amountSymbol || '') + cfg.amount) : ''; amt.style.color = pc; }
  if (sub) sub.textContent = ok ? ('via ' + mode) : (msg || 'Please try another method.');
  if (idEl) { idEl.style.display = (ok && pid) ? 'block' : 'none'; if (ok && pid) idEl.textContent = 'Payment ID: ' + pid; }
  if (retryBtn) retryBtn.style.display = ok ? 'none' : 'inline-block';
  if (ok) {
    if (window.parent !== window) {
      // Inside modal iframe — notify the SDK; SDK closes the modal.
      // Do NOT redirect window.top here (that would hijack the merchant's page).
      window.parent.postMessage({
        source: 'BankUPG', event: 'payment.success',
        payment_id: pid, order_id: cfg.orderId, signature: sig,
        amount: cfg.amount, payment_mode: mode, paid_at: new Date().toISOString()
      }, '*');
    } else {
      // Standalone hosted page (no SDK modal) — redirect to callbackUrl.
      setTimeout(function() { if (rdUrl) window.location.href = rdUrl; }, 2500);
    }
  }
}

window.addEventListener('DOMContentLoaded', init);
