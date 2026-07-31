/**
 * BankUPG Web Checkout SDK v1.0
 * Usage:
 *   <script src="https://your-api-domain.com/checkout.js"></script>
 *   var handler = BankUPG.open({
 *       key:          "your_api_key",        // from Merchant Dashboard
 *       order_id:     "order_12345",          // from POST /api/checkout/orders
 *       checkout_url: "https://your-api-domain.com/checkout/TOKEN",
 *       amount:       50000,                  // in paise (50000 = ₹500.00)
 *       currency:     "INR",
 *       name:         "Acme Corp",
 *       description:  "Order #12345",
 *       image:        "https://example.com/logo.png",
 *       prefill: {
 *           name:    "Ramesh Kumar",
 *           email:   "ramesh@example.com",
 *           contact: "9876543210"
 *       },
 *       theme: { color: "#1a73e8" },
 *       handler: function(response) {
 *           // Called on successful payment
 *           console.log(response.bankupg_payment_id);
 *           console.log(response.bankupg_order_id);
 *           console.log(response.bankupg_signature);
 *           // POST these to your server for verification
 *       },
 *       modal: {
 *           ondismiss: function() { console.log("Modal dismissed"); }
 *       }
 *   });
 */

(function (global) {
    'use strict';

    var SDK_VERSION = '1.0.0';
    var BRAND_NAME = 'BankUPG';

    // ─── Utility ──────────────────────────────────────────────────────────────

    function injectStyles() {
        if (document.getElementById('bankupg-styles')) return;
        var style = document.createElement('style');
        style.id = 'bankupg-styles';
        style.textContent = [
            '#bankupg-overlay{position:fixed;inset:0;background:rgba(0,0,0,0.55);z-index:99998;display:flex;align-items:center;justify-content:center;backdrop-filter:blur(4px);}',
            '#bankupg-modal{width:100%;max-width:860px;height:90vh;max-height:680px;border-radius:16px;overflow:hidden;background:#fff;box-shadow:0 32px 80px rgba(0,0,0,0.3);border:none;z-index:99999;display:flex;flex-direction:column;}',
            '#bankupg-frame{flex:1;border:none;width:100%;height:100%;}',
            '#bankupg-close{position:absolute;top:16px;right:16px;width:32px;height:32px;background:rgba(255,255,255,0.9);border:none;border-radius:50%;cursor:pointer;font-size:18px;line-height:1;display:flex;align-items:center;justify-content:center;box-shadow:0 2px 8px rgba(0,0,0,0.15);z-index:100000;}',
            '#bankupg-close:hover{background:#fff;transform:scale(1.1);}',
            '#bankupg-overlay.bankupg-fade-in{animation:bgFadeIn 0.2s ease;}',
            '#bankupg-modal.bankupg-slide-in{animation:modalIn 0.25s cubic-bezier(0.34,1.56,0.64,1);}',
            '@keyframes bgFadeIn{from{opacity:0}to{opacity:1}}',
            '@keyframes modalIn{from{opacity:0;transform:scale(0.92) translateY(20px)}to{opacity:1;transform:scale(1) translateY(0)}}',
            '@media(max-width:600px){#bankupg-modal{height:100vh;max-height:100vh;border-radius:0;max-width:100%;}}'
        ].join('');
        document.head.appendChild(style);
    }

    function createOverlay() {
        var overlay = document.createElement('div');
        overlay.id = 'bankupg-overlay';
        overlay.className = 'bankupg-fade-in';
        return overlay;
    }

    function createModal(checkoutUrl, opts) {
        var modal = document.createElement('div');
        modal.id = 'bankupg-modal';
        modal.className = 'bankupg-slide-in';

        var closeBtn = document.createElement('button');
        closeBtn.id = 'bankupg-close';
        closeBtn.innerHTML = '&#x2715;';
        closeBtn.title = 'Close';

        var iframe = document.createElement('iframe');
        iframe.id = 'bankupg-frame';
        iframe.src = checkoutUrl;
        iframe.allow = 'payment';
        iframe.setAttribute('allowpaymentrequest', '');

        modal.appendChild(closeBtn);
        modal.appendChild(iframe);

        return { modal: modal, closeBtn: closeBtn, iframe: iframe };
    }

    // ─── Message Handler (cross-frame communication) ───────────────────────────

    function listenForPaymentResult(handler, dismissCallback) {
        function onMessage(event) {
            var data = event.data;
            if (!data || data.source !== BRAND_NAME) return;

            if (data.event === 'payment.success') {
                window.removeEventListener('message', onMessage);
                if (typeof handler === 'function') {
                    handler({
                        bankupg_payment_id: data.payment_id,
                        bankupg_order_id: data.order_id,
                        bankupg_signature: data.signature,
                        amount: data.amount,
                        payment_mode: data.payment_mode,
                        paid_at: data.paid_at
                    });
                }
                BankUPG._closeModal();
            }

            if (data.event === 'payment.dismiss') {
                window.removeEventListener('message', onMessage);
                if (typeof dismissCallback === 'function') dismissCallback();
                BankUPG._closeModal();
            }
        }

        window.addEventListener('message', onMessage);
        return onMessage;
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    var BankUPG = {

        _overlay: null,
        _messageHandler: null,

        /**
         * Open the checkout modal.
         * @param {Object} options
         */
        open: function (options) {
            if (!options) throw new Error('BankUPG.open: options are required.');
            if (!options.key) throw new Error('BankUPG.open: key is required.');
            if (!options.checkout_url) throw new Error('BankUPG.open: checkout_url is required.');

            injectStyles();

            var checkoutUrl = options.checkout_url;
            var handler = (options.handler) ? options.handler : null;
            var dismissCb = (options.modal && options.modal.ondismiss) ? options.modal.ondismiss : null;

            var overlay = createOverlay();
            var parts = createModal(checkoutUrl, options);

            // Close on overlay click
            overlay.addEventListener('click', function (e) {
                if (e.target === overlay) {
                    if (typeof dismissCb === 'function') dismissCb();
                    BankUPG._closeModal();
                }
            });

            parts.closeBtn.addEventListener('click', function () {
                if (typeof dismissCb === 'function') dismissCb();
                BankUPG._closeModal();
            });

            overlay.appendChild(parts.modal);
            document.body.appendChild(overlay);
            BankUPG._overlay = overlay;

            // Listen for cross-frame payment events
            BankUPG._messageHandler = listenForPaymentResult(handler, dismissCb);

            return {
                close: function () { BankUPG._closeModal(); }
            };
        },

        _closeModal: function () {
            if (BankUPG._overlay) {
                BankUPG._overlay.remove();
                BankUPG._overlay = null;
            }
            if (BankUPG._messageHandler) {
                window.removeEventListener('message', BankUPG._messageHandler);
                BankUPG._messageHandler = null;
            }
        },

        /**
         * Helper: Verify payment signature on client (for quick UI check only).
         * Always do server-side verification via POST /api/checkout/verify.
         */
        validateResponse: function (response) {
            return !!(response &&
                response.bankupg_payment_id &&
                response.bankupg_order_id &&
                response.bankupg_signature);
        },

        version: SDK_VERSION
    };

    // Expose globally
    global.BankUPG = BankUPG;

}(window));
