/* ==========================================================
   BankUPG Hosted Checkout — checkout-page.js  v6
   Premium redesign. Fully dynamic: reads all merchant
   branding, amount, and enabled payment modes from the
   server-injected JSON config block.

   Config injected by server:
     cfg.token          — session token
     cfg.amount         — formatted amount string (e.g. "499.00")
     cfg.amountSymbol   — currency symbol (e.g. "₹")
     cfg.orderId        — order ID
     cfg.orderRef       — merchant's own reference
     cfg.merchantName   — merchant display name
     cfg.logoUrl        — merchant logo URL
     cfg.primaryColor   — hex brand color
     cfg.secondaryColor — hex secondary color
     cfg.customerName   / cfg.customerEmail / cfg.customerPhone
     cfg.modes          — array of enabled payment modes
========================================================== */

/* ── Read config ── */
var cfg = null;
(function () {
  try {
    var el = document.getElementById('cfg-data');
    if (el) cfg = JSON.parse(el.textContent || el.innerHTML);
  } catch (e) { cfg = null; }
})();

/* ── Helpers ── */
function esc(s) {
  return String(s || '')
    .replace(/&/g,'&amp;').replace(/</g,'&lt;')
    .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function hex2rgb(hex) {
  try {
    var h = hex.replace('#','');
    if (h.length === 3) h = h[0]+h[0]+h[1]+h[1]+h[2]+h[2];
    return [parseInt(h.slice(0,2),16), parseInt(h.slice(2,4),16), parseInt(h.slice(4,6),16)].join(',');
  } catch(e) { return '37,99,235'; }
}

function shade(hex, pct) {
  try {
    var h = hex.replace('#','');
    if (h.length === 3) h = h[0]+h[0]+h[1]+h[1]+h[2]+h[2];
    var r = Math.min(255, Math.max(0, parseInt(h.slice(0,2),16) + Math.round(255*pct/100)));
    var g = Math.min(255, Math.max(0, parseInt(h.slice(2,4),16) + Math.round(255*pct/100)));
    var b = Math.min(255, Math.max(0, parseInt(h.slice(4,6),16) + Math.round(255*pct/100)));
    return '#' + [r,g,b].map(function(x){ return x.toString(16).padStart(2,'0'); }).join('');
  } catch(e) { return hex; }
}

/* ── Apply merchant brand via CSS custom properties ── */
function applyBrand(pc, sc) {
  var rgb = hex2rgb(pc);
  var old = document.getElementById('brand-vars');
  if (old) old.remove();
  var s = document.createElement('style');
  s.id = 'brand-vars';
  s.textContent = ':root{--pc:' + pc + ';--pc-dark:' + shade(pc,-18) + ';--sc:' + sc + ';--pc-rgb:' + rgb + '}';
  document.head.appendChild(s);
}

/* ── SVG Icons ── */
var ICONS = {
  upi:        '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>',
  debitCard:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="5" width="20" height="14" rx="3"/><line x1="2" y1="10" x2="22" y2="10"/><circle cx="7" cy="15" r="1" fill="currentColor" stroke="none"/></svg>',
  creditCard: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="5" width="20" height="14" rx="3"/><line x1="2" y1="10" x2="22" y2="10"/><rect x="14" y="13" width="4" height="2" rx="0.5" fill="currentColor" stroke="none"/></svg>',
  netBanking: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11M8 10v11M12 10v11M16 10v11"/></svg>',
  wallet:     '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H4a2 2 0 00-2 2v10a2 2 0 002 2h16a2 2 0 002-2V9a2 2 0 00-2-2z"/><circle cx="16" cy="13" r="1" fill="currentColor" stroke="none"/><path d="M16 7V5a2 2 0 00-2-2H6a2 2 0 00-2 2v2"/></svg>',
  payLater:   '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>',
  emi:        '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/><path d="M8 14h2l1 2 2-4 1 2h2"/></svg>',
  qr:         '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><path d="M14 14h2v2h-2zM18 14h2v2h-2zM14 18h2v2h-2zM18 18h2v2h-2z"/></svg>',
  shield:     '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>',
  check:      '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>',
  close:      '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>',
  lock:       '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>',
  chev:       '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>'
};

/* ── Payment method metadata ──
   Keys here must match exactly the PaymentMethodType values in the DB.
   Only modes listed in cfg.modes (from MerchantPaymentMethods) are shown. ── */
var PM_META = {
  UPI:        { key:'upi',        lbl:'UPI',         sub:'GPay, PhonePe, Paytm & more',  icon:'upi' },
  Card:       { key:'card',       lbl:'Card',         sub:'Debit & Credit Cards',          icon:'debitCard' },
  NetBanking: { key:'netBanking', lbl:'Net Banking',  sub:'All major banks',               icon:'netBanking' },
  Wallet:     { key:'wallet',     lbl:'Wallets',      sub:'Paytm, PhonePe, MobiKwik',      icon:'wallet' },
  EMI:        { key:'emi',        lbl:'EMI',          sub:'Easy monthly instalments',      icon:'emi' },
  PayLater:   { key:'payLater',   lbl:'Pay Later',    sub:'Buy now, pay within 30 days',   icon:'payLater' }
};

/* UPI apps */
var UPI_APPS = [
  { id:'gpay',     name:'Google Pay',   color:'#4285F4', init:'G' },
  { id:'phonepe',  name:'PhonePe',      color:'#5f259f', init:'P' },
  { id:'paytm',    name:'Paytm',        color:'#00BAF2', init:'Pa'},
  { id:'bhim',     name:'BHIM',         color:'#00529C', init:'B' },
  { id:'amazon',   name:'Amazon Pay',   color:'#FF9900', init:'A', dark:true },
  { id:'whatsapp', name:'WhatsApp Pay', color:'#25D366', init:'W' }
];

/* Popular banks */
var BANKS = [
  { id:'sbi',   name:'SBI',   color:'#1565C0' },
  { id:'hdfc',  name:'HDFC',  color:'#004B87' },
  { id:'icici', name:'ICICI', color:'#F58220' },
  { id:'axis',  name:'Axis',  color:'#97144D' },
  { id:'kotak', name:'Kotak', color:'#E31837' },
  { id:'bob',   name:'BoB',   color:'#F05A22' }
];

/* Wallets */
var WALLETS = [
  { id:'paytm',    name:'Paytm',      color:'#00BAF2', init:'Pa'},
  { id:'phonepe',  name:'PhonePe',    color:'#5f259f', init:'P' },
  { id:'mobikwik', name:'MobiKwik',   color:'#1A237E', init:'M' },
  { id:'amazon',   name:'Amazon Pay', color:'#FF9900', init:'A', dark:true },
  { id:'airtel',   name:'Airtel Pay', color:'#ED1C24', init:'Ai'},
  { id:'jio',      name:'JioMoney',   color:'#0C2E7E', init:'J' }
];

/* ── State ── */
var activeMethod = 'upi';
var selectedUpiApp = null;
var selectedBank   = null;
var selectedWallet = null;
var selEmiMonths   = 3;
var showCvv        = false;
var amountStr      = '';
var payBtn         = null;

/* ═══════════════════════════════════════
   INIT
═══════════════════════════════════════ */
function init() {

  if (!cfg) {
    document.body.innerHTML =
      '<div class="ck-error-page">' +
        '<div class="ck-error-icon">' + ICONS.close.replace('stroke="currentColor"','stroke="#EF4444"') + '</div>' +
        '<div class="ck-error-title">Unable to Load</div>' +
        '<div class="ck-error-msg">Invalid payment session. Please go back and try again.</div>' +
      '</div>';
    return;
  }

  /* Brand colors */
  var pc = cfg.primaryColor  || '#2563EB';
  var sc = cfg.secondaryColor || '#7C3AED';
  applyBrand(pc, sc);

  /* Amount string */
  var sym  = cfg.amountSymbol || '₹';
  var amt  = cfg.amount || '0.00';
  amountStr = sym + amt;

  /* ── Header: Logo ── */
  var logoWrap = document.getElementById('logoWrap');
  if (logoWrap) {
    if (cfg.logoUrl && cfg.logoUrl.trim()) {
      var img = document.createElement('img');
      img.className = 'ck-logo';
      img.alt = esc(cfg.merchantName || 'Logo');
      img.src = cfg.logoUrl;
      img.onerror = function () {
        var ph = buildLogoPh(cfg.merchantName || 'B');
        logoWrap.replaceChild(ph, img);
      };
      logoWrap.appendChild(img);
    } else {
      logoWrap.appendChild(buildLogoPh(cfg.merchantName || 'B'));
    }
  }

  /* Merchant name */
  var mNameEl = document.getElementById('mName');
  if (mNameEl) mNameEl.textContent = cfg.merchantName || 'BankU Pay';

  /* Paying-to */
  var payingToEl = document.getElementById('payingTo');
  if (payingToEl) payingToEl.textContent = cfg.merchantName || 'BankU Technologies';

  /* Amount */
  var amtEl = document.getElementById('amtVal');
  if (amtEl) amtEl.textContent = amountStr;

  /* Pay button text */
  payBtn = document.getElementById('payBtn');
  if (payBtn) {
    payBtn.innerHTML =
      '<span class="btn-lock">' + ICONS.lock + '</span>' +
      '<span class="btn-lbl">Pay ' + esc(amountStr) + '</span>';
    payBtn.addEventListener('click', handlePay);
  }

  /* Close button */
  var closeEl = document.getElementById('closeBtn');
  if (closeEl) {
    closeEl.addEventListener('click', function () {
      window.parent.postMessage({ source: 'BankUPG', event: 'payment.dismiss' }, '*');
    });
  }

  /* Build payment method list */
  buildMethodList();

  /* Show default panel */
  showPanel(activeMethod);
}

/* ── Logo placeholder ── */
function buildLogoPh(name) {
  var div = document.createElement('div');
  div.className = 'ck-logo-ph';
  div.textContent = (name || 'B').charAt(0).toUpperCase();
  return div;
}

/* ═══════════════════════════════════════
   BUILD PAYMENT METHOD LIST (Left panel)
═══════════════════════════════════════ */
function buildMethodList() {
  var container = document.getElementById('pmList');
  if (!container) return;

  var inner = document.createElement('div');
  inner.className = 'pm-list-inner';
  container.appendChild(inner);

  /* Build ordered list from cfg.modes + deduplication */
  var methods = [];
  var seen = {};

  if (cfg.modes && cfg.modes.length > 0) {
    cfg.modes.forEach(function (mode) {
      var meta = PM_META[mode];
      if (meta && !seen[meta.key]) {
        methods.push(meta);
        seen[meta.key] = true;
      }
    });
  } else {
    /* Fallback: show all when no modes configured */
    Object.keys(PM_META).forEach(function (k) {
      var m = PM_META[k];
      if (!seen[m.key]) { methods.push(m); seen[m.key] = true; }
    });
  }

  /* Set first available method as active */
  if (methods.length > 0) activeMethod = methods[0].key;

  methods.forEach(function (m) {
    inner.appendChild(buildMethodItem(m));
  });
}

function buildMethodItem(m) {
  var btn = document.createElement('button');
  btn.className = 'pm-item' + (m.key === activeMethod ? ' active' : '');
  btn.dataset.key = m.key;
  btn.innerHTML =
    '<div class="pm-ico">' + (ICONS[m.icon] || ICONS.upi) + '</div>' +
    '<div class="pm-text">' +
      '<span class="pm-lbl">' + esc(m.lbl) + '</span>' +
      '<span class="pm-sub">' + esc(m.sub) + '</span>' +
    '</div>' +
    '<div class="pm-chev">' + ICONS.chev + '</div>' +
    '<span class="pm-mobile-lbl">' + esc(m.lbl) + '</span>';

  btn.addEventListener('click', function () {
    var all = document.querySelectorAll('.pm-item');
    all.forEach(function (el) { el.classList.remove('active'); });
    btn.classList.add('active');
    activeMethod = m.key;
    selectedUpiApp = null;
    selectedBank   = null;
    selectedWallet = null;
    showPanel(m.key);
    resetPayBtn();
  });

  return btn;
}

/* ═══════════════════════════════════════
   PAYMENT PANELS (Right panel)
═══════════════════════════════════════ */
function showPanel(key) {
  var panel = document.getElementById('pmPanel');
  if (!panel) return;

  var html = '';

  switch (key) {
    case 'upi':        html = buildUpiPanel();       break;
    case 'card':       html = buildCardPanel();      break;
    case 'debitCard':  html = buildCardPanel();      break;
    case 'creditCard': html = buildCardPanel();      break;
    case 'netBanking': html = buildNetBankingPanel(); break;
    case 'wallet':     html = buildWalletPanel();    break;
    case 'payLater':   html = buildPayLaterPanel();  break;
    case 'emi':        html = buildEmiPanel();         break;
    case 'qr':         html = buildQrPanel();          break;
    default:           html = buildUpiPanel();
  }

  panel.innerHTML = html;
  panel.style.animation = 'none';
  panel.offsetHeight; /* reflow */
  panel.style.animation = 'panelIn 0.22s ease-out';

  bindPanelEvents(key);
}

/* ── UPI Panel ── */
function buildUpiPanel() {
  var appCards = UPI_APPS.map(function (a) {
    return '<button class="app-card" data-appid="' + esc(a.id) + '">' +
      '<div class="app-logo" style="background:' + esc(a.color) + ';color:' + (a.dark ? '#111' : '#fff') + '">' + esc(a.init) + '</div>' +
      '<span class="app-name">' + esc(a.name) + '</span>' +
    '</button>';
  }).join('');

  return '<p class="panel-title">Pay using UPI</p>' +
    '<p class="panel-sub">Enter your UPI ID or choose an app</p>' +
    '<div class="fl-field" id="upiField">' +
      '<label>UPI ID</label>' +
      '<div class="fl-inner">' +
        '<div class="fl-icon">' + ICONS.upi + '</div>' +
        '<input type="text" class="fl-input" id="upiInput" placeholder="yourname@upi" autocomplete="off" autocapitalize="off" />' +
        '<div class="fl-valid" id="upiValid" style="display:none">' +
          '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#22C55E" stroke-width="3" stroke-linecap="round"><path d="M20 6L9 17l-5-5"/></svg>' +
        '</div>' +
      '</div>' +
    '</div>' +
    '<div class="divider"><span class="divider-txt">OR</span></div>' +
    '<p class="app-grid-label">Choose a UPI app</p>' +
    '<div class="app-grid" id="upiApps">' + appCards + '</div>';
}

/* ── Card Panel ── */
function buildCardPanel() {
  var networks =
    '<div class="card-net active" data-net="visa">VISA</div>' +
    '<div class="card-net" data-net="mc"><span style="color:#EB001B;font-weight:800">M</span><span style="color:#F79E1B;font-weight:800">C</span></div>' +
    '<div class="card-net" data-net="rupay">RuPay</div>' +
    '<div class="card-net" data-net="amex">AMEX</div>';

  return '<p class="panel-title">Pay by Card</p>' +
    '<p class="panel-sub">Debit & Credit cards — Visa, Mastercard, RuPay, Amex</p>' +
    '<div class="card-networks">' + networks + '</div>' +

    buildFl('cardNumber', 'Card Number', ICONS.debitCard, 'text',  'cc-number', '0000 0000 0000 0000', 19) +
    buildFl('cardHolder', 'Card Holder Name', ICONS.lock, 'text', 'cc-name',   'Name on card', 80) +

    '<div class="fl-row2">' +
      buildFl('expiry',  'Expiry Date', null, 'text', 'cc-exp', 'MM / YY', 5) +
      '<div class="fl-field" id="cvvField">' +
        '<label>CVV</label>' +
        '<div class="fl-inner">' +
          '<div class="fl-icon">' + ICONS.lock + '</div>' +
          '<input type="password" class="fl-input" id="cvvInput" placeholder="•••" maxlength="4" autocomplete="cc-csc" />' +
          '<button type="button" class="cvv-toggle" id="cvvToggle">' +
            '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>' +
          '</button>' +
        '</div>' +
      '</div>' +
    '</div>' +

    '<label class="save-card-row">' +
      '<input type="checkbox" id="saveCard" />' +
      '<span>Securely save card for future payments</span>' +
    '</label>';
}

function buildFl(id, labelTxt, iconSvg, type, autoComplete, placeholder, maxLen) {
  var icon = iconSvg ? '<div class="fl-icon">' + iconSvg + '</div>' : '';
  return '<div class="fl-field" id="' + id + 'Field">' +
    '<label>' + esc(labelTxt) + '</label>' +
    '<div class="fl-inner">' +
      icon +
      '<input type="' + type + '" class="fl-input" id="' + id + 'Input"' +
        ' placeholder="' + esc(placeholder) + '"' +
        (maxLen ? ' maxlength="' + maxLen + '"' : '') +
        (autoComplete ? ' autocomplete="' + autoComplete + '"' : '') +
      ' />' +
    '</div>' +
  '</div>';
}

/* ── Net Banking Panel ── */
function buildNetBankingPanel() {
  var bankCards = BANKS.map(function (b) {
    return '<button class="app-card" data-bankid="' + esc(b.id) + '">' +
      '<div class="app-logo" style="background:' + esc(b.color) + '">' + esc(b.name.charAt(0)) + '</div>' +
      '<span class="app-name">' + esc(b.name) + '</span>' +
    '</button>';
  }).join('');

  return '<p class="panel-title">Net Banking</p>' +
    '<p class="panel-sub">All major Indian banks supported</p>' +
    '<p class="app-grid-label">Popular Banks</p>' +
    '<div class="app-grid" id="bankGrid">' + bankCards + '</div>' +
    '<p class="app-grid-label" style="margin-top:16px">Other Banks</p>' +
    '<div class="search-wrap">' +
      '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>' +
      '<input type="text" class="search-input" id="bankSearch" placeholder="Search bank name..." />' +
    '</div>';
}

/* ── Wallet Panel ── */
function buildWalletPanel() {
  var cards = WALLETS.map(function (w) {
    return '<button class="wallet-card" data-wid="' + esc(w.id) + '">' +
      '<div class="app-logo" style="background:' + esc(w.color) + ';color:' + (w.dark ? '#111' : '#fff') + '">' + esc(w.init) + '</div>' +
      '<span class="pm-lbl">' + esc(w.name) + '</span>' +
    '</button>';
  }).join('');

  return '<p class="panel-title">Wallets</p>' +
    '<p class="panel-sub">Select your preferred mobile wallet</p>' +
    '<div class="wallet-grid" id="walletGrid">' + cards + '</div>';
}

/* ── Pay Later Panel ── */
function buildPayLaterPanel() {
  return '<p class="panel-title">Pay Later</p>' +
    '<p class="panel-sub">Buy now, pay within 30 days. Zero interest on most transactions.</p>' +
    '<p class="app-grid-label">Available Providers</p>' +
    '<div class="paylater-row">' +
      '<span class="pl-badge" style="background:#1A237E">LazyPay</span>' +
      '<span class="pl-badge" style="background:#F05A22">Simpl</span>' +
      '<span class="pl-badge" style="background:#00529C">ZestMoney</span>' +
    '</div>';
}

/* ── EMI Panel ── */
function buildEmiPanel() {
  var tenures = [3,6,9,12,18,24];
  var btns = tenures.map(function (t) {
    return '<button class="emi-btn' + (t === selEmiMonths ? ' active' : '') + '" data-months="' + t + '">' + t + 'M</button>';
  }).join('');

  return '<p class="panel-title">EMI Options</p>' +
    '<p class="panel-sub">Split into easy monthly instalments.</p>' +
    '<p class="app-grid-label">Select Tenure</p>' +
    '<div class="emi-grid">' + btns + '</div>';
}

/* ── QR Panel ── */
function buildQrPanel() {
  return '<div class="qr-panel">' +
    '<p class="panel-title">Scan &amp; Pay</p>' +
    '<p class="panel-sub">Use any UPI app to scan and pay instantly</p>' +
    '<div class="qr-box">' +
      '<svg width="150" height="150" viewBox="0 0 120 120" fill="none">' +
        '<rect x="10" y="10" width="35" height="35" rx="4" fill="#111827"/>' +
        '<rect x="75" y="10" width="35" height="35" rx="4" fill="#111827"/>' +
        '<rect x="10" y="75" width="35" height="35" rx="4" fill="#111827"/>' +
        '<rect x="18" y="18" width="19" height="19" rx="2" fill="#fff"/>' +
        '<rect x="83" y="18" width="19" height="19" rx="2" fill="#fff"/>' +
        '<rect x="18" y="83" width="19" height="19" rx="2" fill="#fff"/>' +
        '<rect x="55" y="10" width="10" height="10" rx="1" fill="#111827"/>' +
        '<rect x="10" y="55" width="10" height="10" rx="1" fill="#111827"/>' +
        '<rect x="55" y="55" width="10" height="10" rx="2" fill="var(--pc)"/>' +
        '<rect x="75" y="55" width="10" height="10" rx="1" fill="#111827"/>' +
        '<rect x="100" y="55" width="10" height="10" rx="1" fill="#111827"/>' +
        '<rect x="55" y="75" width="10" height="10" rx="1" fill="#111827"/>' +
        '<rect x="55" y="100" width="10" height="10" rx="1" fill="#111827"/>' +
      '</svg>' +
    '</div>' +
    '<p class="qr-note">QR valid for <strong>10:00</strong> minutes</p>' +
  '</div>';
}

/* ═══════════════════════════════════════
   BIND PANEL EVENTS
═══════════════════════════════════════ */
function bindPanelEvents(key) {

  if (key === 'upi') {
    /* UPI input: floating label */
    var upiInput = document.getElementById('upiInput');
    var upiField = document.getElementById('upiField');
    var upiValid = document.getElementById('upiValid');

    if (upiInput && upiField) {
      upiInput.addEventListener('input', function () {
        upiField.classList.toggle('has-value', upiInput.value.length > 0);
        var valid = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9]+$/.test(upiInput.value);
        if (upiValid) upiValid.style.display = valid ? 'flex' : 'none';
      });
      upiInput.addEventListener('focus', function () { upiField.classList.add('focused'); });
      upiInput.addEventListener('blur',  function () { upiField.classList.remove('focused'); });
    }

    /* UPI app selection */
    document.querySelectorAll('#upiApps .app-card').forEach(function (card) {
      card.addEventListener('click', function () {
        document.querySelectorAll('#upiApps .app-card').forEach(function (c) { c.classList.remove('selected'); });
        card.classList.add('selected');
        selectedUpiApp = card.dataset.appid;
        if (upiInput) { upiInput.value = ''; if (upiField) upiField.classList.remove('has-value'); }
      });
    });
  }

  if (key === 'card' || key === 'debitCard' || key === 'creditCard') {
    /* Card number formatting + network detection */
    var numInput = document.getElementById('cardNumberInput');
    var numField = document.getElementById('cardNumberField');
    if (numInput && numField) {
      numInput.addEventListener('input', function () {
        var raw = numInput.value.replace(/\D/g,'').slice(0,16);
        numInput.value = raw.replace(/(.{4})/g,'$1 ').trim();
        numField.classList.toggle('has-value', raw.length > 0);
        detectNetwork(raw);
      });
      numInput.addEventListener('focus', function () { numField.classList.add('focused'); });
      numInput.addEventListener('blur',  function () { numField.classList.remove('focused'); });
    }

    /* Holder name */
    bindFloatLabel('cardHolder');

    /* Expiry formatting */
    var expInput = document.getElementById('expiryInput');
    var expField = document.getElementById('expiryField');
    if (expInput && expField) {
      expInput.addEventListener('input', function () {
        var raw = expInput.value.replace(/\D/g,'').slice(0,4);
        if (raw.length >= 3) raw = raw.slice(0,2) + '/' + raw.slice(2);
        expInput.value = raw;
        expField.classList.toggle('has-value', raw.length > 0);
      });
      expInput.addEventListener('focus', function () { expField.classList.add('focused'); });
      expInput.addEventListener('blur',  function () { expField.classList.remove('focused'); });
    }

    /* CVV field */
    var cvvField = document.getElementById('cvvField');
    var cvvInput = document.getElementById('cvvInput');
    if (cvvInput && cvvField) {
      cvvInput.addEventListener('input', function () {
        cvvField.classList.toggle('has-value', cvvInput.value.length > 0);
      });
      cvvInput.addEventListener('focus', function () { cvvField.classList.add('focused'); });
      cvvInput.addEventListener('blur',  function () { cvvField.classList.remove('focused'); });
    }

    /* CVV toggle */
    var cvvToggle = document.getElementById('cvvToggle');
    if (cvvToggle && cvvInput) {
      cvvToggle.addEventListener('click', function () {
        showCvv = !showCvv;
        cvvInput.type = showCvv ? 'text' : 'password';
      });
    }
  }

  if (key === 'netBanking') {
    document.querySelectorAll('#bankGrid .app-card').forEach(function (card) {
      card.addEventListener('click', function () {
        document.querySelectorAll('#bankGrid .app-card').forEach(function (c) { c.classList.remove('selected'); });
        card.classList.add('selected');
        selectedBank = card.dataset.bankid;
      });
    });
  }

  if (key === 'wallet') {
    document.querySelectorAll('#walletGrid .wallet-card').forEach(function (card) {
      card.addEventListener('click', function () {
        document.querySelectorAll('#walletGrid .wallet-card').forEach(function (c) {
          c.classList.remove('selected');
          var chk = c.querySelector('.wallet-check');
          if (chk) chk.remove();
        });
        card.classList.add('selected');
        selectedWallet = card.dataset.wid;
        var chk = document.createElement('div');
        chk.className = 'wallet-check';
        chk.innerHTML = '<svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="3" stroke-linecap="round"><path d="M20 6L9 17l-5-5"/></svg>';
        card.appendChild(chk);
      });
    });
  }

  if (key === 'emi') {
    document.querySelectorAll('.emi-btn').forEach(function (btn) {
      btn.addEventListener('click', function () {
        document.querySelectorAll('.emi-btn').forEach(function (b) { b.classList.remove('active'); });
        btn.classList.add('active');
        selEmiMonths = parseInt(btn.dataset.months, 10);
      });
    });
  }
}

function bindFloatLabel(id) {
  var input = document.getElementById(id + 'Input');
  var field = document.getElementById(id + 'Field');
  if (input && field) {
    input.addEventListener('input', function () { field.classList.toggle('has-value', input.value.length > 0); });
    input.addEventListener('focus', function () { field.classList.add('focused'); });
    input.addEventListener('blur',  function () { field.classList.remove('focused'); });
  }
}

function detectNetwork(num) {
  var nets = document.querySelectorAll('.card-net');
  if (!nets.length) return;
  nets.forEach(function (n) { n.classList.remove('active'); });
  if (!num) { nets.forEach(function (n) { n.classList.add('active'); }); return; }
  if (/^4/.test(num))       document.querySelector('[data-net="visa"]') && document.querySelector('[data-net="visa"]').classList.add('active');
  else if (/^5[1-5]/.test(num)) document.querySelector('[data-net="mc"]')   && document.querySelector('[data-net="mc"]').classList.add('active');
  else if (/^3[47]/.test(num))  document.querySelector('[data-net="amex"]') && document.querySelector('[data-net="amex"]').classList.add('active');
  else                          document.querySelector('[data-net="rupay"]') && document.querySelector('[data-net="rupay"]').classList.add('active');
}

/* ═══════════════════════════════════════
   PAYMENT BUTTON — HANDLE PAY
═══════════════════════════════════════ */
function handlePay(e) {
  if (payBtn && (payBtn.classList.contains('loading') || payBtn.classList.contains('success'))) return;

  /* Ripple */
  if (e && payBtn) {
    var rect = payBtn.getBoundingClientRect();
    var rip  = document.createElement('span');
    rip.className = 'btn-ripple';
    rip.style.left = (e.clientX - rect.left) + 'px';
    rip.style.top  = (e.clientY - rect.top)  + 'px';
    payBtn.appendChild(rip);
    setTimeout(function () { if (rip.parentNode) rip.parentNode.removeChild(rip); }, 650);
  }

  /* Build request payload */
  var payload = buildPayload();
  if (!payload) return; /* validation failed */

  setBtnLoading();

  /* POST to /api/checkout/pay */
  fetch('/api/checkout/pay', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })
  .then(function (res) { return res.json(); })
  .then(function (data) {
    if (data && data.success && data.data) {
      var d = data.data;
      if (d.requiresRedirect && d.redirectUrl) {
        window.location.href = d.redirectUrl;
      } else {
        onPaymentSuccess(d.paymentId, d.orderId, d.signature, d.amount, d.paidAt);
      }
    } else {
      onPaymentError(data.message || 'Payment failed. Please try again.');
    }
  })
  .catch(function () {
    onPaymentError('Network error. Please check your connection and try again.');
  });
}

/* ── Payment mode key  →  API PaymentMode value ── */
var MODE_MAP = {
  upi:        'UPI',
  card:       'Card',
  debitCard:  'Card',
  creditCard: 'Card',
  netBanking: 'NetBanking',
  wallet:     'Wallet',
  emi:        'EMI',
  payLater:   'PayLater'
};

function buildPayload() {
  var base = {
    checkoutToken: cfg.token,                         /* required by /api/checkout/pay */
    paymentMode:   MODE_MAP[activeMethod] || activeMethod
  };

  if (activeMethod === 'upi') {
    var upiInput = document.getElementById('upiInput');
    var upiId    = upiInput ? upiInput.value.trim() : '';
    if (!upiId && !selectedUpiApp) {
      shakePanel(); return null;
    }
    /* API field: upiVpa */
    base.upiVpa = upiId || (selectedUpiApp ? selectedUpiApp + '@upi' : null);
  }

  if (activeMethod === 'card' || activeMethod === 'debitCard' || activeMethod === 'creditCard') {
    var num  = (document.getElementById('cardNumberInput') || {}).value || '';
    var name = (document.getElementById('cardHolderInput') || {}).value || '';
    var exp  = (document.getElementById('expiryInput')    || {}).value || '';
    var cvv  = (document.getElementById('cvvInput')        || {}).value || '';
    if (!num.replace(/\s/g,'') || !name || !exp || !cvv) { shakePanel(); return null; }
    /* API fields: cardNumber, cardName, cardExpiry, cardCvv */
    base.cardNumber = num.replace(/\s/g,'');
    base.cardName   = name;
    base.cardExpiry = exp.replace(/\s/g,'');  /* normalise "MM / YY" → "MM/YY" */
    base.cardCvv    = cvv;
  }

  if (activeMethod === 'netBanking') {
    var bankSearch = (document.getElementById('bankSearch') || {}).value || '';
    base.bankCode = selectedBank || bankSearch || null;
    if (!base.bankCode) { shakePanel(); return null; }
  }

  if (activeMethod === 'wallet') {
    /* API field: bankCode (reused for wallet provider) */
    base.bankCode = selectedWallet;
    if (!base.bankCode) { shakePanel(); return null; }
  }

  if (activeMethod === 'emi') {
    var emiNum = (document.getElementById('emiCardInput') || {}).value || '';
    base.cardNumber = emiNum.replace(/\s/g,'');
    if (!base.cardNumber) { shakePanel(); return null; }
  }

  return base;
}

function shakePanel() {
  var panel = document.getElementById('pmPanel');
  if (!panel) return;
  panel.style.animation = 'none';
  panel.offsetHeight;
  panel.style.animation = 'shake 0.35s ease-out';
}

/* ── Button state helpers ── */
function setBtnLoading() {
  if (!payBtn) return;
  payBtn.disabled = true;
  payBtn.classList.add('loading');
  payBtn.innerHTML =
    '<div class="btn-spinner"></div>' +
    '<span class="btn-lbl">Processing…</span>';
}

function setBtnSuccess() {
  if (!payBtn) return;
  payBtn.classList.remove('loading');
  payBtn.classList.add('success');
  payBtn.innerHTML =
    '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.5" stroke-linecap="round"><path d="M20 6L9 17l-5-5"/></svg>' +
    '<span class="btn-lbl">Payment Successful!</span>';
}

function resetPayBtn() {
  if (!payBtn) return;
  payBtn.disabled = false;
  payBtn.className = 'ck-pay-btn';
  payBtn.innerHTML =
    '<span class="btn-lock">' + ICONS.lock + '</span>' +
    '<span class="btn-lbl">Pay ' + esc(amountStr) + '</span>';
}

/* ═══════════════════════════════════════════════════════════
   RESULT BOTTOM SHEET  — modern slide-up card with animated
   SVG check / cross and circular countdown ring
═══════════════════════════════════════════════════════════ */

/* SVG ring circumference for r = 22: 2π × 22 ≈ 138.23 */
var RING_C = 138.23;

function removeResultSheet() {
  var old = document.getElementById('ckResultSheet');
  if (old) old.remove();
}

/* ── shared helper: human-readable mode label ── */
function modeLabel(key) {
  var map = { UPI:'UPI', Card:'Card', NetBanking:'Net Banking',
              Wallet:'Wallet', EMI:'EMI', PayLater:'Pay Later' };
  return map[key] || key || '—';
}

/* ══════════ SUCCESS ══════════ */
function onPaymentSuccess(paymentId, orderId, signature, amount, paidAt) {
  setBtnSuccess();

  /* ① Notify parent window immediately via postMessage */
  var modeKey = MODE_MAP[activeMethod] || activeMethod;
  window.parent.postMessage({
    source:       'BankUPG',
    event:        'payment.success',
    payment_id:   paymentId,
    order_id:     orderId,
    signature:    signature,
    amount:       String(amount || cfg.amount || ''),
    payment_mode: modeKey,
    paid_at:      paidAt || new Date().toISOString()
  }, '*');

  /* ② Build bottom sheet */
  removeResultSheet();
  var sym   = cfg.amountSymbol || '₹';
  var amtStr = String(typeof amount === 'number' ? amount.toFixed(2) : (amount || cfg.amount || '0.00'));
  var sheet = document.createElement('div');
  sheet.id        = 'ckResultSheet';
  sheet.className = 'crs-sheet crs-sheet-success';

  sheet.innerHTML =
    /* ── backdrop strip (gradient) ── */
    '<div class="crs-top">' +
      /* animated check SVG */
      '<div class="crs-icon-circle crs-success-circle">' +
        '<svg class="crs-svg-check" viewBox="0 0 52 52">' +
          '<circle class="crs-circle-stroke" cx="26" cy="26" r="24" fill="none" stroke-width="2.5"/>' +
          '<path  class="crs-check-mark"  fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round" d="M14 27 23 36 38 18"/>' +
        '</svg>' +
      '</div>' +
      '<p class="crs-heading">Payment Successful!</p>' +
      '<p class="crs-amount">' + esc(sym + amtStr) + '</p>' +
    '</div>' +

    /* ── details card ── */
    '<div class="crs-body">' +
      '<div class="crs-meta">' +
        '<div class="crs-meta-row"><span>Paid to</span><strong>' + esc(cfg.merchantName || 'BankU') + '</strong></div>' +
        '<div class="crs-meta-row"><span>Order ID</span><code>'   + esc(orderId    || '—') + '</code></div>' +
        '<div class="crs-meta-row"><span>Payment ID</span><code>' + esc(paymentId  || '—') + '</code></div>' +
        '<div class="crs-meta-row"><span>Method</span><strong>'   + esc(modeLabel(modeKey))  + '</strong></div>' +
      '</div>' +

      /* countdown ring + label */
      '<div class="crs-countdown-wrap">' +
        '<div class="crs-ring-wrap">' +
          '<svg class="crs-ring-svg" viewBox="0 0 52 52">' +
            '<circle class="crs-ring-bg"   cx="26" cy="26" r="22" fill="none" stroke-width="3.5"/>' +
            '<circle class="crs-ring-fill" cx="26" cy="26" r="22" fill="none" stroke-width="3.5"' +
              ' stroke-dasharray="' + RING_C + '" stroke-dashoffset="0" id="crsRing"/>' +
          '</svg>' +
          '<span class="crs-ring-num" id="crsRingNum">5</span>' +
        '</div>' +
        '<p class="crs-redirect-txt">Redirecting in <span id="crsRedirSecs">5</span>s…</p>' +
      '</div>' +
    '</div>';

  document.body.appendChild(sheet);

  /* ③ Tick countdown — ring drains from full to empty over 5 s */
  var secs = 5;
  function tick() {
    secs--;
    var numEl  = document.getElementById('crsRingNum');
    var secsEl = document.getElementById('crsRedirSecs');
    var ring   = document.getElementById('crsRing');
    if (numEl)  numEl.textContent  = Math.max(0, secs);
    if (secsEl) secsEl.textContent = Math.max(0, secs);
    /* dashoffset goes from 0 (full) to RING_C (empty) */
    if (ring)   ring.style.strokeDashoffset = String((RING_C * (5 - secs) / 5).toFixed(2));
    if (secs <= 0) {
      if (numEl) numEl.textContent = '✓';
      window.parent.postMessage({ source: 'BankUPG', event: 'payment.close_ok' }, '*');
    } else {
      setTimeout(tick, 1000);
    }
  }
  setTimeout(tick, 1000);
}

/* ══════════ ERROR ══════════ */
function onPaymentError(msg) {
  /* Reset pay button to "Try Again" state */
  if (payBtn) {
    payBtn.disabled = false;
    payBtn.classList.remove('loading');
    payBtn.innerHTML =
      ICONS.close +
      '<span class="btn-lbl">Payment Failed — Tap to Retry</span>';
    payBtn.className = 'ck-pay-btn error';
    /* One-shot click restores normal state */
    payBtn.addEventListener('click', function onRetap() {
      payBtn.removeEventListener('click', onRetap);
      resetPayBtn();
      removeResultSheet();
    }, { once: true });
  }

  removeResultSheet();
  var sheet = document.createElement('div');
  sheet.id        = 'ckResultSheet';
  sheet.className = 'crs-sheet crs-sheet-error';

  sheet.innerHTML =
    '<div class="crs-top crs-top-error">' +
      '<div class="crs-icon-circle crs-error-circle">' +
        '<svg class="crs-svg-cross" viewBox="0 0 52 52">' +
          '<circle class="crs-circle-stroke crs-circle-red" cx="26" cy="26" r="24" fill="none" stroke-width="2.5"/>' +
          '<path class="crs-cross-line1" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" d="M17 17 35 35"/>' +
          '<path class="crs-cross-line2" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" d="M35 17 17 35"/>' +
        '</svg>' +
      '</div>' +
      '<p class="crs-heading">Payment Failed</p>' +
      '<p class="crs-err-msg">' + esc(msg) + '</p>' +
    '</div>' +

    '<div class="crs-body crs-body-error">' +
      '<div class="crs-err-actions">' +
        '<button class="crs-btn-retry" id="crsRetryBtn">' +
          '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><path d="M1 4v6h6"/><path d="M3.51 15a9 9 0 102.13-9.36L1 10"/></svg>' +
          ' Try Another Method' +
        '</button>' +
        '<button class="crs-btn-close" id="crsCloseBtn">Close</button>' +
      '</div>' +
    '</div>';

  document.body.appendChild(sheet);

  var retryBtn = document.getElementById('crsRetryBtn');
  var closeBtn = document.getElementById('crsCloseBtn');
  if (retryBtn) retryBtn.addEventListener('click', function () {
    removeResultSheet();
    resetPayBtn();
  });
  if (closeBtn) closeBtn.addEventListener('click', function () {
    window.parent.postMessage({ source: 'BankUPG', event: 'payment.dismiss' }, '*');
  });
}

/* ── Panel entrance animation ── */
(function addStyles() {
  var s = document.createElement('style');
  s.textContent =
    '@keyframes panelIn { from { opacity:0; transform:translateX(6px); } to { opacity:1; transform:translateX(0); } }' +
    '@keyframes shake   { 0%,100%{transform:translateX(0)} 20%,60%{transform:translateX(-6px)} 40%,80%{transform:translateX(6px)} }';
  document.head.appendChild(s);
})();

/* ── Boot ── */
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}
