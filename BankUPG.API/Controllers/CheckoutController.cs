using BankUPG.Application.Interfaces.Checkout;
using BankUPG.Application.Services.Checkout;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _service;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICheckoutService service, ILogger<CheckoutController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}";

        private string? GetApiKey() => Request.Headers.TryGetValue("X-Api-Key", out var v) ? v.ToString() : null;

        private string GetCallerIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            if (ip == null) return string.Empty;
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            return ip.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // MERCHANT SERVER-SIDE APIs (protected by X-Api-Key header)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Step 1: Merchant server creates a checkout order and gets a checkout_token.
        /// </summary>
        [HttpPost("api/checkout/orders")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutOrderResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutOrderResponse>>> CreateOrder(
            [FromBody] CheckoutInitiateRequest request)
        {
            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
                return Unauthorized(new ApiResponse { Success = false, Message = "X-Api-Key header is required." });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CheckoutOrderResponse>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });

            try
            {
                var result = await _service.InitiateOrderAsync(apiKey, request, GetBaseUrl(), GetCallerIp());
                return Ok(new ApiResponse<CheckoutOrderResponse> { Success = true, Message = "Order created.", Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout order creation failed");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred." });
            }
        }

        /// <summary>
        /// Step 4: Merchant server verifies HMAC signature after redirect.
        /// </summary>
        [HttpPost("api/checkout/verify")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutVerifyResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutVerifyResponse>>> VerifyPayment(
            [FromBody] CheckoutVerifyRequest request)
        {
            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
                return Unauthorized(new ApiResponse { Success = false, Message = "X-Api-Key header is required." });

            try
            {
                var result = await _service.VerifyPaymentAsync(apiKey, request, GetCallerIp());
                return Ok(new ApiResponse<CheckoutVerifyResponse>
                {
                    Success = result.IsValid,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment verification failed");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred." });
            }
        }

        /// <summary>
        /// Get order payment status.
        /// </summary>
        [HttpGet("api/checkout/orders/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutStatusResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutStatusResponse>>> GetOrderStatus(string orderId)
        {
            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
                return Unauthorized(new ApiResponse { Success = false, Message = "X-Api-Key header is required." });

            try
            {
                var result = await _service.GetOrderStatusAsync(apiKey, orderId, GetCallerIp());
                if (result == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Order not found." });

                return Ok(new ApiResponse<CheckoutStatusResponse> { Success = true, Message = "Order status retrieved.", Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // CHECKOUT PAGE (served to customer browser – no auth needed)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Step 2: Customer opens the hosted checkout page.
        /// </summary>
        [HttpGet("checkout/{token}")]
        public async Task<IActionResult> CheckoutPage(string token)
        {
            var session = await _service.GetSessionAsync(token);
            if (session == null) return Content(BuildErrorPage("Invalid payment link."), "text/html");
            if (session.IsExpired) return Content(BuildErrorPage("This payment link has expired."), "text/html");
            if (session.IsPaid) return Content(BuildSuccessPage(session.OrderId, session.Amount, session.Currency, "Payment already completed."), "text/html");

            return Content(BuildCheckoutPage(token, session), "text/html");
        }

        /// <summary>
        /// Step 3: Customer submits payment (called from checkout page JS).
        /// </summary>
        [HttpPost("api/checkout/pay")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutPayResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutPayResponse>>> ProcessPayment(
            [FromBody] CheckoutPayCardRequest request)
        {
            try
            {
                var result = await _service.ProcessPaymentAsync(request, GetBaseUrl());
                return Ok(new ApiResponse<CheckoutPayResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing error");
                return StatusCode(500, new ApiResponse { Success = false, Message = "Payment processing failed. Please try again." });
            }
        }

        /// <summary>
        /// Checkout result landing page (for demo redirect).
        /// </summary>
        [HttpGet("checkout-result")]
        public IActionResult CheckoutResult(
            [FromQuery(Name = "payment_id")] string? paymentId,
            [FromQuery(Name = "order_id")] string? orderId,
            [FromQuery] string? status)
        {
            if (status == "success")
                return Content(BuildSuccessPage(orderId ?? "", 0, "INR", $"Payment ID: {paymentId}"), "text/html");

            return Content(BuildErrorPage("Payment was not completed. Please try again."), "text/html");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // HTML PAGE BUILDERS
        // ─────────────────────────────────────────────────────────────────────────

        private static string BuildCheckoutPage(string token, CheckoutSessionResponse session)
        {
            var symbol = session.Currency == "INR" ? "₹" : session.Currency;
            var config = new
            {
                token,
                amount = session.Amount.ToString("F2"),
                amountSymbol = symbol,
                orderId = session.OrderId,
                orderRef = session.OrderRef,
                merchantName = session.MerchantName ?? "BankUPG",
                logoUrl = session.MerchantLogoUrl ?? "",
                primaryColor = session.PrimaryColor ?? "#009688",
                secondaryColor = session.SecondaryColor ?? "#7c3aed",
                customerName = session.CustomerName ?? "",
                customerEmail = session.CustomerEmail ?? "",
                customerPhone = session.CustomerPhone ?? "",
                modes = session.EnabledPaymentModes
            };
            var configJson = System.Text.Json.JsonSerializer.Serialize(config);
            var pageTitle = $"Pay {session.Amount:F2} | {session.MerchantName ?? "BankUPG"}";
            return GetCheckoutHtmlTemplate()
                .Replace("__CHECKOUT_CONFIG__", configJson)
                .Replace("__PAGE_TITLE__", pageTitle);
        }

        private static string GetCheckoutHtmlTemplate()
        {
            return """
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>__PAGE_TITLE__</title>
<link rel='stylesheet' href='/checkout-page.css?v=5'>
</head>
<body>
<div class='pg-wrap'>

  <!-- ── LEFT PANEL (desktop branding strip) ── -->
  <div class='pg-left'>

    <div class='pg-brand'>
      <div class='logo-wrap' id='logoWrap'></div>
      <div class='m-name' id='mName'></div>
    </div>

    <div class='pg-amount-section'>
      <div class='amt-lbl'>Total Payable</div>
      <div class='amt-row'>
        <span class='amt-val' id='amtVal'></span>
        <span class='amt-chev' id='amtChev'>&#9660;</span>
      </div>
      <div class='ord-dtl' id='ordDtl'></div>
    </div>

    <div class='cust-info' id='custInfo'></div>
    <div class='l-spacer'></div>

    <div class='pg-secure'>
      <div class='sec-row'>
        <svg width='13' height='15' viewBox='0 0 13 15' fill='none' style='flex-shrink:0'>
          <path d='M6.5 0L0 2.7v4.9c0 3.6 2.8 6.9 6.5 7.6 3.7-.7 6.5-4 6.5-7.6V2.7L6.5 0z' fill='rgba(255,255,255,0.7)'/>
        </svg>
        Secure Checkout
      </div>
      <div class='txn-lbl' id='txnLbl'></div>
    </div>

  </div>

  <!-- ── MOBILE AMOUNT CARD (shown between top-bar and payment list on mobile) ── -->
  <div class='mob-card'>
    <div class='mob-amt-lbl'>Amount To Pay</div>
    <div class='mob-amt-val' id='mobAmtVal'></div>
    <div class='mob-secure'>Secure Payment Gateway</div>
  </div>

  <!-- ── RIGHT PANEL (payment options) ── -->
  <div class='pg-right'>

    <div class='pg-hdr'>
      <button class='back-btn' id='backBtn'>&#8592; Back</button>
    </div>

    <div class='opt-label'>PAYMENT OPTIONS</div>
    <div class='opt-list' id='optList'></div>

    <div class='res-ov' id='resOv'>
      <div class='res-box'>
        <div class='res-ico' id='resIco'></div>
        <div class='res-ttl' id='resTtl'></div>
        <div class='res-amt' id='resAmt'></div>
        <div class='res-sub' id='resSub'></div>
        <div class='res-id'  id='resId'></div>
        <div class='res-countdown' id='resCountdown'></div>
        <button class='retry-btn' id='retryBtn'>Try Another Method</button>
      </div>
    </div>

    <div class='pg-foot'>
      <span class='pow-by'>Powered by <strong>BankU</strong></span>
      <div class='net-tags'>
        <span class='net-tag'>VISA</span>
        <span class='net-tag'>Mastercard</span>
        <span class='net-tag'>RuPay</span>
      </div>
    </div>

  </div>
</div>
<!-- Config injected by server — parsed by checkout-page.js, never executed -->
<script type='application/json' id='cfg-data'>__CHECKOUT_CONFIG__</script>
<script src='/checkout-page.js?v=5'></script>
<!-- all JS served from /checkout-page.js -->
<!-- REMOVEME_START
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
    if (cfg.customerName) ch += '<div class="cust-row"><span>&#x1F464;</span>' + cfg.customerName + '</div>';
    if (cfg.customerEmail) ch += '<div class="cust-row"><span>&#x2709;</span>' + cfg.customerEmail + '</div>';
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
  var nm = document.getElementById('cardName').value.trim();
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
    if (v.startsWith('4')) b.textContent = 'Visa';
    else if (v.startsWith('5')) b.textContent = 'MC';
    else if (v.startsWith('34') || v.startsWith('37')) b.textContent = 'Amex';
    else if (v.startsWith('6')) b.textContent = 'RuPay';
    else b.textContent = '💳';
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
    if (window.parent !== window) { window.parent.postMessage({ source: 'BankUPG', event: 'payment.success', payment_id: pid, order_id: cfg.orderId, signature: sig, amount: cfg.amount, payment_mode: mode, paid_at: new Date().toISOString() }, '*'); }
    setTimeout(function() { if (rdUrl) window.top.location.href = rdUrl; }, 2500);
  }
}

window.addEventListener('DOMContentLoaded', init);
REMOVEME_END -->
</body>
</html>
""";
        }

        private static string BuildCheckoutPage_REMOVED(string token, CheckoutSessionResponse session)
        {
            var amount = session.Amount.ToString("N2");
            var primaryColor = string.IsNullOrEmpty(session.PrimaryColor) ? "#1a73e8" : session.PrimaryColor;
            var merchantName = session.MerchantName ?? "BankUPG";
            var logoHtml = string.IsNullOrEmpty(session.MerchantLogoUrl)
                ? $"<div class='logo-placeholder'>{merchantName[0]}</div>"
                : $"<img src='{session.MerchantLogoUrl}' class='merchant-logo' alt='{merchantName}' />";

            var tabsHtml = BuildPaymentModeTabs(session.EnabledPaymentModes, primaryColor);
            var modesJson = System.Text.Json.JsonSerializer.Serialize(session.EnabledPaymentModes);

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>Pay {session.Currency} {amount} | {merchantName}</title>
<style>
  *{{margin:0;padding:0;box-sizing:border-box;}}
  body{{font-family:'Segoe UI',system-ui,sans-serif;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);min-height:100vh;display:flex;align-items:center;justify-content:center;padding:16px;}}
  .checkout-wrapper{{display:flex;gap:0;max-width:840px;width:100%;background:#fff;border-radius:16px;box-shadow:0 24px 64px rgba(0,0,0,0.18);overflow:hidden;}}
  .checkout-left{{width:280px;min-width:280px;background:linear-gradient(160deg,{primaryColor} 0%,#1557b0 100%);color:#fff;padding:32px 24px;display:flex;flex-direction:column;gap:16px;}}
  .merchant-logo{{width:56px;height:56px;border-radius:12px;object-fit:contain;background:#fff;padding:4px;}}
  .logo-placeholder{{width:56px;height:56px;border-radius:12px;background:rgba(255,255,255,0.2);display:flex;align-items:center;justify-content:center;font-size:24px;font-weight:700;}}
  .merchant-name{{font-size:18px;font-weight:700;margin-top:4px;}}
  .pay-amount{{font-size:32px;font-weight:800;margin:8px 0 4px;}}
  .pay-label{{font-size:12px;opacity:0.8;text-transform:uppercase;letter-spacing:1px;}}
  .customer-info{{margin-top:auto;font-size:13px;opacity:0.85;line-height:1.6;}}
  .checkout-right{{flex:1;padding:32px;display:flex;flex-direction:column;}}
  .checkout-title{{font-size:16px;font-weight:600;color:#333;margin-bottom:20px;}}
  .tab-bar{{display:flex;border-bottom:2px solid #e8eaed;margin-bottom:24px;gap:4px;}}
  .tab-btn{{padding:10px 16px;font-size:13px;font-weight:600;color:#666;border:none;background:none;cursor:pointer;border-bottom:2px solid transparent;margin-bottom:-2px;border-radius:8px 8px 0 0;transition:all 0.2s;}}
  .tab-btn.active{{color:{primaryColor};border-bottom-color:{primaryColor};background:#f0f6ff;}}
  .tab-btn:hover:not(.active){{background:#f5f5f5;color:#333;}}
  .tab-content{{display:none;}}
  .tab-content.active{{display:flex;flex-direction:column;gap:16px;}}
  .form-group{{display:flex;flex-direction:column;gap:6px;}}
  .form-row{{display:grid;grid-template-columns:1fr 1fr;gap:12px;}}
  label{{font-size:12px;font-weight:600;color:#555;text-transform:uppercase;letter-spacing:0.5px;}}
  input{{height:48px;border:1.5px solid #ddd;border-radius:8px;padding:0 14px;font-size:14px;color:#333;transition:border 0.2s;outline:none;width:100%;}}
  input:focus{{border-color:{primaryColor};box-shadow:0 0 0 3px {primaryColor}22;}}
  .card-icons{{position:relative;}}
  .card-brand{{position:absolute;right:12px;top:50%;transform:translateY(-50%);font-size:18px;opacity:0.7;}}
  .pay-btn{{height:52px;background:linear-gradient(90deg,{primaryColor},{primaryColor}cc);color:#fff;border:none;border-radius:10px;font-size:16px;font-weight:700;cursor:pointer;letter-spacing:0.3px;transition:all 0.2s;margin-top:8px;width:100%;display:flex;align-items:center;justify-content:center;gap:8px;}}
  .pay-btn:hover{{transform:translateY(-1px);box-shadow:0 6px 20px {primaryColor}66;}}
  .pay-btn:active{{transform:translateY(0);}}
  .pay-btn:disabled{{opacity:0.7;cursor:not-allowed;transform:none;}}
  .secure-badge{{display:flex;align-items:center;gap:6px;justify-content:center;font-size:11px;color:#888;margin-top:12px;}}
  .spinner{{width:18px;height:18px;border:2px solid rgba(255,255,255,0.4);border-top-color:#fff;border-radius:50%;animation:spin 0.6s linear infinite;}}
  @keyframes spin{{to{{transform:rotate(360deg);}}}}
  .result-screen{{display:none;flex-direction:column;align-items:center;justify-content:center;padding:40px 20px;text-align:center;gap:16px;}}
  .result-icon{{width:72px;height:72px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:32px;}}
  .result-icon.success{{background:#e8f5e9;color:#2e7d32;}}
  .result-icon.failure{{background:#fce4ec;color:#c62828;}}
  .result-title{{font-size:22px;font-weight:700;color:#333;}}
  .result-sub{{font-size:14px;color:#666;}}
  .result-amount{{font-size:28px;font-weight:800;color:{primaryColor};}}
  .result-id{{background:#f5f7fa;border-radius:8px;padding:10px 16px;font-size:13px;color:#555;width:100%;word-break:break-all;}}
  .retry-btn{{height:44px;background:transparent;border:2px solid {primaryColor};color:{primaryColor};border-radius:8px;font-size:14px;font-weight:600;cursor:pointer;padding:0 24px;margin-top:8px;}}
  .upi-hint{{font-size:12px;color:#888;margin-top:-8px;}}
  .bank-select{{height:48px;border:1.5px solid #ddd;border-radius:8px;padding:0 14px;font-size:14px;color:#333;background:#fff;width:100%;outline:none;cursor:pointer;}}
  .bank-select:focus{{border-color:{primaryColor};}}
  .error-msg{{font-size:12px;color:#d32f2f;background:#ffeaea;border-radius:6px;padding:8px 12px;display:none;}}
  @media(max-width:600px){{.checkout-wrapper{{flex-direction:column;}}.checkout-left{{width:100%;flex-direction:row;align-items:center;flex-wrap:wrap;}}.pay-amount{{font-size:24px;}}.customer-info{{display:none;}}}}
</style>
</head>
<body>
<div class=""checkout-wrapper"">
  <!-- Left Panel -->
  <div class=""checkout-left"">
    {logoHtml}
    <div class=""merchant-name"">{merchantName}</div>
    <div>
      <div class=""pay-label"">Amount to Pay</div>
      <div class=""pay-amount"">{session.Currency} {amount}</div>
    </div>
    {BuildCustomerInfoHtml(session)}
    <div style=""font-size:11px;opacity:0.6;margin-top:8px;"">Order: {session.OrderRef}</div>
  </div>
  <!-- Right Panel -->
  <div class=""checkout-right"">
    <div class=""checkout-title"">Choose Payment Method</div>
    <div class=""tab-bar"" id=""tabBar"">{tabsHtml}</div>

    <!-- CARD TAB -->
    <div class=""tab-content"" id=""tab-Card"">
      <div class=""form-group card-icons"">
        <label>Card Number</label>
        <input type=""text"" id=""cardNumber"" placeholder=""0000 0000 0000 0000"" maxlength=""19"" autocomplete=""cc-number"" />
        <span class=""card-brand"" id=""cardBrand"">💳</span>
      </div>
      <div class=""form-group"">
        <label>Name on Card</label>
        <input type=""text"" id=""cardName"" placeholder=""JOHN SMITH"" autocomplete=""cc-name"" value=""{session.CustomerName ?? ""}"" />
      </div>
      <div class=""form-row"">
        <div class=""form-group"">
          <label>Expiry (MM/YY)</label>
          <input type=""text"" id=""cardExpiry"" placeholder=""MM / YY"" maxlength=""7"" autocomplete=""cc-exp"" />
        </div>
        <div class=""form-group"">
          <label>CVV</label>
          <input type=""password"" id=""cardCvv"" placeholder=""•••"" maxlength=""4"" autocomplete=""cc-csc"" />
        </div>
      </div>
      <div class=""error-msg"" id=""cardError""></div>
      <button class=""pay-btn"" id=""payCardBtn"" onclick=""payCard()"">
        <span>Pay {session.Currency} {amount}</span>
      </button>
    </div>

    <!-- UPI TAB -->
    <div class=""tab-content"" id=""tab-UPI"">
      <div class=""form-group"">
        <label>UPI ID / VPA</label>
        <input type=""text"" id=""upiVpa"" placeholder=""yourname@okhdfc"" />
        <div class=""upi-hint"">Enter your UPI ID linked to any bank</div>
      </div>
      <div class=""error-msg"" id=""upiError""></div>
      <button class=""pay-btn"" id=""payUpiBtn"" onclick=""payUpi()"">
        <span>Pay {session.Currency} {amount}</span>
      </button>
    </div>

    <!-- NET BANKING TAB -->
    <div class=""tab-content"" id=""tab-NetBanking"">
      <div class=""form-group"">
        <label>Select Your Bank</label>
        <select class=""bank-select"" id=""bankCode"">
          <option value="""">-- Select Bank --</option>
          <option value=""HDFCBANK"">HDFC Bank</option>
          <option value=""SBIN"">State Bank of India</option>
          <option value=""ICICIBANK"">ICICI Bank</option>
          <option value=""AXISBANK"">Axis Bank</option>
          <option value=""KOTAKBANK"">Kotak Mahindra Bank</option>
          <option value=""INDUSIND"">IndusInd Bank</option>
          <option value=""YESBANK"">Yes Bank</option>
          <option value=""PNBRETAIL"">Punjab National Bank</option>
          <option value=""BOBIRETAIL"">Bank of Baroda</option>
          <option value=""UNIONBANK"">Union Bank of India</option>
        </select>
      </div>
      <div class=""error-msg"" id=""nbError""></div>
      <button class=""pay-btn"" id=""payNbBtn"" onclick=""payNetBanking()"">
        <span>Pay {session.Currency} {amount}</span>
      </button>
    </div>

    <!-- Result Screen -->
    <div class=""result-screen"" id=""resultScreen"">
      <div class=""result-icon"" id=""resultIcon""></div>
      <div class=""result-title"" id=""resultTitle""></div>
      <div class=""result-amount"" id=""resultAmount""></div>
      <div class=""result-sub"" id=""resultSub""></div>
      <div class=""result-id"" id=""resultId"" style=""display:none;""></div>
      <button class=""retry-btn"" id=""retryBtn"" onclick=""retryPayment()"" style=""display:none;"">Try Another Method</button>
    </div>

    <div class=""secure-badge"">
      <svg width=""12"" height=""14"" viewBox=""0 0 12 14"" fill=""none""><path d=""M6 0L0 2.5v4.5c0 3.3 2.6 6.3 6 7 3.4-.7 6-3.7 6-7V2.5L6 0z"" fill=""#34a853""/></svg>
      Secured by BankUPG · 256-bit SSL encryption
    </div>
  </div>
</div>

<script>
const TOKEN = '{token}';
const MODES = {modesJson};
const API_BASE = '';

function setLoading(btnId, loading) {{
  const btn = document.getElementById(btnId);
  if (!btn) return;
  if (loading) {{
    btn.disabled = true;
    btn.innerHTML = '<div class=""spinner""></div><span>Processing...</span>';
  }} else {{
    btn.disabled = false;
  }}
}}

function showError(id, msg) {{
  const el = document.getElementById(id);
  if (el) {{ el.style.display = 'block'; el.textContent = msg; }}
}}

function hideError(id) {{
  const el = document.getElementById(id);
  if (el) el.style.display = 'none';
}}

function showResult(success, paymentId, amount, mode, message) {{
  document.querySelector('.tab-bar').style.display = 'none';
  document.querySelectorAll('.tab-content').forEach(t => t.style.display = 'none');
  const rs = document.getElementById('resultScreen');
  rs.style.display = 'flex';
  const icon = document.getElementById('resultIcon');
  const title = document.getElementById('resultTitle');
  const amtEl = document.getElementById('resultAmount');
  const sub = document.getElementById('resultSub');
  const idEl = document.getElementById('resultId');
  const retryBtn = document.getElementById('retryBtn');
  if (success) {{
    icon.className = 'result-icon success';
    icon.textContent = '✓';
    title.textContent = 'Payment Successful!';
    amtEl.textContent = amount;
    sub.textContent = `via ${{mode}}`;
    idEl.style.display = 'block';
    idEl.textContent = `Payment ID: ${{paymentId}}`;
  }} else {{
    icon.className = 'result-icon failure';
    icon.textContent = '✕';
    title.textContent = 'Payment Failed';
    amtEl.textContent = '';
    sub.textContent = message || 'Please try another method.';
    retryBtn.style.display = 'inline-block';
  }}
}}

function retryPayment() {{
  location.reload();
}}

async function submitPayment(payload, btnId, errId, resetBtnHtml) {{
  hideError(errId);
  setLoading(btnId, true);
  try {{
    const res = await fetch('/api/checkout/pay', {{
      method: 'POST',
      headers: {{ 'Content-Type': 'application/json' }},
      body: JSON.stringify(payload)
    }});
    const json = await res.json();
    const data = json.data;
    if (json.success && data.success) {{
      showResult(true, data.paymentId, '{session.Currency} {amount}', data.paymentMode, data.message);
      setTimeout(() => {{
        if (data.redirectUrl && data.redirectUrl !== '/checkout-result?payment_id=undefined&order_id=undefined&status=success') {{
          window.top.location.href = data.redirectUrl;
        }}
      }}, 2000);
    }} else {{
      showError(errId, (data && data.message) || json.message || 'Payment failed. Please try again.');
      const btn = document.getElementById(btnId);
      if (btn) btn.innerHTML = resetBtnHtml;
      btn.disabled = false;
    }}
  }} catch(e) {{
    showError(errId, 'Network error. Please try again.');
    const btn = document.getElementById(btnId);
    if (btn) btn.innerHTML = resetBtnHtml;
    btn.disabled = false;
  }}
}}

function payCard() {{
  const num = document.getElementById('cardNumber').value.replace(/\s/g,'');
  const name = document.getElementById('cardName').value.trim();
  const exp = document.getElementById('cardExpiry').value.trim();
  const cvv = document.getElementById('cardCvv').value.trim();
  if (num.length < 15) return showError('cardError','Please enter a valid card number.');
  if (!name) return showError('cardError','Please enter the name on card.');
  if (!exp.match(/^\d{{2}}\s*\/\s*\d{{2}}$/)) return showError('cardError','Please enter a valid expiry date (MM/YY).');
  if (cvv.length < 3) return showError('cardError','Please enter a valid CVV.');
  submitPayment({{ checkoutToken: TOKEN, paymentMode:'Card', cardNumber:num, cardName:name, cardExpiry:exp, cardCvv:cvv }}, 'payCardBtn', 'cardError', '<span>Pay {session.Currency} {amount}</span>');
}}

function payUpi() {{
  const vpa = document.getElementById('upiVpa').value.trim();
  if (!vpa.includes('@')) return showError('upiError','Please enter a valid UPI ID (e.g. name@okhdfc).');
  submitPayment({{ checkoutToken: TOKEN, paymentMode:'UPI', upiVpa: vpa }}, 'payUpiBtn', 'upiError', '<span>Pay {session.Currency} {amount}</span>');
}}

function payNetBanking() {{
  const bank = document.getElementById('bankCode').value;
  if (!bank) return showError('nbError','Please select your bank.');
  submitPayment({{ checkoutToken: TOKEN, paymentMode:'NetBanking', bankCode: bank }}, 'payNbBtn', 'nbError', '<span>Pay {session.Currency} {amount}</span>');
}}

// Tab switching
function switchTab(mode) {{
  document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
  const btn = document.querySelector(`[data-mode='${{mode}}']`);
  const content = document.getElementById(`tab-${{mode}}`);
  if (btn) btn.classList.add('active');
  if (content) content.classList.add('active');
}}

// Card number formatting
const cardInput = document.getElementById('cardNumber');
if (cardInput) {{
  cardInput.addEventListener('input', function() {{
    let val = this.value.replace(/\D/g,'').substring(0,16);
    this.value = val.match(/.{{1,4}}/g)?.join(' ') || val;
    const brand = document.getElementById('cardBrand');
    if (brand) {{
      if (val.startsWith('4')) brand.textContent = '💳 Visa';
      else if (val.startsWith('5')) brand.textContent = '💳 MC';
      else if (val.startsWith('34') || val.startsWith('37')) brand.textContent = '💳 Amex';
      else if (val.startsWith('6')) brand.textContent = '💳 Rupay';
      else brand.textContent = '💳';
    }}
  }});
}}

// Expiry formatting
const expiryInput = document.getElementById('cardExpiry');
if (expiryInput) {{
  expiryInput.addEventListener('input', function() {{
    let val = this.value.replace(/\D/g,'');
    if (val.length >= 2) val = val.substring(0,2) + ' / ' + val.substring(2,4);
    this.value = val;
  }});
}}

// Activate first tab
if (MODES.length > 0) switchTab(MODES[0]);
</script>
</body>
</html>";
        }

        private static string BuildCustomerInfoHtml(CheckoutSessionResponse session)
        {
            if (string.IsNullOrEmpty(session.CustomerName) && string.IsNullOrEmpty(session.CustomerEmail)) return "";
            return $@"<div class=""customer-info"">
      {(!string.IsNullOrEmpty(session.CustomerName) ? $"<div>👤 {session.CustomerName}</div>" : "")}
      {(!string.IsNullOrEmpty(session.CustomerEmail) ? $"<div>✉ {session.CustomerEmail}</div>" : "")}
      {(!string.IsNullOrEmpty(session.CustomerPhone) ? $"<div>📱 {session.CustomerPhone}</div>" : "")}
    </div>";
        }

        private static string BuildPaymentModeTabs(List<string> modes, string color)
        {
            var icons = new Dictionary<string, string> { ["Card"] = "💳 Card", ["UPI"] = "⚡ UPI", ["NetBanking"] = "🏦 Net Banking" };
            var result = "";
            var first = true;
            foreach (var mode in modes)
            {
                var label = icons.TryGetValue(mode, out var lbl) ? lbl : mode;
                var active = first ? "active" : "";
                result += $"<button class='tab-btn {active}' data-mode='{mode}' onclick=\"switchTab('{mode}')\">{label}</button>";
                first = false;
            }
            return result;
        }

        private static string BuildSuccessPage(string orderId, decimal amount, string currency, string message)
        {
            return $@"<!DOCTYPE html>
<html><head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>Payment Successful | BankUPG</title>
<style>body{{font-family:'Segoe UI',sans-serif;background:linear-gradient(135deg,#43e97b 0%,#38f9d7 100%);min-height:100vh;display:flex;align-items:center;justify-content:center;}}
.card{{background:#fff;border-radius:20px;padding:48px 40px;text-align:center;max-width:420px;box-shadow:0 20px 60px rgba(0,0,0,0.1);}}
.icon{{font-size:64px;margin-bottom:16px;}}
h1{{color:#2e7d32;font-size:26px;margin-bottom:8px;}}
p{{color:#555;font-size:14px;line-height:1.6;}}
.badge{{background:#e8f5e9;border-radius:8px;padding:12px 20px;margin-top:20px;font-size:13px;color:#2e7d32;}}
</style></head>
<body><div class=""card""><div class=""icon"">✅</div>
<h1>Payment Successful</h1>
<p>{message}</p>
{(amount > 0 ? $"<div class='badge'>Amount: {currency} {amount:N2}</div>" : "")}
<p style=""margin-top:16px;font-size:12px;color:#aaa;"">Powered by BankUPG</p>
</div></body></html>";
        }

        private static string BuildErrorPage(string message)
        {
            return $@"<!DOCTYPE html>
<html><head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>Payment Error | BankUPG</title>
<style>body{{font-family:'Segoe UI',sans-serif;background:linear-gradient(135deg,#f093fb 0%,#f5576c 100%);min-height:100vh;display:flex;align-items:center;justify-content:center;}}
.card{{background:#fff;border-radius:20px;padding:48px 40px;text-align:center;max-width:420px;box-shadow:0 20px 60px rgba(0,0,0,0.1);}}
.icon{{font-size:64px;margin-bottom:16px;}}
h1{{color:#c62828;font-size:24px;margin-bottom:8px;}}
p{{color:#555;font-size:14px;}}
</style></head>
<body><div class=""card""><div class=""icon"">❌</div>
<h1>Payment Error</h1>
<p>{message}</p>
<p style=""margin-top:16px;font-size:12px;color:#aaa;"">Powered by BankUPG</p>
</div></body></html>";
        }
    }
}
