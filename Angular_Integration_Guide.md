# BankUPG — Angular Frontend Integration Guide
## Complete Implementation + Testing Reference

> **API Base URL:** `https://apipg.banku.co.in`  
> **Frontend URL:** `https://paymentgateway.banku.co.in`  
> **Hosted Checkout:** `https://apipg.banku.co.in/checkout/{token}`  
> **JS SDK:** `https://apipg.banku.co.in/checkout.js`  
> **Version:** 1.1 | August 2026

---

## Table of Contents

1. [Project Setup & Environment](#1-project-setup--environment)
2. [TypeScript Interfaces (DTOs)](#2-typescript-interfaces-dtos)
3. [Angular HTTP Interceptor (Auth)](#3-angular-http-interceptor-auth)
4. [Checkout Service](#4-checkout-service)
5. [Checkout Customization Service](#5-checkout-customization-service)
6. [Transaction Charge Service](#6-transaction-charge-service)
7. [Complete API Reference](#7-complete-api-reference)
8. [Step-by-Step Testing Guide](#8-step-by-step-testing-guide)
9. [Postman Collection Setup](#9-postman-collection-setup)
10. [Test Data Reference](#10-test-data-reference)
11. [Common Errors & Fixes](#11-common-errors--fixes)
12. [Testing Checklist](#12-testing-checklist)

---

## 1. Project Setup & Environment

### 1.1 Angular Environment Files

**`src/environments/environment.ts`** (Development)
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://apipg.banku.co.in',
  checkoutJsSdk: 'https://apipg.banku.co.in/checkout.js'
};
```

**`src/environments/environment.prod.ts`** (Production)
```typescript
export const environment = {
  production: true,
  apiBaseUrl: 'https://apipg.banku.co.in',
  checkoutJsSdk: 'https://apipg.banku.co.in/checkout.js'
};
```

### 1.2 Install Dependencies

```bash
npm install @angular/common @angular/forms @angular/router
```

### 1.3 `app.module.ts` — Register HttpClientModule

```typescript
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  imports: [
    HttpClientModule,
    // ...
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ]
})
export class AppModule {}
```

---

## 2. TypeScript Interfaces (DTOs)

Create `src/app/models/checkout.models.ts`:

```typescript
// ──────────────────────────────────────────────
// REQUEST MODELS
// ──────────────────────────────────────────────

export interface CreateOrderRequest {
  amount: number;          // in rupees — ₹499.00 = 499
  currency?: string;       // default 'INR'
  orderRef: string;        // your unique order ID
  customerName?: string;
  customerEmail?: string;
  customerPhone?: string;
  notes?: string;
  callbackUrl?: string;    // redirect after payment
  cancelUrl?: string;      // redirect on cancel
}

export interface VerifyPaymentRequest {
  bankupgPaymentId: string;
  bankupgOrderId: string;
  bankupgSignature: string;
}

// ──────────────────────────────────────────────
// RESPONSE MODELS
// ──────────────────────────────────────────────

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface CreateOrderResponse {
  orderId: string;
  checkoutToken: string;
  checkoutUrl: string;
  amount: number;
  currency: string;
  orderRef: string;
  customerName?: string;
  customerEmail?: string;
  customerPhone?: string;
  status: string;
  expiryDate: string;
  createdDate: string;
}

export interface OrderStatusResponse {
  orderId: string;
  orderRef: string;
  amount: number;
  currency: string;
  status: 'created' | 'paid' | 'failed' | 'expired' | 'cancelled';
  paymentId?: string;
  paymentMode?: string;
  paidAt?: string;
  createdDate: string;
  expiryDate: string;
}

export interface VerifyPaymentResponse {
  isValid: boolean;
  paymentId?: string;
  orderId?: string;
  status?: string;
  amount?: number;
  paymentMode?: string;
  paidAt?: string;
  message: string;
}

// ──────────────────────────────────────────────
// CHECKOUT CUSTOMIZATION
// ──────────────────────────────────────────────

export interface CheckoutCustomization {
  checkoutCustomizationId: number;
  mid: number;
  brandLogoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  language?: string;
  ownerSignatureUrl?: string;
  createdDate?: string;
  updatedDate?: string;
}

export interface UpsertCustomizationRequest {
  brandLogoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  language?: string;
  ownerSignatureUrl?: string;
}

export interface UploadAssetResponse {
  url: string;
}

// ──────────────────────────────────────────────
// TRANSACTION CHARGES
// ──────────────────────────────────────────────

export interface TransactionChargeDetail {
  transactionChargeId: number;
  transactionId: number;
  mid: number;
  paymentMethodType: string;
  networkName?: string;
  chargeType: string;
  chargeValue: number;
  transactionAmount: number;
  chargeAmount: number;
  gstAmount: number;
  totalDeduction: number;
  netAmount: number;
  createdDate: string;
}

// ──────────────────────────────────────────────
// JS SDK TYPES
// ──────────────────────────────────────────────

// Received via window.postMessage from the hosted checkout page
// when embedded as an iframe, OR via redirect callback URL query params
export interface BankUPGHandlerResponse {
  payment_id: string;    // e.g. "pay_ABCDEF1234567890"
  order_id: string;      // e.g. "order_12345"
  signature: string;     // HMAC-SHA256 to verify server-side
  amount: string;        // rupees as string e.g. "499.00"
  payment_mode: string;  // "Card", "UPI", "NetBanking", etc.
  paid_at: string;       // ISO 8601 timestamp
  source: string;        // "BankUPG"
  event: string;         // "payment.success"
}
```

---

## 3. Angular HTTP Interceptor (Auth)

Create `src/app/interceptors/auth.interceptor.ts`:

```typescript
import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('access_token');

    if (token) {
      const authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
      return next.handle(authReq);
    }

    return next.handle(req);
  }
}
```

> **Note:** The interceptor automatically adds `Authorization: Bearer <token>` to all requests.  
> For `X-Api-Key` based calls (server-side checkout order creation), pass the header explicitly in each request.

---

## 4. Checkout Service

Create `src/app/services/checkout.service.ts`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ApiResponse,
  CreateOrderRequest,
  CreateOrderResponse,
  OrderStatusResponse,
  VerifyPaymentRequest,
  VerifyPaymentResponse,
  BankUPGOptions
} from '../models/checkout.models';

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // ── Uses X-Api-Key (merchant server-side calls) ──────────────────────────

  /**
   * STEP 1: Create a checkout order.
   * ⚠️  Best practice: call this from your backend, not directly from Angular,
   *      so the X-Api-Key is never exposed to the browser.
   */
  createOrder(apiKey: string, request: CreateOrderRequest)
    : Observable<ApiResponse<CreateOrderResponse>> {
    const headers = new HttpHeaders({ 'X-Api-Key': apiKey });
    return this.http.post<ApiResponse<CreateOrderResponse>>(
      `${this.base}/api/checkout/orders`, request, { headers }
    );
  }

  /**
   * STEP 4: Verify payment signature after customer returns from checkout.
   * ALWAYS call this server-side before marking the order as paid.
   */
  verifyPayment(apiKey: string, request: VerifyPaymentRequest)
    : Observable<ApiResponse<VerifyPaymentResponse>> {
    const headers = new HttpHeaders({ 'X-Api-Key': apiKey });
    return this.http.post<ApiResponse<VerifyPaymentResponse>>(
      `${this.base}/api/checkout/verify`, request, { headers }
    );
  }

  /**
   * Get current status of an order by orderId (e.g. "order_12345").
   */
  getOrderStatus(apiKey: string, orderId: string)
    : Observable<ApiResponse<OrderStatusResponse>> {
    const headers = new HttpHeaders({ 'X-Api-Key': apiKey });
    return this.http.get<ApiResponse<OrderStatusResponse>>(
      `${this.base}/api/checkout/orders/${orderId}`, { headers }
    );
  }

  /**
   * STEP 2: Redirect customer to the hosted checkout page (full-page).
   */
  redirectToCheckout(checkoutUrl: string): void {
    window.location.href = checkoutUrl;
  }
}
```

---

## 5. Checkout Customization Service

Create `src/app/services/checkout-customization.service.ts`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ApiResponse,
  CheckoutCustomization,
  UpsertCustomizationRequest,
  UploadAssetResponse
} from '../models/checkout.models';

@Injectable({ providedIn: 'root' })
export class CheckoutCustomizationService {
  private base = `${environment.apiBaseUrl}/api/checkout-customizations`;

  constructor(private http: HttpClient) {}

  /** Get own checkout customization. Requires JWT (merchant login). */
  getMy(): Observable<ApiResponse<CheckoutCustomization>> {
    return this.http.get<ApiResponse<CheckoutCustomization>>(`${this.base}/my`);
  }

  /** Create or update own customization. Null fields are ignored (partial update). */
  upsertMy(request: UpsertCustomizationRequest)
    : Observable<ApiResponse<CheckoutCustomization>> {
    return this.http.put<ApiResponse<CheckoutCustomization>>(
      `${this.base}/my`, request
    );
  }

  /** Upload brand logo. Returns the hosted URL. */
  uploadLogo(file: File): Observable<ApiResponse<UploadAssetResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<UploadAssetResponse>>(
      `${this.base}/my/upload-logo`, formData
    );
  }

  /** Upload signatory signature image. Returns the hosted URL. */
  uploadSignature(file: File): Observable<ApiResponse<UploadAssetResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<UploadAssetResponse>>(
      `${this.base}/my/upload-signature`, formData
    );
  }
}
```

---

## 6. Transaction Charge Service

Create `src/app/services/transaction-charge.service.ts`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, TransactionChargeDetail } from '../models/checkout.models';

@Injectable({ providedIn: 'root' })
export class TransactionChargeService {
  private base = `${environment.apiBaseUrl}/api/transaction-charges`;

  constructor(private http: HttpClient) {}

  /** Get MDR charge breakdown for a specific transaction (merchant-owned). */
  getByTransaction(transactionId: number)
    : Observable<ApiResponse<TransactionChargeDetail[]>> {
    return this.http.get<ApiResponse<TransactionChargeDetail[]>>(
      `${this.base}/by-transaction/${transactionId}`
    );
  }
}
```

---

## 7. Complete API Reference

> **Base URL:** `https://apipg.banku.co.in`

### 7.1 Authentication Types

| Type | Header | Used For |
|------|--------|----------|
| API Key | `X-Api-Key: <your_api_key>` | Checkout order creation & verification |
| JWT Bearer | `Authorization: Bearer <token>` | Customization, charges, merchant dashboard |

---

### 7.2 Checkout APIs

#### `POST /api/checkout/orders`
Create a new checkout order.

- **Auth:** `X-Api-Key`
- **Content-Type:** `application/json`

**Request:**
```json
{
  "amount": 499.00,
  "currency": "INR",
  "orderRef": "ORD-20260730-001",
  "customerName": "Ramesh Kumar",
  "customerEmail": "ramesh@example.com",
  "customerPhone": "9876543210",
  "callbackUrl": "https://paymentgateway.banku.co.in/payment/callback",
  "cancelUrl": "https://paymentgateway.banku.co.in/payment/cancel"
}
```

**Response `200`:**
```json
{
  "success": true,
  "message": "Order created.",
  "data": {
    "orderId": "order_12345",
    "checkoutToken": "MTIzNDU6YWJjZGVmZ2g",
    "checkoutUrl": "https://apipg.banku.co.in/checkout/MTIzNDU6YWJjZGVmZ2g",
    "amount": 499.00,
    "currency": "INR",
    "orderRef": "ORD-20260730-001",
    "status": "created",
    "expiryDate": "2026-07-30T14:30:00Z",
    "createdDate": "2026-07-30T14:00:00Z"
  }
}
```

---

#### `GET /checkout/{token}`
Opens the BankUPG-hosted checkout UI in the customer's browser.

- **Auth:** None
- **Usage:** Direct full-page redirect **or** load in an `<iframe>` to receive results via `window.postMessage`

```
https://apipg.banku.co.in/checkout/MTIzNDU6YWJjZGVmZ2g
```

The page shows a two-panel branded checkout:
- **Left panel:** merchant logo/initials, order amount, customer info, secure badge
- **Right panel:** payment method accordion (UPI ⚡, Card 💳, Net Banking 🏦, Wallet 👛, EMI 📅, Pay Later ⏰)

**Integration approach A — Full-page redirect (recommended):**
```typescript
window.location.href = res.data.checkoutUrl;
// After payment, customer is redirected to callbackUrl with query params
```

**Integration approach B — Embedded iframe:**
```html
<iframe [src]="checkoutUrl" width="100%" height="600"></iframe>
```
```typescript
// Listen for postMessage from the checkout page
window.addEventListener('message', (event) => {
  if (event.data?.source === 'BankUPG' && event.data?.event === 'payment.success') {
    const { payment_id, order_id, signature, amount, payment_mode } = event.data;
    // Call your backend to verify
  }
  if (event.data?.event === 'payment.dismiss') {
    // Customer pressed Back
  }
});
```

---

#### `POST /api/checkout/verify`
Verify HMAC signature after payment. **Always call this server-side before confirming payment.**

- **Auth:** `X-Api-Key`

**Request:**
```json
{
  "bankupgPaymentId": "pay_ABCDEF1234567890",
  "bankupgOrderId": "order_12345",
  "bankupgSignature": "a3f8c2e1b4d5..."
}
```

**Response `200` — Valid:**
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

**Response `200` — Invalid signature:**
```json
{
  "success": true,
  "data": { "isValid": false, "message": "Signature verification failed." }
}
```

---

#### `GET /api/checkout/orders/{orderId}`
Get current status of any order.

- **Auth:** `X-Api-Key`
- **URL Param:** `orderId` — e.g. `order_12345`

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "orderId": "order_12345",
    "orderRef": "ORD-20260730-001",
    "amount": 499.00,
    "currency": "INR",
    "status": "paid",
    "paymentId": "pay_ABCDEF1234567890",
    "paymentMode": "UPI",
    "paidAt": "2026-07-30T13:45:22Z"
  }
}
```

---

### 7.3 Checkout Customization APIs

All endpoints require `Authorization: Bearer <jwt_token>` (merchant login).

---

#### `GET /api/checkout-customizations/my`
Get own merchant checkout branding.

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "checkoutCustomizationId": 42,
    "mid": 1001,
    "brandLogoUrl": "/uploads/checkout-assets/logo-1001-abc.png",
    "primaryColor": "#009688",
    "secondaryColor": "#7c3aed",
    "language": "English",
    "ownerSignatureUrl": null,
    "createdDate": "2026-07-01T10:00:00Z",
    "updatedDate": "2026-07-30T12:00:00Z"
  }
}
```

> If no customization is set yet, `data` will be `null` and `success` will be `true`.

---

#### `PUT /api/checkout-customizations/my`
Save or update branding. **Partial update** — only non-null fields are written.

**Request:**
```json
{
  "primaryColor": "#1a73e8",
  "secondaryColor": "#e91e63",
  "language": "Hindi"
}
```

**Response `200`:**
```json
{
  "success": true,
  "message": "Checkout customization saved",
  "data": { ...full customization object... }
}
```

---

#### `POST /api/checkout-customizations/my/upload-logo`
Upload brand logo image.

- **Content-Type:** `multipart/form-data`
- **Form field:** `file`
- **Allowed:** `.jpg .jpeg .png .svg .webp` — max **2 MB**

**Response `200`:**
```json
{
  "success": true,
  "message": "Logo uploaded",
  "data": { "url": "/uploads/checkout-assets/logo-1001-3f8a2b.png" }
}
```

---

#### `POST /api/checkout-customizations/my/upload-signature`
Upload authorised signatory signature.

- Same file rules as logo upload above.

**Response `200`:**
```json
{
  "success": true,
  "message": "Signature uploaded",
  "data": { "url": "/uploads/checkout-assets/signature-1001-xyz.png" }
}
```

---

### 7.4 Transaction Charge APIs

#### `GET /api/transaction-charges/by-transaction/{transactionId}`
Get MDR charge breakdown for a transaction. Merchants can only see their own.

- **Auth:** `Authorization: Bearer <jwt_token>`

**Response `200`:**
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

---

### 7.5 JS SDK

#### `GET /checkout.js`
Load the BankUPG JS SDK.

```html
<script src="https://apipg.banku.co.in/checkout.js"></script>
```

---

## 8. Step-by-Step Testing Guide

### Prerequisites
- BankUPG merchant account with status **Active**
- API Key + API Salt from dashboard
- JWT token from merchant login

---

### TEST 1 — Create Checkout Order

**Tool:** Postman / Angular  
**Endpoint:** `POST https://apipg.banku.co.in/api/checkout/orders`

```
Headers:
  X-Api-Key: YOUR_API_KEY
  Content-Type: application/json

Body:
{
  "amount": 499.00,
  "currency": "INR",
  "orderRef": "TEST-001",
  "customerName": "Test User",
  "customerEmail": "test@banku.co.in",
  "customerPhone": "9999999999",
  "callbackUrl": "https://paymentgateway.banku.co.in/payment/callback"
}
```

> **Amount is in rupees** (decimal). ₹499.00 → send `499` or `499.00`. Valid range: `1` to `10000000`.

**Expected:**
- `success: true`
- `data.status: "created"`
- `data.checkoutUrl` contains a valid URL

✅ **Save:** `data.orderId`, `data.checkoutUrl`, `data.checkoutToken`

---

### TEST 2 — Open Checkout Page (Browser)

Open the `checkoutUrl` from TEST 1 directly in a browser:

```
https://apipg.banku.co.in/checkout/MTIzNDU6YWJjZGVmZ2g
```

**Expected:**
- Two-panel checkout page loads with branded gradient design
- **Left panel (teal gradient):** merchant logo/initials, order amount (e.g. ₹499.00), customer name/email, secure badge
- **Right panel (white):** accordion list of enabled payment methods — UPI ⚡, Card 💳, Net Banking 🏦, etc.
- Clicking a method expands its form
- Secure payment form is functional

---

### TEST 3 — Pay with Test Card

On the checkout page, select **Card** payment method and use:

```
Card Number:  4111 1111 1111 1111
Name:         Test User
Expiry:       12/28
CVV:          123
```

Click **Pay Now**

**Expected:**
- "Payment successful" message
- Browser redirects to:
  ```
  https://paymentgateway.banku.co.in/payment/callback
    ?payment_id=pay_XXXXXXXX
    &order_id=order_12345
    &signature=abc123...
    &status=success
  ```

✅ **Save:** `payment_id`, `order_id`, `signature` from URL

---

### TEST 4 — Verify Payment (CRITICAL)

**Endpoint:** `POST https://apipg.banku.co.in/api/checkout/verify`

```
Headers:
  X-Api-Key: YOUR_API_KEY
  Content-Type: application/json

Body:
{
  "bankupgPaymentId": "pay_XXXXXXXX",
  "bankupgOrderId": "order_12345",
  "bankupgSignature": "abc123..."
}
```

**Expected:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "status": "success",
    "amount": 499.00
  }
}
```

---

### TEST 5 — Get Order Status

**Endpoint:** `GET https://apipg.banku.co.in/api/checkout/orders/order_12345`

```
Headers:
  X-Api-Key: YOUR_API_KEY
```

**Expected:**
```json
{ "data": { "status": "paid", "paymentId": "pay_XXXXXXXX" } }
```

---

### TEST 6 — Pay with UPI

On the checkout page, select **UPI** and enter:

```
UPI VPA: test@okhdfc
```

Click **Pay Now** → Expected: Success

**Fail test:**
```
UPI VPA: fail@upi
```
Expected: `"Payment declined"` error message on page

---

### TEST 7 — Test Declined Card

```
Card Number: 4000 0000 0000 0002
```

Expected: Error message — "Card declined by issuing bank."  
Order status stays `created` (not `paid`).

---

### TEST 8 — Angular Integration (Full-Page Redirect)

This is the **recommended** integration. Your Angular app creates the order and redirects the customer to the hosted checkout page. After payment, BankUPG redirects back to your `callbackUrl`.

**Template:**
```html
<button (click)="openCheckout()" [disabled]="loading">
  {{ loading ? 'Creating order...' : 'Pay ₹499' }}
</button>
```

**Component:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CheckoutService } from './services/checkout.service';
import { ActivatedRoute } from '@angular/router';

@Component({ selector: 'app-payment', templateUrl: './payment.component.html' })
export class PaymentComponent implements OnInit {
  apiKey = 'YOUR_API_KEY';
  loading = false;

  constructor(
    private checkoutService: CheckoutService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Handle redirect callback from BankUPG after payment
    this.route.queryParams.subscribe(params => {
      if (params['payment_id']) {
        this.handleCallback(params);
      }
    });
  }

  openCheckout(): void {
    this.loading = true;
    this.checkoutService.createOrder(this.apiKey, {
      amount: 499.00,           // rupees (decimal) — NOT paise
      currency: 'INR',
      orderRef: `ORD-${Date.now()}`,
      customerName: 'Test User',
      customerEmail: 'test@banku.co.in',
      customerPhone: '9999999999',
      callbackUrl: 'https://paymentgateway.banku.co.in/payment/callback'
    }).subscribe({
      next: res => {
        if (res.success) {
          // Redirect to hosted checkout page
          this.checkoutService.redirectToCheckout(res.data.checkoutUrl);
        }
      },
      error: () => { this.loading = false; }
    });
  }

  handleCallback(params: any): void {
    // Customer returned via callbackUrl
    // params: { payment_id, order_id, signature, status }
    if (params['status'] === 'success') {
      this.checkoutService.verifyPayment(this.apiKey, {
        bankupgPaymentId: params['payment_id'],
        bankupgOrderId:   params['order_id'],
        bankupgSignature: params['signature']
      }).subscribe(res => {
        if (res.success && res.data.isValid) {
          console.log('✅ Payment verified! ID:', res.data.paymentId);
        } else {
          console.error('❌ Signature verification failed!');
        }
      });
    }
  }
}
```

**Approach B — Embedded iframe with postMessage:**
```typescript
// In ngOnInit or constructor:
window.addEventListener('message', (event) => {
  if (event.data?.source !== 'BankUPG') return;

  if (event.data.event === 'payment.success') {
    // event.data: { payment_id, order_id, signature, amount, payment_mode, paid_at }
    this.checkoutService.verifyPayment(this.apiKey, {
      bankupgPaymentId: event.data.payment_id,
      bankupgOrderId:   event.data.order_id,
      bankupgSignature: event.data.signature
    }).subscribe(res => {
      if (res.data.isValid) console.log('✅ Payment confirmed');
    });
  }

  if (event.data.event === 'payment.dismiss') {
    console.log('Customer pressed Back');
  }
});
```
```

---

### TEST 9 — Checkout Customization

Login as merchant and get JWT token.

**Step 1 — Get current customization:**
```
GET https://apipg.banku.co.in/api/checkout-customizations/my
Authorization: Bearer YOUR_JWT_TOKEN
```

**Step 2 — Update colors:**
```
PUT https://apipg.banku.co.in/api/checkout-customizations/my
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "primaryColor": "#1a73e8",
  "secondaryColor": "#ff5722",
  "language": "Hindi"
}
```

**Step 3 — Upload logo (Postman):**
```
POST https://apipg.banku.co.in/api/checkout-customizations/my/upload-logo
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: multipart/form-data

Key: file   Value: [select PNG/JPG file, max 2MB]
```

**Step 4 — Re-open checkout page** and confirm the new logo and colors appear.

---

### TEST 10 — View Transaction Charges

After a successful payment (TEST 3), view the MDR charge:

```
GET https://apipg.banku.co.in/api/transaction-charges/by-transaction/{transactionId}
Authorization: Bearer YOUR_JWT_TOKEN
```

> **Note:** To find the `transactionId`, query the transaction list from the merchant dashboard or check the Transactions API.

**Expected:**
```json
{
  "success": true,
  "data": [{
    "paymentMethodType": "Card",
    "networkName": "Visa",
    "chargeType": "Percentage",
    "chargeValue": 1.80,
    "transactionAmount": 499.00,
    "chargeAmount": 8.98,
    "gstAmount": 1.62,
    "totalDeduction": 10.60,
    "netAmount": 488.40,
    "createdDate": "2026-07-30T13:45:23Z"
  }]
}
```

> If the array is empty, no MDR rate has been configured for that payment method. Ask SuperAdmin to add one via `POST /api/payment-method-charges`.

---

## 9. Postman Collection Setup

### Environment Variables

Create a Postman Environment named **"BankUPG Production"** with:

| Variable | Value |
|----------|-------|
| `base_url` | `https://apipg.banku.co.in` |
| `api_key` | `your_merchant_api_key` |
| `jwt_token` | *(set after login)* |
| `order_id` | *(set from create order response)* |
| `payment_id` | *(set from callback params)* |
| `signature` | *(set from callback params)* |

### Pre-request Script (Auto-set JWT)

In Collection settings → Pre-request Script:

```javascript
// Auto-refresh token if needed
const token = pm.environment.get("jwt_token");
if (!token) {
  console.warn("jwt_token not set — run the Login request first");
}
```

### Test Script for Create Order (Auto-save orderId)

In the "Create Order" request → Tests tab:

```javascript
const res = pm.response.json();
if (res.success) {
  pm.environment.set("order_id", res.data.orderId);
  pm.environment.set("checkout_url", res.data.checkoutUrl);
  pm.environment.set("checkout_token", res.data.checkoutToken);
  console.log("Order created:", res.data.orderId);
}
```

### Postman Request List

| # | Name | Method | URL |
|---|------|--------|-----|
| 1 | Create Checkout Order | POST | `{{base_url}}/api/checkout/orders` |
| 2 | Get Order Status | GET | `{{base_url}}/api/checkout/orders/{{order_id}}` |
| 3 | Verify Payment | POST | `{{base_url}}/api/checkout/verify` |
| 4 | Get My Customization | GET | `{{base_url}}/api/checkout-customizations/my` |
| 5 | Update My Customization | PUT | `{{base_url}}/api/checkout-customizations/my` |
| 6 | Upload Logo | POST | `{{base_url}}/api/checkout-customizations/my/upload-logo` |
| 7 | Upload Signature | POST | `{{base_url}}/api/checkout-customizations/my/upload-signature` |
| 8 | Get Transaction Charges | GET | `{{base_url}}/api/transaction-charges/by-transaction/{{txn_id}}` |

---

## 10. Test Data Reference

### Test Cards

| Card Number | Network | CVV | Expiry | Brand detected on form | Expected |
|-------------|---------|-----|--------|------------------------|----------|
| `4111 1111 1111 1111` | Visa | Any 3-digit | Any future | Visa | ✅ Success |
| `4012 8888 8888 1881` | Visa | Any | Any future | Visa | ✅ Success |
| `5200 0000 0000 0007` | Mastercard | Any | Any future | MC | ✅ Success |
| `5105 1051 0510 5100` | Mastercard | Any | Any future | MC | ✅ Success |
| `6521 0000 0000 0001` | RuPay | Any | Any future | RuPay | ✅ Success |
| `3782 8224 6310 005` | Amex (15-digit) | Any 4-digit | Any future | Amex | ✅ Success |
| `4000 0000 0000 0002` | Visa | Any | Any future | Visa | ❌ Declined |
| `4000 0000 0000 9995` | Visa | Any | Any future | Visa | ❌ Insufficient Funds |
| `4000 0000 0000 9987` | Visa | Any | Any future | Visa | ❌ Do Not Honour |

### Test UPI VPAs

| VPA | Expected |
|-----|----------|
| `test@okhdfc` | ✅ Success |
| `user@oksbi` | ✅ Success |
| `any@upi` | ✅ Success |
| `fail@upi` | ❌ Payment Failed |

### Test Net Banking

| Bank Code | Expected |
|-----------|----------|
| `HDFC` | ✅ Success |
| `ICIC` | ✅ Success |
| `SBIN` | ✅ Success |
| `fail` | ❌ Payment Failed |

### Amount Format

> **Amount is in rupees (decimal), NOT paise.**

| Send this | Displays as |
|-----------|-------------|
| `1` | ₹1.00 |
| `10` | ₹10.00 |
| `100` | ₹100.00 |
| `499` or `499.00` | ₹499.00 |
| `1000` | ₹1,000.00 |
| `10000000` | ₹1,00,00,000 (max) |

---

## 11. Common Errors & Fixes

### Error: `401 Unauthorized — X-Api-Key header is required.`
**Cause:** Missing or wrong API key header.  
**Fix:** Add header `X-Api-Key: YOUR_KEY` to the request.

---

### Error: `401 Unauthorized — Invalid API key.`
**Cause:** API key doesn't exist or is revoked.  
**Fix:** Check API key in the BankUPG dashboard → Settings → API Keys.

---

### Error: `Merchant account is not active.`
**Cause:** Merchant onboarding is incomplete or rejected.  
**Fix:** Complete merchant onboarding or contact BankUPG support.

---

### Error: `Order already paid.`
**Cause:** Trying to pay an already-completed order.  
**Fix:** Create a new order for each payment attempt.

---

### Error: `This payment session has expired.`
**Cause:** Order was created more than 30 minutes ago.  
**Fix:** Call `POST /api/checkout/orders` again to create a fresh order.

---

### Error: `Signature verification failed.`
**Cause:** `signature` from callback URL (or postMessage) does not match recomputed HMAC.  
**Possible reasons:**
1. Wrong `api_salt` used
2. `payment_id` or `order_id` was modified (tampering attempt)
3. Params were URL-decoded incorrectly

**Fix:**
```typescript
// Correct signature computation (TypeScript / Node.js)
import * as crypto from 'crypto';

const message  = `${paymentId}|${orderId}`;
const expected = crypto
  .createHmac('sha256', apiSalt)
  .update(message)
  .digest('hex');

const isValid = expected === receivedSignature.toLowerCase();
```

---

### Error: `CORS` policy error in Angular
**Cause:** API does not allow requests from `paymentgateway.banku.co.in`.  
**Fix:** Ensure `https://paymentgateway.banku.co.in` is in the CORS allowed origins in `apipg.banku.co.in` server config.

---

### Error: Checkout page does not load (blank page)
**Cause:** Invalid or expired checkout token.  
**Fix:** Verify the `checkoutUrl` uses a token from a newly created (`status: "created"`) order.

---

### Error: Checkout page loads but amount / merchant name are blank
**Cause:** JavaScript config failed to parse (network issue or malformed session data).  
**Fix:** The page will show an error message instead of a blank state. Ensure the token is valid and the order has not expired. Open browser DevTools → Console to see any JS errors.

---

### Error: File upload returns 400 — `"File type not allowed"`
**Cause:** Unsupported file extension.  
**Fix:** Only upload `.jpg`, `.jpeg`, `.png`, `.svg`, `.webp`. Max size: **2 MB**.

---

### Error: `data: null` on GET customization
**Cause:** Merchant has not configured any customization yet.  
**Fix:** This is normal for first-time setup. Call `PUT /api/checkout-customizations/my` to create it.

---

## 12. Testing Checklist

Use this checklist to verify all features are working after deployment.

### Checkout Flow
- [ ] `POST /api/checkout/orders` returns `checkoutUrl`
- [ ] `checkoutUrl` opens the hosted checkout page with merchant branding
- [ ] Payment succeeds with Visa test card `4111 1111 1111 1111`
- [ ] Payment fails with card `4000 0000 0000 0002` (correct error message shown)
- [ ] UPI payment succeeds with any valid VPA
- [ ] UPI payment fails with `fail@upi`
- [ ] Net Banking payment succeeds
- [ ] Callback URL receives correct `payment_id`, `order_id`, `signature` query params
- [ ] `POST /api/checkout/verify` returns `isValid: true` for legitimate payment
- [ ] `POST /api/checkout/verify` returns `isValid: false` when signature is tampered
- [ ] `GET /api/checkout/orders/{orderId}` shows `status: "paid"` after successful payment
- [ ] Expired order (>30 min) returns `status: "expired"` error on payment attempt

### Iframe / postMessage Integration
- [ ] Checkout page embedded in `<iframe>` renders correctly
- [ ] `window.postMessage` with `event: 'payment.success'` fires on success
- [ ] `postMessage` contains `payment_id`, `order_id`, `signature` fields
- [ ] `postMessage` with `event: 'payment.dismiss'` fires when customer presses Back
- [ ] `verifyPayment()` called with `payment_id` / `order_id` / `signature` validates correctly

### Checkout Customization
- [ ] `GET /api/checkout-customizations/my` returns `null` for new merchant (no error)
- [ ] `PUT /api/checkout-customizations/my` creates customization on first call
- [ ] `PUT /api/checkout-customizations/my` updates only specified fields (partial update works)
- [ ] Logo upload succeeds (PNG under 2 MB)
- [ ] Logo upload fails for oversized file (>2 MB)
- [ ] Logo upload fails for unsupported type (e.g. `.pdf`)
- [ ] After logo upload, new checkout URL shows the uploaded logo
- [ ] Color changes applied on checkout page after customization update

### Transaction Charges
- [ ] `GET /api/transaction-charges/by-transaction/{id}` returns charge breakdown
- [ ] `chargeAmount + gstAmount = totalDeduction`
- [ ] `transactionAmount - totalDeduction = netAmount`
- [ ] Different card networks (Visa vs RuPay) return different charge rates (if configured)

---

## Quick Reference Card

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  BankUPG API Quick Reference
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  API Base URL:    https://apipg.banku.co.in
  Frontend URL:    https://paymentgateway.banku.co.in
  Checkout Page:   https://apipg.banku.co.in/checkout/{token}
  JS SDK:          https://apipg.banku.co.in/checkout.js
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  CREATE ORDER     POST  /api/checkout/orders
  HOSTED PAGE      GET   /checkout/{token}
  VERIFY PAYMENT   POST  /api/checkout/verify
  ORDER STATUS     GET   /api/checkout/orders/{orderId}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  GET BRANDING     GET   /api/checkout-customizations/my
  SAVE BRANDING    PUT   /api/checkout-customizations/my
  UPLOAD LOGO      POST  /api/checkout-customizations/my/upload-logo
  UPLOAD SIG       POST  /api/checkout-customizations/my/upload-signature
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  MDR CHARGES      GET   /api/transaction-charges/by-transaction/{id}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  SUCCESS CARD     4111 1111 1111 1111  (Visa)
  FAIL CARD        4000 0000 0000 0002  (Declined)
  SUCCESS UPI      any@upi
  FAIL UPI         fail@upi
  AMOUNT FORMAT    Rupees (decimal) e.g. 499 = ₹499.00
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  CALLBACK PARAMS  payment_id, order_id, signature, status
  SIGNATURE        HMAC-SHA256(payment_id + "|" + order_id, api_salt)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

*© 2026 BankUPG Payment Services — Internal Angular Integration Reference*  
*For support: support@bankupg.com | Dashboard: https://paymentgateway.banku.co.in*
