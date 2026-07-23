# BankUPG API Integration Guide

> For Angular developers — covers Payment Gateway APIs built/extended in this sprint.

## Base URL & Auth
- **Base URL:** `{host}/api`
- **Auth:** `Bearer <JWT>` in `Authorization` header
- **User context:** Endpoints below use the JWT `sub`/`NameIdentifier` claim to resolve the merchant. These are **Merchant Portal** APIs unless marked otherwise.

---

## Auth

### POST `/api/auth/login`
- **Panel:** Merchant / Public
- **Request:**
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- **Response:** `LoginResponse` — token, user details, merchant onboarding flags.

### POST `/api/auth/verify-otp`
- **Panel:** Merchant / Public
- **Request:** OTP token
- **Response:** `LoginResponse`

---

## Transactions

### GET `/api/transactions`
- **Panel:** Merchant
- **Query params:**
  | Field | Type | Description |
  |-------|------|-------------|
  | `TransactionId` | `long?` | exact transaction id |
  | `CustomerEmail` | `string?` | partial email |
  | `MerchantReferenceId` | `string?` | partial reference |
  | `Phone` | `string?` | partial phone |
  | `CustomerName` | `string?` | partial name |
  | `UpiTransactionId` | `string?` | UPI reference |
  | `BankReferenceNumber` | `string?` | bank RRN |
  | `DateFilter` | `string?` | `today`, `yesterday`, `last7days`, `last30days`, `custom` |
  | `FromDate` | `DateTime?` | with `custom` |
  | `ToDate` | `DateTime?` | with `custom` |
  | `Source` | `string?` | source channel |
  | `Status` | `string?` | e.g. `Success`, `Failed`, `Pending` |
  | `RefundType` | `string?` | refund type filter |
  | `PaymentMode` | `string?` | e.g. `UPI`, `Card`, `NetBanking` |
  | `SortBy` | `string?` | `transactionId`, `amount`, `createdDate` |
  | `SortDirection` | `string?` | `asc` / `desc` (default `desc`) |
  | `PageNumber` | `int` | default `1` |
  | `PageSize` | `int` | default `20` |
- **Response:** `ApiResponse<PagedResponse<TransactionResponse>>`
  ```json
  {
    "success": true,
    "message": "Transactions retrieved",
    "data": {
      "items": [ { "paymentLinkId": 0, "paymentLink": "...", "amount": 0, ... } ],
      "totalCount": 0,
      "pageNumber": 1,
      "pageSize": 20
    }
  }
  ```

### GET `/api/transactions/{transactionId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<TransactionResponse>`

### GET `/api/transactions/summary`
- **Panel:** Merchant
- **Query params:** same filter set as `GET /api/transactions` (optional)
- **Response:** `ApiResponse<TransactionSummaryResponse>`
  ```json
  {
    "success": true,
    "data": {
      "totalPayments": 0,
      "numberOfTransactions": 0,
      "successCount": 0,
      "successRate": 0,
      "failedCount": 0,
      "pendingCount": 0,
      "refundedAmount": 0
    }
  }
  ```

### GET `/api/transactions/{transactionId:long}/charges`
- **Panel:** Merchant
- **Response:** `ApiResponse<TransactionChargesResponse>`

### GET `/api/transactions/mdr-rates`
- **Panel:** Merchant
- **Response:** `ApiResponse<List<MdrRateResponse>>`

---

## Settlements

### GET `/api/settlements`
- **Panel:** Merchant
- **Query params:**
  | Field | Type | Description |
  |-------|------|-------------|
  | `UtrNumber` | `string?` | partial UTR |
  | `TransactionId` | `long?` | related transaction id |
  | `DateFilter` | `string?` | `today`, `yesterday`, `last7days`, `last30days`, `custom` |
  | `FromDate` | `DateTime?` | with `custom` |
  | `ToDate` | `DateTime?` | with `custom` |
  | `Status` | `string?` | e.g. `Settled`, `Pending` |
  | `SettlementCycle` | `string?` | cycle code |
  | `SettlementT` | `int?` | T+ value |
  | `SortBy` | `string?` | `utr`, `salesAmount`, `settledAmount`, `settlementDate`, `createdDate` |
  | `SortDirection` | `string?` | `asc` / `desc` |
  | `PageNumber` | `int` | default `1` |
  | `PageSize` | `int` | default `20` |
- **Response:** `ApiResponse<PagedResponse<SettlementResponse>>`

### GET `/api/settlements/{settlementId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<SettlementResponse>`

### GET `/api/settlements/summary`
- **Panel:** Merchant
- **Response:** `ApiResponse<SettlementSummaryResponse>`
  ```json
  {
    "success": true,
    "data": {
      "totalSalesAmount": 0,
      "totalFees": 0,
      "totalSettledAmount": 0,
      "pendingAmount": 0,
      "lastSettledAmount": 0,
      "totalSettlementPending": 0,
      "upcomingSettlementAmount": 0,
      "totalSettlements": 0,
      "currentSettlementT": 0
    }
  }
  ```

### GET `/api/settlements/config`
- **Panel:** Merchant
- **Response:** `ApiResponse<SettlementConfigResponse>`

### PUT `/api/settlements/config`
- **Panel:** Merchant
- **Request:** `UpdateSettlementConfigRequest`
  ```json
  {
    "settlementT": 1,
    "settlementCycleType": "T+1"
  }
  ```
- **Response:** `ApiResponse<SettlementConfigResponse>`

---

## Chargebacks

### GET `/api/chargebacks`
- **Panel:** Merchant
- **Query params:**
  | Field | Type | Description |
  |-------|------|-------------|
  | `TransactionId` | `long?` | exact transaction |
  | `BankCaseNumber` | `string?` | partial bank case |
  | `CaseNumber` | `string?` | partial case number |
  | `Status` | `string?` | `New`, `PendingResponse`, `PendingReview`, `Closed` |
  | `DateFilter` | `string?` | presets or `custom` |
  | `FromDate` | `DateTime?` | with `custom` |
  | `ToDate` | `DateTime?` | with `custom` |
  | `ChargebackType` | `string?` | `CB`, `GoodFaith`, `PreArb` |
  | `CloseReason` | `string?` | `Accepted`, `Rejected`, `Resolved` |
  | `SortBy` | `string?` | `transactionId`, `caseNumber`, `chargebackDate`, `replyBefore`, `status` |
  | `SortDirection` | `string?` | `asc` / `desc` |
  | `PageNumber` | `int` | default `1` |
  | `PageSize` | `int` | default `20` |
- **Response:** `ApiResponse<PagedResponse<ChargebackResponse>>`
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "chargebackId": 0,
          "transactionId": 0,
          "chargebackDate": "...",
          "replyBefore": "...",
          "status": "New",
          "caseNumber": "...",
          "chargebackReason": "...",
          "documents": "...",
          "isOverdue": false
        }
      ],
      "totalCount": 0,
      "pageNumber": 1,
      "pageSize": 20
    }
  }
  ```

### GET `/api/chargebacks/{chargebackId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<ChargebackResponse>`

### PUT `/api/chargebacks/{chargebackId:long}`
- **Panel:** Merchant
- **Request:** `UpdateChargebackRequest`
  ```json
  {
    "status": "Closed",
    "closeReason": "Accepted",
    "documentPath": "https://..."
  }
  ```
- **Response:** `ApiResponse<ChargebackResponse>`

---

## Payment Links

### POST `/api/payment-links`
- **Panel:** Merchant
- **Request:** `CreatePaymentLinkRequest`
  ```json
  {
    "description": "Invoice payment",
    "amount": 2500.00,
    "amountType": "fixed",
    "expiryDate": "2026-08-31T23:59:00",
    "dueDate": "2026-07-30T23:59:00",
    "validationPeriod": 30,
    "timeUnit": "D",
    "paymentType": "Standard",
    "isPartialPayment": false,
    "maxPaymentsAllowed": 1,
    "customerName": "Rahul Sharma",
    "customerEmail": "rahul@example.com",
    "customerPhone": "9876543210",
    "referenceId": "REF-0001",
    "invoiceId": "INV-0001",
    "maxUses": 1,
    "sendSms": true,
    "customerDataCapture": [
      { "fieldType": "Text", "name": "GSTIN" },
      { "fieldType": "Dropdown", "name": "Plan", "options": "Monthly,Yearly" }
    ]
  }
  ```
- **Response:** `ApiResponse<PaymentLinkResponse>`

### GET `/api/payment-links`
- **Panel:** Merchant
- **Query params:**
  | Field | Type | Description |
  |-------|------|-------------|
  | `DateFilter` | `string?` | `today`, `yesterday`, `last7days`, `last30days`, `custom` |
  | `FromDate` | `DateTime?` | with `custom` |
  | `ToDate` | `DateTime?` | with `custom` |
  | `Status` | `string?` | `active`, `deactivated`, `expired`, `paid`, `cancelled` |
  | `PaymentType` | `string?` | `Standard`, `PartialPayment` |
  | `Purpose` | `string?` | partial purpose/description |
  | `InvoiceId` | `string?` | partial invoice id |
  | `CustomerName` | `string?` | partial name |
  | `CustomerEmail` | `string?` | partial email |
  | `CustomerPhone` | `string?` | partial phone |
  | `ReferenceId` | `string?` | partial reference |
  | `SortBy` | `string?` | `amount`, `status`, `paymentType`, `purpose`, `createdDate` |
  | `SortDirection` | `string?` | `asc` / `desc` |
  | `PageNumber` | `int` | default `1` |
  | `PageSize` | `int` | default `20` |
- **Response:** `ApiResponse<PagedResponse<PaymentLinkResponse>>`
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "createdOn": "...",
          "paymentLinkId": 0,
          "purposeOfPayment": "Invoice payment",
          "invoiceID": "INV-0001",
          "amount": 2500.00,
          "paymentLink": "pay.banku.in/ABC123",
          "paymentType": "Standard",
          "status": "active",
          "customerEmail": "...",
          "customerMobile": "...",
          "totalViews": 0,
          "totalAmountPaid": 0
        }
      ],
      "totalCount": 0,
      "pageNumber": 1,
      "pageSize": 20
    }
  }
  ```

### GET `/api/payment-links/{linkId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<PaymentLinkResponse>`

### GET `/api/payment-links/summary`
- **Panel:** Merchant
- **Response:** `ApiResponse<PaymentLinkSummaryResponse>`
  ```json
  {
    "success": true,
    "data": {
      "activePaymentLinks": 0,
      "revenueViaPaymentLinks": 0,
      "totalViews": 0,
      "totalLinks": 0,
      "paidLinks": 0
    }
  }
  ```

### POST `/api/payment-links/{linkId:long}/cancel`
- **Panel:** Merchant
- **Response:** `ApiResponse`

---

## Payment Link Bulk Uploads

### POST `/api/payment-link-bulk-uploads`
- **Panel:** Merchant
- **Request:** `CreatePaymentLinkBulkUploadRequest`
  ```json
  {
    "batchReferenceId": "BULK-12345678",
    "batchDescription": "July campaign",
    "fileName": "PaymentLinkBulkUploadSample.csv",
    "sendEmail": false,
    "sendSms": true,
    "customerDataCapture": [
      { "fieldType": "Text", "name": "UTR" }
    ]
  }
  ```
- **Response:** `ApiResponse<PaymentLinkBulkUploadResponse>`

### GET `/api/payment-link-bulk-uploads`
- **Panel:** Merchant
- **Query params:**
  | Field | Type | Description |
  |-------|------|-------------|
  | `DateFilter` | `string?` | presets or `custom` |
  | `FromDate` | `DateTime?` | with `custom` |
  | `ToDate` | `DateTime?` | with `custom` |
  | `Status` | `string?` | `All`, `Pending`, `Completed`, `Failed` |
  | `PageNumber` | `int` | default `1` |
  | `PageSize` | `int` | default `20` |
- **Response:** `ApiResponse<PagedResponse<PaymentLinkBulkUploadResponse>>`

### GET `/api/payment-link-bulk-uploads/{bulkUploadId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<PaymentLinkBulkUploadResponse>`

### POST `/api/payment-link-bulk-uploads/files`
- **Panel:** Merchant
- **Request:** `CreatePaymentLinkBulkUploadFileRequest`
  ```json
  {
    "fileName": "bulk.csv",
    "filePath": "/uploads/bulk.csv"
  }
  ```
- **Response:** `ApiResponse<PaymentLinkBulkUploadFileResponse>`

### GET `/api/payment-link-bulk-uploads/files`
- **Panel:** Merchant
- **Query params:** `pageNumber`, `pageSize`
- **Response:** `ApiResponse<PagedResponse<PaymentLinkBulkUploadFileResponse>>`

---

## Sample Files
- `c:\BankUPG\SampleFiles\ChargebackBulkUploadSample.csv`
- `c:\BankUPG\SampleFiles\PaymentLinkBulkUploadSample.csv`

---

## Transaction Charges (SuperAdmin)

> Charge breakup for individual transactions. Managed by SuperAdmin.

### POST `/api/transaction-charges`
- **Panel:** SuperAdmin
- **Request:** `CreateTransactionChargeRequest`
  ```json
  {
    "transactionId": 1,
    "mid": 1,
    "paymentMethodChargeId": null,
    "paymentMethodType": "UPI",
    "networkName": "Visa",
    "chargeType": "Percentage",
    "chargeValue": 2.0,
    "transactionAmount": 1000.0,
    "chargeAmount": 20.0,
    "gstAmount": 3.6,
    "totalDeduction": 23.6,
    "netAmount": 976.4
  }
  ```
- **Response:** `ApiResponse<TransactionChargeResponse>`

### PUT `/api/transaction-charges/{transactionChargeId:long}`
- **Panel:** SuperAdmin
- **Request:** `UpdateTransactionChargeRequest`
- **Response:** `ApiResponse<TransactionChargeResponse>`

### GET `/api/transaction-charges/{transactionChargeId:long}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse<TransactionChargeResponse>`

### POST `/api/transaction-charges/recalculate/{transactionId:long}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse<TransactionChargeResponse>`

### DELETE `/api/transaction-charges/{transactionChargeId:long}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse`

---

## Payment Method Charges (SuperAdmin)

> Master charge rules by payment method/network. Managed by SuperAdmin.

### POST `/api/payment-method-charges`
- **Panel:** SuperAdmin
- **Request:** `CreatePaymentMethodChargeRequest`
  ```json
  {
    "paymentMethodType": "CreditCard",
    "networkName": "Visa",
    "chargeType": "Percentage",
    "chargeValue": 2.0,
    "minCharge": 10.0,
    "maxCharge": 1000.0,
    "gstPercentage": 18.0,
    "isActive": true
  }
  ```
- **Response:** `ApiResponse<PaymentMethodChargeResponse>`

### PUT `/api/payment-method-charges/{paymentMethodChargeId:int}`
- **Panel:** SuperAdmin
- **Request:** `CreatePaymentMethodChargeRequest`
- **Response:** `ApiResponse<PaymentMethodChargeResponse>`

### GET `/api/payment-method-charges/{paymentMethodChargeId:int}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse<PaymentMethodChargeResponse>`

### GET `/api/payment-method-charges`
- **Panel:** SuperAdmin
- **Query params:** `pageNumber`, `pageSize`
- **Response:** `ApiResponse<PagedResponse<PaymentMethodChargeResponse>>`

### DELETE `/api/payment-method-charges/{paymentMethodChargeId:int}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse`

---

## Merchant API Keys

### POST `/api/merchant-api-keys`
- **Panel:** SuperAdmin
- **Request:** `CreateMerchantApiKeyRequest`
  ```json
  {
    "mid": 1,
    "apiKey": "...",
    "apiSalt": "...",
    "clientId": "...",
    "clientSecretHash": "..."
  }
  ```
- **Response:** `ApiResponse<MerchantApiKeyResponse>`

### PUT `/api/merchant-api-keys/{merchantApiKeyId:int}`
- **Panel:** SuperAdmin
- **Request:** `UpdateMerchantApiKeyRequest`
- **Response:** `ApiResponse<MerchantApiKeyResponse>`

### GET `/api/merchant-api-keys/{merchantApiKeyId:int}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse<MerchantApiKeyResponse>`

### GET `/api/merchant-api-keys/by-mid/{mid:int}`
- **Panel:** SuperAdmin / Merchant (own MID)
- **Response:** `ApiResponse<MerchantApiKeyResponse>`

### DELETE `/api/merchant-api-keys/{merchantApiKeyId:int}`
- **Panel:** SuperAdmin
- **Response:** `ApiResponse`

---

## Subscriptions

### POST `/api/subscriptions`
- **Panel:** Merchant
- **Request:** `CreateSubscriptionRequest`
- **Response:** `ApiResponse<SubscriptionResponse>`

### GET `/api/subscriptions/{subscriptionId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<SubscriptionResponse>`

### GET `/api/subscriptions`
- **Panel:** Merchant
- **Query params:** `ListSubscriptionsRequest`
- **Response:** `ApiResponse<PagedResponse<SubscriptionResponse>>`

### POST `/api/subscriptions/{subscriptionId:long}/cancel`
- **Panel:** Merchant
- **Response:** `ApiResponse`

---

## EMI Plans

### POST `/api/emi-plans`
- **Panel:** Merchant
- **Request:** `CreateEmiPlanRequest`
- **Response:** `ApiResponse<EmiPlanResponse>`

### GET `/api/emi-plans`
- **Panel:** Merchant
- **Response:** `ApiResponse<List<EmiPlanResponse>>`

### PUT `/api/emi-plans/{emiPlanId:int}`
- **Panel:** Merchant
- **Request:** `UpdateEmiPlanRequest`
- **Response:** `ApiResponse<EmiPlanResponse>`

### DELETE `/api/emi-plans/{emiPlanId:int}`
- **Panel:** Merchant
- **Response:** `ApiResponse` (deactivate)

---

## Invoices

### POST `/api/invoices`
- **Panel:** Merchant
- **Request:** `CreateInvoiceRequest`
- **Response:** `ApiResponse<InvoiceResponse>`

### GET `/api/invoices/{invoiceId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<InvoiceResponse>`

### GET `/api/invoices`
- **Panel:** Merchant
- **Query params:** `ListInvoicesRequest`
- **Response:** `ApiResponse<PagedResponse<InvoiceResponse>>`

### POST `/api/invoices/{invoiceId:long}/send`
- **Panel:** Merchant
- **Response:** `ApiResponse`

### POST `/api/invoices/{invoiceId:long}/cancel`
- **Panel:** Merchant
- **Response:** `ApiResponse`

---

## Batch Refunds

### POST `/api/batch-refunds`
- **Panel:** Merchant
- **Request:** `CreateBatchRefundRequest`
- **Response:** `ApiResponse<BatchRefundResponse>`

### GET `/api/batch-refunds/{batchRefundId:long}`
- **Panel:** Merchant
- **Response:** `ApiResponse<BatchRefundResponse>`

### GET `/api/batch-refunds`
- **Panel:** Merchant
- **Query params:** `ListBatchRefundsRequest`
- **Response:** `ApiResponse<PagedResponse<BatchRefundResponse>>`

---

## SuperAdmin Master APIs

All `*MasterController` endpoints now require `SuperAdmin` role. Supported operations vary per controller; Angular side should guard these routes behind the SuperAdmin menu.

| Controller | Base Route | Panel |
|------------|------------|-------|
| BusinessCategoryMaster | `/api/businessCategoryMaster` | SuperAdmin |
| BusinessSubCategoryMaster | `/api/businessSubCategoryMaster` | SuperAdmin |
| BusinessEntityTypeMaster | `/api/businessEntityTypeMaster` | SuperAdmin |
| BusinessProofTypeMaster | `/api/businessProofTypeMaster` | SuperAdmin |
| DocumentMaster | `/api/documentMaster` | SuperAdmin |
| DocumentTypeMaster | `/api/documentTypeMaster` | SuperAdmin |
| MerchantMaster | `/api/merchantMaster` | SuperAdmin |
| UserMaster | `/api/userMaster` | SuperAdmin |
| PepstatusMaster | `/api/pepstatusMaster` | SuperAdmin |

Each supports the standard patterns exposed by the underlying master service (POST create, GET by id, GET list, PUT update, etc.).

---

## Additional SuperAdmin APIs

### Checkout Customizations — `/api/checkout-customizations`
- **Panel:** SuperAdmin
- `POST` `CreateCheckoutCustomizationRequest` → `CheckoutCustomizationResponse`
- `PUT /{checkoutCustomizationId:int}` `UpdateCheckoutCustomizationRequest` → `CheckoutCustomizationResponse`
- `GET /{checkoutCustomizationId:int}` → `CheckoutCustomizationResponse`
- `GET /by-mid/{mid:int}` → `CheckoutCustomizationResponse`
- `GET` (paged list) → `PagedResponse<CheckoutCustomizationResponse>`
- `DELETE /{checkoutCustomizationId:int}`

### Webhooks — `/api/webhooks`
- **Panel:** SuperAdmin
- `POST` `CreateWebhookRequest` → `WebhookResponse`
- `PUT /{webhookId:int}` `UpdateWebhookRequest` → `WebhookResponse`
- `GET /{webhookId:int}` → `WebhookResponse`
- `GET /by-mid/{mid:int}` (paged) → `PagedResponse<WebhookResponse>`
- `GET` (paged list) → `PagedResponse<WebhookResponse>`
- `DELETE /{webhookId:int}`

### Merchant Payment Methods — `/api/merchant-payment-methods`
- **Panel:** SuperAdmin
- `POST` `CreateMerchantPaymentMethodRequest` → `MerchantPaymentMethodResponse`
- `PUT /{merchantPaymentMethodId:int}` `UpdateMerchantPaymentMethodRequest` → `MerchantPaymentMethodResponse`
- `GET /{merchantPaymentMethodId:int}` → `MerchantPaymentMethodResponse`
- `GET /by-mid/{mid:int}` (paged) → `PagedResponse<MerchantPaymentMethodResponse>`
- `DELETE /{merchantPaymentMethodId:int}`

### Merchant Column Preferences — `/api/merchant-column-preferences`
- **Panel:** SuperAdmin
- `POST` `CreateMerchantColumnPreferenceRequest` → `MerchantColumnPreferenceResponse`
- `PUT /{merchantColumnPreferenceId:int}` `UpdateMerchantColumnPreferenceRequest` → `MerchantColumnPreferenceResponse`
- `GET /{merchantColumnPreferenceId:int}` → `MerchantColumnPreferenceResponse`
- `GET /by-mid/{mid:int}/{gridName}` → `MerchantColumnPreferenceResponse`
- `DELETE /{merchantColumnPreferenceId:int}`

---

## Notes for Angular Developer
- All `GET` list endpoints return `ApiResponse<PagedResponse<T>>`.
- Dates are ISO-8601 with time (`2026-07-23T18:30:00`).
- Use `DateFilter` presets (`today`, `yesterday`, `last7days`, `last30days`) for common ranges; send `custom` with `FromDate`/`ToDate` for custom ranges.
- The `SortDirection` default is `desc`.
- SuperAdmin-specific endpoints live under `/api/admin/**` and require a SuperAdmin role/claim.
- **SuperAdmin** endpoints in this guide: `Transaction Charges`, `Payment Method Charges`, `Merchant API Keys`, `*MasterController` (BusinessCategory, BusinessSubCategory, BusinessEntityType, BusinessProofType, DocumentMaster, DocumentTypeMaster, MerchantMaster, UserMaster, PepstatusMaster), `Checkout Customizations`, `Webhooks`, `Merchant Payment Methods`, `Merchant Column Preferences`. `Merchant API Keys` `GET by-mid` also allows a merchant to fetch its own keys.
