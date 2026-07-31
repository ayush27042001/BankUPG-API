# BankUPG — Payment Gateway Web Integration Guide

> **Version:** 1.0 | **Last Updated:** July 2026  
> **Base URL (Production):** `https://pg.bankupg.com`  
> **Base URL (Staging):** `https://staging-pg.bankupg.com`

---

## Table of Contents

1. [Overview](#1-overview)
2. [How It Works — Full Flow](#2-how-it-works--full-flow)
3. [Getting Started](#3-getting-started)
4. [Authentication](#4-authentication)
5. [Create a Checkout Order](#5-create-a-checkout-order)
6. [Open the Checkout Page](#6-open-the-checkout-page)
7. [Embed via JS SDK (Modal)](#7-embed-via-js-sdk-modal)
8. [Payment Methods Supported](#8-payment-methods-supported)
9. [After Payment — Callback & Redirect](#9-after-payment--callback--redirect)
10. [Verify Payment (Server-Side — CRITICAL)](#10-verify-payment-server-side--critical)
11. [Get Order Status](#11-get-order-status)
12. [Transaction Charges (MDR)](#12-transaction-charges-mdr)
13. [Checkout Customization API](#13-checkout-customization-api)
14. [Webhook Events](#14-webhook-events)
15. [Test Data](#15-test-data)
16. [Signature Verification — Code Examples](#16-signature-verification--code-examples)
17. [API Error Codes](#17-api-error-codes)
18. [Frequently Asked Questions](#18-frequently-asked-questions)

---

## 1. Overview

BankUPG provides a **hosted Web Checkout** experience similar to Razorpay / PayU. You create an order server-side, get a `checkoutUrl`, and direct your customer there. BankUPG handles the payment UI, validation, and processing. After payment you get a callback with a signed payload which you verify on your server.

```
Your Server  ──►  BankUPG API (create order)
                         │
Customer Browser  ──►  BankUPG Hosted Checkout Page
                         │
                  Customer Pays (Card / UPI / NetBanking / Wallet / EMI / PayLater)
                         │
Your callbackUrl  ◄──  Redirect with signed params
                         │
Your Server  ──►  BankUPG API (verify payment)  ✅ Confirmed
```

---

## 2. How It Works — Full Flow

| Step | Who | Action |
|------|-----|--------|
| 1 | **Your server** | `POST /api/checkout/orders` → receive `checkoutUrl` |
| 2 | **Your frontend** | Redirect customer to `checkoutUrl` OR open it in a JS modal |
| 3 | **Customer** | Selects payment method and pays on BankUPG-hosted page |
| 4 | **BankUPG** | Redirects to your `callbackUrl` with payment ID + signature |
| 5 | **Your server** | `POST /api/checkout/verify` → confirm signature & mark order paid |

---

## 3. Getting Started

### 3.1 Obtain API Credentials

Log in to the BankUPG Merchant Dashboard and navigate to:
**Settings → API Keys**

You will find:

| Credential | Description | Used In |
|------------|-------------|---------|
| `api_key` | Public key to identify your merchant | `X-Api-Key` header |
| `api_salt` | Secret used for HMAC signature | Server-side signature verification only |
| `client_id` | OAuth client ID (optional) | Alternative auth |
| `client_secret` | OAuth client secret (optional) | Alternative auth |

> ⚠️ **Never expose `api_salt` in frontend/client code.** It is only used server-side for HMAC verification.

### 3.2 Environments

| Environment | Base URL | Notes |
|-------------|----------|-------|
| **Production** | `https://pg.bankupg.com` | Live transactions |
| **Staging** | `https://staging-pg.bankupg.com` | Use test cards |

---

## 4. Authentication

All **server-side API calls** use API Key authentication:

```http
X-Api-Key: rzp_test_xxxxxxxxxxxxxxxxx
Content-Type: application/json
```

The **hosted checkout page** (`/checkout/{token}`) and **JS SDK** require no authentication — they use the signed token issued when you create the order.

---

## 5. Create a Checkout Order

### Endpoint

```
POST /api/checkout/orders
Header: X-Api-Key: <your_api_key>
```

### Request Body

```json
{
  "amount": 49900,
  "currency": "INR",
  "orderRef": "ORD-20260730-001",
  "customerName": "Ramesh Kumar",
  "customerEmail": "ramesh@example.com",
  "customerPhone": "9876543210",
  "notes": "Order for product XYZ",
  "callbackUrl": "https://yoursite.com/payment/callback",
  "cancelUrl": "https://yoursite.com/payment/cancel"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | decimal | ✅ | Amount in smallest currency unit (paise for INR). ₹499 = `49900` |
| `currency` | string | ❌ | Default `"INR"` |
| `orderRef` | string | ✅ | Your unique order reference ID (max 200 chars) |
| `customerName` | string | ❌ | Pre-fills name on checkout page |
| `customerEmail` | string | ❌ | Pre-fills email on checkout page |
| `customerPhone` | string | ❌ | Pre-fills phone on checkout page |
| `notes` | string | ❌ | Internal notes (max 500 chars) |
| `callbackUrl` | string | ❌ | URL to redirect after payment (max 1000 chars) |
| `cancelUrl` | string | ❌ | URL to redirect if customer cancels |

### Response

```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "orderId": "order_12345",
    "checkoutToken": "MTIzNDU6YWJjZGVmZ2g",
    "checkoutUrl": "https://pg.bankupg.com/checkout/MTIzNDU6YWJjZGVmZ2g",
    "amount": 49900,
    "currency": "INR",
    "orderRef": "ORD-20260730-001",
    "customerName": "Ramesh Kumar",
    "customerEmail": "ramesh@example.com",
    "customerPhone": "9876543210",
    "status": "created",
    "expiryDate": "2026-07-30T14:30:00Z",
    "createdDate": "2026-07-30T14:00:00Z"
  }
}
```

> **Order expiry:** 30 minutes from creation. A new order must be created if it expires.

---

## 6. Open the Checkout Page

### Option A — Direct Redirect (Simplest)

After creating the order, redirect the browser to `checkoutUrl`:

```javascript
// Node.js / Express example
const order = await createCheckoutOrder({ amount: 49900, ... });
res.redirect(order.data.checkoutUrl);
```

```html
<!-- HTML form example -->
<script>
  window.location.href = "{{ checkoutUrl }}";
</script>
```

### Option B — New Tab / Window

```javascript
window.open(order.data.checkoutUrl, '_blank');
```

The checkout page automatically:
- Loads merchant branding (logo, colors)
- Shows the amount and customer info
- Presents all enabled payment methods in an accordion layout
- Handles form validation and payment submission
- Redirects to `callbackUrl` after payment

---

## 7. Embed via JS SDK (Modal)

Load the BankUPG JS SDK once on your page:

```html
<script src="https://pg.bankupg.com/checkout.js"></script>
```

Then open the modal:

```javascript
var handler = BankUPG.open({
  key:          "your_api_key",
  order_id:     "order_12345",
  checkout_url: "https://pg.bankupg.com/checkout/MTIzNDU6YWJjZGVmZ2g",
  amount:       49900,
  currency:     "INR",
  name:         "Your Company Name",
  description:  "Product purchase",
  image:        "https://yoursite.com/logo.png",   // Optional — overrides merchant logo
  prefill: {
    name:    "Ramesh Kumar",
    email:   "ramesh@example.com",
    contact: "9876543210"
  },
  theme: {
    color: "#009688"    // Optional — overrides primary color
  },
  handler: function(response) {
    // Called on successful payment
    // ALWAYS verify this on your server before marking order as paid
    fetch('/verify-payment', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        bankupg_payment_id: response.bankupg_payment_id,
        bankupg_order_id:   response.bankupg_order_id,
        bankupg_signature:  response.bankupg_signature
      })
    });
  },
  modal: {
    ondismiss: function() {
      console.log("Customer closed the checkout modal");
    }
  }
});
```

### SDK Handler Response Object

```json
{
  "bankupg_payment_id": "pay_ABCDEF1234567890",
  "bankupg_order_id":   "order_12345",
  "bankupg_signature":  "a3f8c2e1b4d5...sha256hex...",
  "amount":             49900,
  "payment_mode":       "Card",
  "paid_at":            "2026-07-30T13:45:22.000Z"
}
```

---

## 8. Payment Methods Supported

| Method | Type | Network / Sub-type |
|--------|------|--------------------|
| **Card** | Credit & Debit | Visa, Mastercard, RuPay, Amex |
| **UPI** | Real-time | Any UPI VPA (GPay, PhonePe, Paytm, BHIM…) |
| **Net Banking** | Bank Transfer | 50+ banks |
| **Wallet** | Prepaid | Paytm, PhonePe, Amazon Pay, Mobikwik, Freecharge, JioMoney |
| **EMI** | Installments | 3 / 6 / 9 / 12 month tenures |
| **Pay Later** | BNPL | Simpl, LazyPay, ZestMoney |

### Enable / Disable Payment Methods

Payment methods shown on the checkout page are controlled per merchant via `MerchantPaymentMethods`. Contact your BankUPG account manager or use the SuperAdmin panel to configure enabled methods.

If no methods are configured the default set shown is: **UPI, Card, NetBanking, Wallet, EMI, PayLater**.

---

## 9. After Payment — Callback & Redirect

After the customer completes (or fails) payment, BankUPG redirects to your `callbackUrl`:

### Success Redirect

```
https://yoursite.com/payment/callback
  ?bankupg_payment_id=pay_ABCDEF1234567890
  &bankupg_order_id=order_12345
  &bankupg_signature=a3f8c2e1b4d5...
  &status=success
```

### Failed Redirect

```
https://yoursite.com/payment/callback
  ?bankupg_order_id=order_12345
  &status=failed
```

> ⚠️ **Do NOT trust the `status` query parameter alone.** Always call `POST /api/checkout/verify` to confirm a successful payment using the HMAC signature.

---

## 10. Verify Payment (Server-Side — CRITICAL)

This is the most important step. **Never mark an order as paid without server-side verification.**

### Endpoint

```
POST /api/checkout/verify
Header: X-Api-Key: <your_api_key>
```

### Request Body

```json
{
  "bankupgPaymentId": "pay_ABCDEF1234567890",
  "bankupgOrderId":   "order_12345",
  "bankupgSignature": "a3f8c2e1b4d5..."
}
```

### Success Response

```json
{
  "success": true,
  "data": {
    "isValid": true,
    "paymentId": "pay_ABCDEF1234567890",
    "orderId": "order_12345",
    "status": "success",
    "amount": 49900,
    "paymentMode": "Card",
    "paidAt": "2026-07-30T13:45:22Z",
    "message": "Payment verified successfully."
  }
}
```

### Failure Response (tampered signature)

```json
{
  "success": true,
  "data": {
    "isValid": false,
    "message": "Signature verification failed."
  }
}
```

### Signature Algorithm

```
HMAC-SHA256(
  message = payment_id + "|" + order_id,
  key     = api_salt
)
→ hex-encoded lowercase string
```

See [Section 16](#16-signature-verification--code-examples) for code examples in multiple languages.

---

## 11. Get Order Status

Poll the status of any order at any time.

### Endpoint

```
GET /api/checkout/orders/{orderId}
Header: X-Api-Key: <your_api_key>
```

### Response

```json
{
  "success": true,
  "data": {
    "orderId": "order_12345",
    "orderRef": "ORD-20260730-001",
    "amount": 49900,
    "currency": "INR",
    "status": "paid",
    "paymentId": "pay_ABCDEF1234567890",
    "paymentMode": "UPI",
    "paidAt": "2026-07-30T13:45:22Z",
    "createdDate": "2026-07-30T13:30:00Z",
    "expiryDate": "2026-07-30T14:00:00Z"
  }
}
```

| `status` | Meaning |
|----------|---------|
| `created` | Order created, awaiting payment |
| `paid` | Payment successful |
| `failed` | Payment failed |
| `expired` | Order expired (30 min elapsed) |
| `cancelled` | Order cancelled by merchant |

---

## 12. Transaction Charges (MDR)

BankUPG automatically calculates the Merchant Discount Rate (MDR) for every successful payment and records it as a **TransactionCharge**.

### 12.1 How MDR is Applied

Each payment mode has a charge rate configured by BankUPG (SuperAdmin):

| Payment Mode | Network | Charge Type | Typical Rate |
|-------------|---------|-------------|-------------|
| Card | Visa / Mastercard | Percentage | ~1.8% |
| Card | RuPay | Percentage | ~0.0% – 1.0% |
| Card | Amex | Percentage | ~2.5% |
| UPI | — | Percentage | ~0.9% |
| Net Banking | — | Fixed / Percentage | ₹15 – ₹25 flat |
| Wallet | — | Percentage | ~1.5% |
| EMI | — | Percentage | ~2.0% – 3.0% |
| Pay Later | — | Percentage | ~2.5% |

> Actual rates are configured by BankUPG and may vary by merchant agreement.

### 12.2 Charge Calculation Formula

```
Charge Amount  = Transaction Amount × Rate%    (for Percentage type)
              OR Fixed Amount                  (for Fixed type)

  → Clamped between MinCharge and MaxCharge (if configured)

GST Amount     = Charge Amount × GST%  (typically 18%)
Total Deduction = Charge Amount + GST Amount
Net Amount     = Transaction Amount − Total Deduction
```

### 12.3 Card Network Auto-Detection

BankUPG detects the card network from the card number prefix and applies the correct rate:

| Prefix | Network |
|--------|---------|
| `4xxx` | Visa |
| `51xx`–`55xx` or `2221`–`2720` | Mastercard |
| `6xxx` | RuPay |
| `34xx` / `37xx` | Amex |

### 12.4 View Transaction Charges (Merchant)

```
GET /api/transaction-charges/by-transaction/{transactionId}
Authorization: Bearer <merchant_jwt_token>
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "transactionChargeId": 101,
      "transactionId": 5001,
      "mid": 1001,
      "paymentMethodType": "Card",
      "networkName": "Visa",
      "chargeType": "Percentage",
      "chargeValue": 1.80,
      "transactionAmount": 49900.00,
      "chargeAmount": 898.20,
      "gstAmount": 161.68,
      "totalDeduction": 1059.88,
      "netAmount": 48840.12,
      "createdDate": "2026-07-30T13:45:23Z"
    }
  ]
}
```

### 12.5 Recalculate Charges (SuperAdmin)

If MDR rates are updated, charges can be recalculated:

```
POST /api/transaction-charges/recalculate/{transactionId}
Authorization: Bearer <superadmin_jwt_token>
```

---

## 13. Checkout Customization API

Merchants can brand the checkout page with their logo, colors, and language.

### 13.1 Get Own Customization

```
GET /api/checkout-customizations/my
Authorization: Bearer <merchant_jwt_token>
```

**Response:**

```json
{
  "success": true,
  "data": {
    "checkoutCustomizationId": 42,
    "mid": 1001,
    "brandLogoUrl": "/uploads/checkout-assets/logo-1001-abc123.png",
    "primaryColor": "#009688",
    "secondaryColor": "#7c3aed",
    "language": "English",
    "ownerSignatureUrl": "/uploads/checkout-assets/signature-1001-def456.png",
    "createdDate": "2026-07-01T10:00:00Z",
    "updatedDate": "2026-07-30T12:00:00Z"
  }
}
```

### 13.2 Save / Update Customization (Upsert)

Creates the record if it does not exist, updates it if it does.

```
PUT /api/checkout-customizations/my
Authorization: Bearer <merchant_jwt_token>
Content-Type: application/json
```

**Request:**

```json
{
  "brandLogoUrl": "https://yoursite.com/logo.png",
  "primaryColor": "#1a73e8",
  "secondaryColor": "#e91e63",
  "language": "English",
  "ownerSignatureUrl": "https://yoursite.com/signature.png"
}
```

> **Partial update supported:** Only non-null fields are updated. Send only the fields you want to change.

**Supported Languages:** `English`, `Hindi`, `Tamil`, `Telugu`, `Kannada`, `Marathi`, `Bengali`, `Gujarati`

### 13.3 Upload Brand Logo

Upload an image file directly. The URL is automatically saved to the customization record.

```
POST /api/checkout-customizations/my/upload-logo
Authorization: Bearer <merchant_jwt_token>
Content-Type: multipart/form-data
```

**Form field:** `file` (image file)

**Constraints:**
- Allowed formats: `.jpg`, `.jpeg`, `.png`, `.svg`, `.webp`
- Maximum size: **2 MB**

**Response:**

```json
{
  "success": true,
  "message": "Logo uploaded",
  "data": {
    "url": "/uploads/checkout-assets/logo-1001-3f8a2b1c.png"
  }
}
```

### 13.4 Upload Owner / Signatory Signature

```
POST /api/checkout-customizations/my/upload-signature
Authorization: Bearer <merchant_jwt_token>
Content-Type: multipart/form-data
```

**Form field:** `file` (image file — same constraints as logo)

### 13.5 Customization Fields Reference

| Field | Description | Default |
|-------|-------------|---------|
| `primaryColor` | Main accent color (buttons, headers) | `#009688` |
| `secondaryColor` | Secondary accent color | `#7c3aed` |
| `brandLogoUrl` | Merchant logo shown on checkout left panel | *(initial of merchant name)* |
| `language` | UI language | `English` |
| `ownerSignatureUrl` | Authorised signatory signature (for PDFs/agreements) | — |

---

## 14. Webhook Events

Configure webhooks in the BankUPG dashboard to receive real-time payment notifications on your server.

### Supported Events

| Event | Trigger |
|-------|---------|
| `payment.success` | Payment completed successfully |
| `payment.failed` | Payment attempt failed |
| `refund.processed` | Refund credited |
| `chargeback.created` | Chargeback raised |

### Webhook Payload (payment.success)

```json
{
  "event_type": "payment.success",
  "payment_id": "pay_ABCDEF1234567890",
  "order_id": "order_12345",
  "amount": 49900,
  "currency": "INR",
  "created_at": "2026-07-30T13:45:22Z"
}
```

### Retry Policy

BankUPG retries failed webhook deliveries up to **3 times** with exponential backoff. Your endpoint must return HTTP `200` to acknowledge receipt.

### Security

Verify the webhook payload by recomputing the signature using your `api_salt`. Discard any webhook that fails signature verification.

---

## 15. Test Data

### Test Cards

| Card Number | Network | Expected Result |
|-------------|---------|-----------------|
| `4111 1111 1111 1111` | Visa | ✅ Success |
| `4012 8888 8888 1881` | Visa | ✅ Success |
| `5200 0000 0000 0007` | Mastercard | ✅ Success |
| `5105 1051 0510 5100` | Mastercard | ✅ Success |
| `6521 0000 0000 0001` | RuPay | ✅ Success |
| `3782 8224 6310 005` | Amex (15 digit) | ✅ Success |
| `4000 0000 0000 0002` | Visa | ❌ Card Declined |
| `4000 0000 0000 9995` | Visa | ❌ Insufficient Funds |
| `4000 0000 0000 9987` | Visa | ❌ Do Not Honour |

**Test CVV:** Any 3-digit number (e.g. `123`)  
**Test Expiry:** Any future date (e.g. `12/28`)

### Test UPI VPAs

| VPA | Result |
|-----|--------|
| Any valid VPA (e.g. `test@okhdfc`) | ✅ Success |
| `fail@upi` | ❌ Payment Failed |

### Test Net Banking

| Bank Code | Result |
|-----------|--------|
| Any valid bank code | ✅ Success |
| `fail` | ❌ Payment Failed |

---

## 16. Signature Verification — Code Examples

### Algorithm

```
signature = HMAC-SHA256(
  message = payment_id + "|" + order_id,
  key     = api_salt
).to_hex_lowercase()
```

### Node.js

```javascript
const crypto = require('crypto');

function verifyPayment(paymentId, orderId, receivedSignature, apiSalt) {
  const message   = `${paymentId}|${orderId}`;
  const expected  = crypto
    .createHmac('sha256', apiSalt)
    .update(message)
    .digest('hex');

  return expected === receivedSignature.toLowerCase();
}
```

### Python

```python
import hmac, hashlib

def verify_payment(payment_id, order_id, received_signature, api_salt):
    message  = f"{payment_id}|{order_id}"
    expected = hmac.new(
        api_salt.encode('utf-8'),
        message.encode('utf-8'),
        hashlib.sha256
    ).hexdigest()
    return expected == received_signature.lower()
```

### PHP

```php
function verifyPayment($paymentId, $orderId, $receivedSignature, $apiSalt) {
    $message  = $paymentId . '|' . $orderId;
    $expected = hash_hmac('sha256', $message, $apiSalt);
    return hash_equals($expected, strtolower($receivedSignature));
}
```

### Java

```java
import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

public static boolean verifyPayment(String paymentId, String orderId,
                                    String receivedSig, String apiSalt) throws Exception {
    String message = paymentId + "|" + orderId;
    Mac mac = Mac.getInstance("HmacSHA256");
    mac.init(new SecretKeySpec(apiSalt.getBytes("UTF-8"), "HmacSHA256"));
    byte[] hash = mac.doFinal(message.getBytes("UTF-8"));
    StringBuilder sb = new StringBuilder();
    for (byte b : hash) sb.append(String.format("%02x", b));
    return sb.toString().equalsIgnoreCase(receivedSig);
}
```

### C# (.NET)

```csharp
using System.Security.Cryptography;
using System.Text;

bool VerifyPayment(string paymentId, string orderId,
                   string receivedSignature, string apiSalt)
{
    var message = $"{paymentId}|{orderId}";
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSalt));
    var hash = BitConverter.ToString(
        hmac.ComputeHash(Encoding.UTF8.GetBytes(message))
    ).Replace("-", "").ToLowerInvariant();

    return hash == receivedSignature.ToLowerInvariant();
}
```

---

## 17. API Error Codes

### HTTP Status Codes

| Status | Meaning |
|--------|---------|
| `200 OK` | Request succeeded |
| `400 Bad Request` | Validation error or bad input |
| `401 Unauthorized` | Invalid or missing API key / token |
| `403 Forbidden` | Authenticated but insufficient permissions |
| `404 Not Found` | Resource does not exist |
| `500 Internal Server Error` | BankUPG server error |

### Application Error Response Format

```json
{
  "success": false,
  "message": "Human-readable error description",
  "errors": [
    "Specific field validation error 1",
    "Specific field validation error 2"
  ]
}
```

### Common Error Messages

| Message | Cause | Fix |
|---------|-------|-----|
| `Invalid API key.` | Wrong or revoked `api_key` | Re-check dashboard API key |
| `Merchant account is not active.` | Merchant not yet activated | Contact BankUPG support |
| `Order already paid.` | Duplicate payment attempt | Check order status before retrying |
| `This payment session has expired.` | Order > 30 min old | Create a new order |
| `Signature verification failed.` | Tampered params or wrong `api_salt` | Recompute HMAC with correct salt |
| `Invalid order ID.` | Order ID format wrong | Ensure format is `order_XXXXX` |
| `Payment not found.` | `payment_id` doesn't match order | Verify correct payment/order IDs |

---

## 18. Frequently Asked Questions

**Q: Is the amount in rupees or paise?**  
A: **Paise.** ₹499.00 = `49900`. ₹1 = `100`. Always multiply rupees × 100.

**Q: Can I use BankUPG without the hosted checkout page?**  
A: The hosted page is the recommended approach. Custom/direct API payment flows require additional integration and are not covered in this guide.

**Q: How do I enable only certain payment methods (e.g., only UPI + Card)?**  
A: Configure `MerchantPaymentMethods` through your BankUPG account manager or the SuperAdmin panel.

**Q: How long is a checkout token valid?**  
A: 30 minutes from the time the order was created. Create a fresh order if it expires.

**Q: Will I be charged MDR even in the staging environment?**  
A: No. MDR is only applied in production. Staging transactions have no financial impact.

**Q: Can I pre-fill customer info so they don't have to type it?**  
A: Yes. Pass `customerName`, `customerEmail`, and `customerPhone` when creating the order. The checkout page will pre-fill those fields.

**Q: What happens if the customer closes the checkout page without paying?**  
A: The order remains in `created` status until it expires. No charge is applied. You can detect this via the `modal.ondismiss` callback in the JS SDK.

**Q: How do I customise the checkout page look and feel?**  
A: Use the Checkout Customization API (Section 13) to set your brand logo, primary/secondary colors, and language. All changes apply immediately to the live checkout page.

**Q: Is the card number stored by BankUPG?**  
A: No. Card numbers are never persisted. Only the detected network (Visa, Mastercard, etc.) is stored for charge calculation purposes.

**Q: How do I recalculate MDR if the rate changes?**  
A: Use `POST /api/transaction-charges/recalculate/{transactionId}` (SuperAdmin only). This updates the existing `TransactionCharge` record with the latest configured rate.

---

## Quick Reference — All Checkout Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/checkout/orders` | `X-Api-Key` | Create order |
| `GET` | `/checkout/{token}` | None | Hosted checkout page |
| `POST` | `/api/checkout/pay` | None (token-based) | Submit payment (called by checkout page) |
| `POST` | `/api/checkout/verify` | `X-Api-Key` | Verify payment signature |
| `GET` | `/api/checkout/orders/{orderId}` | `X-Api-Key` | Get order status |
| `GET` | `/api/checkout-customizations/my` | `Bearer JWT` | Get merchant customization |
| `PUT` | `/api/checkout-customizations/my` | `Bearer JWT` | Save/update customization |
| `POST` | `/api/checkout-customizations/my/upload-logo` | `Bearer JWT` | Upload brand logo |
| `POST` | `/api/checkout-customizations/my/upload-signature` | `Bearer JWT` | Upload signature image |
| `GET` | `/api/transaction-charges/by-transaction/{id}` | `Bearer JWT` | Get MDR charges for a transaction |
| `POST` | `/api/payment-method-charges` | `Bearer JWT (SuperAdmin)` | Configure MDR rates |
| `GET` | `/checkout.js` | None | JS SDK script |

---

*© 2026 BankUPG Payment Services. All rights reserved.*  
*For technical support: support@bankupg.com*
