using BankUPG.Application.Interfaces.Checkout;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BankUPG.Application.Services.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CheckoutService> _logger;

        private static readonly HashSet<string> FailCardNumbers = new()
        {
            "4000000000000002",
            "4000000000009995",
            "4000000000009987"
        };

        public CheckoutService(AppDBContext context, ILogger<CheckoutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CheckoutOrderResponse> InitiateOrderAsync(string apiKey, CheckoutInitiateRequest request, string baseUrl)
        {
            var apiKeyRecord = await _context.MerchantApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.ApiKey == apiKey)
                ?? throw new UnauthorizedAccessException("Invalid API key.");

            var merchant = await _context.Merchants.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Mid == apiKeyRecord.Mid)
                ?? throw new InvalidOperationException("Merchant not found.");

            if (merchant.IsActive != true)
                throw new InvalidOperationException("Merchant account is not active.");

            var order = new Infrastructure.Entities.PaymentOrder
            {
                Mid = apiKeyRecord.Mid,
                OrderRef = request.OrderRef,
                Amount = request.Amount,
                Currency = request.Currency,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Notes = request.Notes,
                Status = "created",
                ExpiryDate = DateTime.UtcNow.AddMinutes(30),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentOrders.Add(order);
            await _context.SaveChangesAsync();

            var token = GenerateCheckoutToken(order.PaymentOrderId, apiKeyRecord.ApiSalt ?? apiKey);

            _logger.LogInformation("Checkout order {OrderId} created for MID {Mid}", order.PaymentOrderId, apiKeyRecord.Mid);

            return new CheckoutOrderResponse
            {
                OrderId = $"order_{order.PaymentOrderId}",
                CheckoutToken = token,
                CheckoutUrl = $"{baseUrl}/checkout/{token}",
                Amount = order.Amount,
                Currency = order.Currency ?? "INR",
                OrderRef = order.OrderRef!,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                Status = "created",
                ExpiryDate = order.ExpiryDate!.Value,
                CreatedDate = order.CreatedDate!.Value
            };
        }

        public async Task<CheckoutSessionResponse?> GetSessionAsync(string checkoutToken)
        {
            var orderId = ExtractOrderIdFromToken(checkoutToken);
            if (orderId == 0) return null;

            var order = await _context.PaymentOrders.AsNoTracking()
                .Include(o => o.MidNavigation)
                    .ThenInclude(m => m.CheckoutCustomization)
                .Include(o => o.MidNavigation)
                    .ThenInclude(m => m.MerchantPaymentMethods)
                .FirstOrDefaultAsync(o => o.PaymentOrderId == orderId);

            if (order == null) return null;

            var customization = order.MidNavigation?.CheckoutCustomization;
            var paymentMethods = order.MidNavigation?.MerchantPaymentMethods
                .Where(m => m.IsActive == true)
                .Select(m => m.PaymentMethodType ?? "Card")
                .Distinct()
                .ToList() ?? new List<string> { "Card", "UPI", "NetBanking" };

            if (!paymentMethods.Any())
                paymentMethods = new List<string> { "UPI", "Card", "NetBanking", "Wallet", "EMI", "PayLater" };

            return new CheckoutSessionResponse
            {
                OrderId = $"order_{order.PaymentOrderId}",
                Amount = order.Amount,
                Currency = order.Currency ?? "INR",
                OrderRef = order.OrderRef!,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                Status = order.Status ?? "created",
                ExpiryDate = order.ExpiryDate ?? DateTime.UtcNow.AddMinutes(30),
                MerchantName = order.MidNavigation?.BusinessName,
                MerchantLogoUrl = customization?.BrandLogoUrl,
                PrimaryColor = customization?.PrimaryColor ?? "#009688",
                SecondaryColor = customization?.SecondaryColor ?? "#7c3aed",
                Language = customization?.Language ?? "English",
                IsExpired = order.ExpiryDate.HasValue && order.ExpiryDate.Value < DateTime.UtcNow,
                IsPaid = order.Status == "paid",
                EnabledPaymentModes = paymentMethods
            };
        }

        public async Task<CheckoutPayResponse> ProcessPaymentAsync(CheckoutPayCardRequest request, string baseUrl)
        {
            var orderId = ExtractOrderIdFromToken(request.CheckoutToken);
            if (orderId == 0)
                return new CheckoutPayResponse { Success = false, Message = "Invalid session token." };

            var order = await _context.PaymentOrders
                .Include(o => o.MidNavigation)
                    .ThenInclude(m => m.MerchantApiKey)
                .FirstOrDefaultAsync(o => o.PaymentOrderId == orderId);

            if (order == null)
                return new CheckoutPayResponse { Success = false, Message = "Order not found." };

            if (order.Status == "paid")
                return new CheckoutPayResponse { Success = false, Message = "Order already paid.", Status = "paid" };

            if (order.ExpiryDate.HasValue && order.ExpiryDate.Value < DateTime.UtcNow)
            {
                order.Status = "expired";
                await _context.SaveChangesAsync();
                return new CheckoutPayResponse { Success = false, Message = "This payment session has expired.", Status = "expired" };
            }

            var isSuccess = SimulatePayment(request);
            var paymentId = $"pay_{GenerateUniqueId()}";
            var now = DateTime.UtcNow;

            var attempt = new PaymentAttempt
            {
                OrderId = order.PaymentOrderId,
                Mid = order.Mid,
                PaymentMode = request.PaymentMode,
                Amount = order.Amount,
                Status = isSuccess ? "success" : "failed",
                FailureReason = isSuccess ? null : "Card declined by issuing bank.",
                AttemptDate = now,
                CreatedDate = now
            };

            _context.PaymentAttempts.Add(attempt);

            if (isSuccess)
            {
                var transaction = new Infrastructure.Entities.Transaction
                {
                    Mid = order.Mid,
                    PayuId = paymentId,
                    MerchantReferenceId = order.OrderRef,
                    CustomerEmail = order.CustomerEmail,
                    CustomerPhone = order.CustomerPhone,
                    CustomerName = order.CustomerName,
                    PaymentMode = request.PaymentMode,
                    Source = "WebCheckout",
                    Amount = order.Amount,
                    Status = "success",
                    UpiReference = request.PaymentMode == "UPI" ? $"UPI{GenerateUniqueId()}" : null,
                    BankReference = $"BANKUPG{GenerateUniqueId()}",
                    TransactionDate = now,
                    OrderId = order.PaymentOrderId,
                    CreatedDate = now,
                    UpdatedDate = now
                };

                _context.Transactions.Add(transaction);
                order.Status = "paid";
                order.PaidDate = now;
                order.UpdatedDate = now;

                await _context.SaveChangesAsync();
                attempt.TransactionId = transaction.TransactionId;
                await _context.SaveChangesAsync();

                await ComputeTransactionChargeAsync(transaction, request.PaymentMode, request.CardNumber);

                var salt = order.MidNavigation?.MerchantApiKey?.ApiSalt ?? "default";
                var signature = GenerateSignature(paymentId, $"order_{order.PaymentOrderId}", salt);

                var callbackUrl = BuildCallbackUrl(order, paymentId, signature, true);

                _ = FireWebhookAsync(order, paymentId);

                return new CheckoutPayResponse
                {
                    Success = true,
                    PaymentId = paymentId,
                    OrderId = $"order_{order.PaymentOrderId}",
                    Status = "success",
                    Message = "Payment successful.",
                    RedirectUrl = callbackUrl,
                    Signature = signature,
                    Amount = order.Amount,
                    PaymentMode = request.PaymentMode,
                    PaidAt = now
                };
            }
            else
            {
                await _context.SaveChangesAsync();
                return new CheckoutPayResponse
                {
                    Success = false,
                    OrderId = $"order_{order.PaymentOrderId}",
                    Status = "failed",
                    Message = "Payment declined. Please check your card details or try a different payment method.",
                    Amount = order.Amount,
                    PaymentMode = request.PaymentMode
                };
            }
        }

        public async Task<CheckoutVerifyResponse> VerifyPaymentAsync(string apiKey, CheckoutVerifyRequest request)
        {
            var apiKeyRecord = await _context.MerchantApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.ApiKey == apiKey)
                ?? throw new UnauthorizedAccessException("Invalid API key.");

            var expectedSignature = GenerateSignature(request.BankupgPaymentId, request.BankupgOrderId, apiKeyRecord.ApiSalt ?? apiKey);
            var isValid = string.Equals(expectedSignature, request.BankupgSignature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
                return new CheckoutVerifyResponse { IsValid = false, Message = "Signature verification failed." };

            if (!long.TryParse(request.BankupgOrderId.Replace("order_", ""), out var orderId))
                return new CheckoutVerifyResponse { IsValid = false, Message = "Invalid order ID." };

            var transaction = await _context.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.OrderId == orderId && t.PayuId == request.BankupgPaymentId && t.Mid == apiKeyRecord.Mid);

            if (transaction == null)
                return new CheckoutVerifyResponse { IsValid = false, Message = "Payment not found." };

            return new CheckoutVerifyResponse
            {
                IsValid = true,
                PaymentId = transaction.PayuId,
                OrderId = request.BankupgOrderId,
                Status = transaction.Status,
                Amount = transaction.Amount ?? 0,
                PaymentMode = transaction.PaymentMode,
                PaidAt = transaction.TransactionDate,
                Message = "Payment verified successfully."
            };
        }

        public async Task<CheckoutStatusResponse?> GetOrderStatusAsync(string apiKey, string orderId)
        {
            var apiKeyRecord = await _context.MerchantApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.ApiKey == apiKey)
                ?? throw new UnauthorizedAccessException("Invalid API key.");

            if (!long.TryParse(orderId.Replace("order_", ""), out var id)) return null;

            var order = await _context.PaymentOrders.AsNoTracking()
                .Include(o => o.Transactions)
                .FirstOrDefaultAsync(o => o.PaymentOrderId == id && o.Mid == apiKeyRecord.Mid);

            if (order == null) return null;

            var txn = order.Transactions.FirstOrDefault(t => t.Status == "success");

            return new CheckoutStatusResponse
            {
                OrderId = $"order_{order.PaymentOrderId}",
                OrderRef = order.OrderRef!,
                Amount = order.Amount,
                Currency = order.Currency ?? "INR",
                Status = order.Status ?? "created",
                PaymentId = txn?.PayuId,
                PaymentMode = txn?.PaymentMode,
                PaidAt = txn?.TransactionDate,
                CreatedDate = order.CreatedDate!.Value,
                ExpiryDate = order.ExpiryDate ?? DateTime.UtcNow
            };
        }

        private async Task ComputeTransactionChargeAsync(
            Infrastructure.Entities.Transaction transaction,
            string? paymentMode,
            string? cardNumber)
        {
            var network = DetectCardNetwork(cardNumber, paymentMode);

            var charge = await _context.PaymentMethodCharges
                .Where(c => c.IsActive
                    && c.PaymentMethodType == paymentMode
                    && c.NetworkName == network)
                .FirstOrDefaultAsync()
                ?? await _context.PaymentMethodCharges
                .Where(c => c.IsActive
                    && c.PaymentMethodType == paymentMode
                    && c.NetworkName == null)
                .FirstOrDefaultAsync();

            if (charge == null)
            {
                _logger.LogInformation("No PaymentMethodCharge configured for mode '{Mode}', skipping charge record.", paymentMode);
                return;
            }

            var amount = transaction.Amount ?? 0m;
            decimal chargeAmount;

            if (string.Equals(charge.ChargeType, "Percentage", StringComparison.OrdinalIgnoreCase))
                chargeAmount = Math.Round(amount * charge.ChargeValue / 100m, 2);
            else
                chargeAmount = charge.ChargeValue;

            if (charge.MinCharge.HasValue && chargeAmount < charge.MinCharge.Value)
                chargeAmount = charge.MinCharge.Value;
            if (charge.MaxCharge.HasValue && chargeAmount > charge.MaxCharge.Value)
                chargeAmount = charge.MaxCharge.Value;

            var gstAmount    = Math.Round(chargeAmount * charge.GstPercentage / 100m, 2);
            var totalDeduct  = chargeAmount + gstAmount;
            var netAmount    = amount - totalDeduct;

            _context.TransactionCharges.Add(new Infrastructure.Entities.TransactionCharge
            {
                TransactionId        = transaction.TransactionId,
                Mid                  = transaction.Mid,
                PaymentMethodChargeId = charge.PaymentMethodChargeId,
                PaymentMethodType    = paymentMode,
                NetworkName          = network,
                ChargeType           = charge.ChargeType,
                ChargeValue          = charge.ChargeValue,
                TransactionAmount    = amount,
                ChargeAmount         = chargeAmount,
                GstAmount            = gstAmount,
                TotalDeduction       = totalDeduct,
                NetAmount            = netAmount,
                CreatedDate          = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("TransactionCharge saved: Txn={TxnId} Mode={Mode} Net={Net}",
                transaction.TransactionId, paymentMode, netAmount);
        }

        private static string? DetectCardNetwork(string? cardNumber, string? paymentMode)
        {
            if (!string.Equals(paymentMode, "Card", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(cardNumber))
                return null;

            var raw = cardNumber.Replace(" ", "").Replace("-", "");
            if (raw.StartsWith("34") || raw.StartsWith("37")) return "Amex";
            if (raw.StartsWith("4")) return "Visa";
            if (raw.StartsWith("6")) return "RuPay";
            if (raw.Length >= 2 && int.TryParse(raw[..2], out var p2) && p2 >= 51 && p2 <= 55) return "Mastercard";
            if (raw.Length >= 4 && int.TryParse(raw[..4], out var p4) && p4 >= 2221 && p4 <= 2720) return "Mastercard";
            return null;
        }

        private bool SimulatePayment(CheckoutPayCardRequest req)
        {
            if (req.PaymentMode == "UPI")
                return req.UpiVpa?.ToLower() != "fail@upi";

            if (req.PaymentMode == "NetBanking")
                return req.BankCode?.ToLower() != "fail";

            var rawCard = (req.CardNumber ?? "").Replace(" ", "").Replace("-", "");
            return !FailCardNumbers.Contains(rawCard);
        }

        private static string GenerateCheckoutToken(long orderId, string salt)
        {
            var data = $"{orderId}:{DateTime.UtcNow.Ticks}";
            var hash = ComputeHmac(data, salt).Substring(0, 16);
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{orderId}:{hash}"))
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
            return encoded;
        }

        private static long ExtractOrderIdFromToken(string token)
        {
            try
            {
                var padded = token.Replace("-", "+").Replace("_", "/");
                var mod4 = padded.Length % 4;
                if (mod4 > 0) padded += new string('=', 4 - mod4);
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                var parts = decoded.Split(':');
                if (parts.Length >= 1 && long.TryParse(parts[0], out var id)) return id;
            }
            catch { }
            return 0;
        }

        public static string GenerateSignature(string paymentId, string orderId, string salt)
            => ComputeHmac($"{paymentId}|{orderId}", salt);

        private static string ComputeHmac(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLowerInvariant();
        }

        private static string GenerateUniqueId()
            => Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant();

        private static string BuildCallbackUrl(Infrastructure.Entities.PaymentOrder order, string paymentId, string signature, bool success)
        {
            var notes = order.Notes ?? "";
            if (notes.StartsWith("callback="))
            {
                var cb = notes.Replace("callback=", "");
                return $"{cb}?bankupg_payment_id={paymentId}&bankupg_order_id=order_{order.PaymentOrderId}&bankupg_signature={signature}&status={(success ? "success" : "failed")}";
            }
            return $"/checkout-result?payment_id={paymentId}&order_id=order_{order.PaymentOrderId}&status={(success ? "success" : "failed")}";
        }

        private Task FireWebhookAsync(Infrastructure.Entities.PaymentOrder order, string paymentId)
        {
            _ = Task.Run(async () =>
            {
                var webhook = await _context.Webhooks
                    .FirstOrDefaultAsync(w => w.Mid == order.Mid && w.Status == "Active" && w.Event == "payment.success");
                if (webhook?.WebhookUrl != null)
                {
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        var payload = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            event_type = "payment.success",
                            payment_id = paymentId,
                            order_id = $"order_{order.PaymentOrderId}",
                            amount = order.Amount,
                            currency = order.Currency,
                            created_at = DateTime.UtcNow
                        });
                        await client.PostAsync(webhook.WebhookUrl,
                            new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json"));
                    }
                    catch { }
                }
            });
            return Task.CompletedTask;
        }
    }
}
