/**
 * BankUPG Web Checkout SDK v1.1
 *
 * Integration (3 lines for the merchant):
 *   1.  Include this script on your checkout page:
 *       <script src="https://apipg.banku.co.in/checkout.js"></script>
 *
 *   2.  Create an order on YOUR server (never expose X-Api-Key on the client):
 *       POST /api/checkout/orders  →  response.data.checkoutUrl
 *
 *   3.  Open the checkout modal:
 *       BankUPG.open({
 *           key:          "your_api_key",
 *           checkout_url: res.data.checkoutUrl,   // from step 2
 *           handler: function(response) {
 *               // Called on successful payment — send to your server to verify
 *               console.log(response.payment_id);   // pay_XXXX
 *               console.log(response.order_id);     // order_XXXX
 *               console.log(response.signature);    // HMAC signature
 *           },
 *           modal: {
 *               ondismiss: function() { console.log("Checkout closed"); }
 *           }
 *       });
 */

(function (global) {
    'use strict';

    var SDK_VERSION = '1.1.0';
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
                BankUPG._closeModal();
                if (typeof handler === 'function') {
                    handler({
                        payment_id:   data.payment_id,
                        order_id:     data.order_id,
                        signature:    data.signature,
                        amount:       data.amount,
                        payment_mode: data.payment_mode,
                        paid_at:      data.paid_at
                    });
                }
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
                response.payment_id &&
                response.order_id &&
                response.signature);
        },

        version: SDK_VERSION
    };

    // Expose globally
    global.BankUPG = BankUPG;

}(window));
