using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mazaad.Infrastructure.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly PaymobClient _paymob;
        private readonly PaymobOptions _options;
        private readonly INotificationService _notificationService;
        private readonly IEscrowService _escrowService;

        public PaymentService(
            AppDbContext context,
            PaymobClient paymob,
            IOptions<PaymobOptions> options,
            INotificationService notificationService,
            IEscrowService escrowService)
        {
            _context = context;
            _paymob = paymob;
            _options = options.Value;
            _notificationService = notificationService;
            _escrowService = escrowService;
        }


        // ─── Initiate ─────────────────────────────────────────────────────────

        public async Task<PaymentInitiationResponseDto> InitiatePaymentAsync(
            int companyId, CreatePaymentRequestDto request)
        {
            // ── Validate order ──────────────────────────────────────────────────
            var order = await _context.Orders
                .Include(o => o.BuyerCompany)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new InvalidOperationException("Order not found.");

            if (order.BuyerCompanyId != companyId)
                throw new UnauthorizedAccessException("Only the buyer company can pay for this order.");

            if (order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("This order has already been paid.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("This order was cancelled and can't be paid.");

            // ── Validate wallet number ──────────────────────────────────────────
            var isWallet = request.Method != PaymentMethodType.Card;
            if (isWallet)
            {
                if (string.IsNullOrWhiteSpace(request.WalletMobileNumber))
                    throw new InvalidOperationException("رقم الموبايل مطلوب لطرق الدفع بالمحفظة الإلكترونية.");

                if (!IsValidEgyptianMobile(request.WalletMobileNumber))
                    throw new InvalidOperationException("رقم الموبايل غير صحيح. يجب أن يبدأ بـ 01 ويكون 11 رقم.");

                ValidateWalletConfig(request.Method);
            }

            // ── Reuse pending payment or create new ─────────────────────────────
            var methodName = request.Method.ToString();
            var payment = await _context.Payments
                .Where(p => p.OrderId == order.Id
                         && p.Status == PaymentStatus.Pending
                         && p.PaymentMethod == methodName)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = new Payments
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Currency = "EGP",
                    PaymentMethod = methodName,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            // ── Buyer info ──────────────────────────────────────────────────────
            var buyerUser = await _context.Users.FirstOrDefaultAsync(u => u.CompanyId == companyId);
            var buyerEmail = buyerUser?.Email ?? "buyer@mazaad.com";
            var buyerPhone = buyerUser?.PhoneNumber ?? "01000000000";
            var buyerName = order.BuyerCompany?.CompanyName ?? "Mazaad Buyer";
            var amountCents = (long)Math.Round(order.TotalAmount * 100, MidpointRounding.AwayFromZero);

            // ── Paymob flow ─────────────────────────────────────────────────────
            var authToken = await _paymob.AuthenticateAsync();
            var paymobOrderId = await _paymob.RegisterOrderAsync(authToken, order.Id, amountCents, payment.Currency);

            string? iframeUrl = null;
            string? redirectUrl = null;

            if (!isWallet)
            {
                // Card: نرجع iframe URL
                var paymentToken = await _paymob.RequestCardPaymentKeyAsync(
                    authToken, paymobOrderId, amountCents, payment.Currency,
                    buyerEmail, buyerPhone, buyerName, "Company");

                payment.ProviderOrderId = paymobOrderId;
                payment.PaymentToken = paymentToken;
                iframeUrl = _paymob.BuildIframeUrl(paymentToken);
            }
            else
            {
                // Wallet: Paymob بيبعت OTP للعميل
                var walletResult = await _paymob.RequestWalletPaymentAsync(
                    authToken, paymobOrderId, amountCents, payment.Currency,
                    buyerEmail, buyerPhone, buyerName, "Company",
                    request.WalletMobileNumber!, request.Method);

                payment.ProviderOrderId = paymobOrderId;
                payment.PaymentToken = walletResult.PaymentToken;
                redirectUrl = walletResult.RedirectUrl;
            }

            await _context.SaveChangesAsync();

            return new PaymentInitiationResponseDto
            {
                PaymentId = payment.Id,
                IframeUrl = iframeUrl,
                RedirectUrl = redirectUrl,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Method = request.Method
            };
        }

        // ─── Lookup ───────────────────────────────────────────────────────────

        public async Task<PaymentResponseDto?> GetPaymentForOrderAsync(int orderId)
        {
            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return payment == null ? null : MapToDto(payment);
        }

        // ─── Webhook ──────────────────────────────────────────────────────────

        public async Task<bool> HandlePaymobWebhookAsync(JsonElement payload, string hmacFromQuery)
        {
            if (!payload.TryGetProperty("obj", out var obj))
                return false;

            if (!VerifyHmac(obj, hmacFromQuery))
                return false;

            var success = obj.GetProperty("success").GetBoolean();
            var transactionId = obj.GetProperty("id").GetRawText();
            var merchantOrderId = obj.GetProperty("order").GetProperty("merchant_order_id").GetString();

            if (!int.TryParse(merchantOrderId, out var orderId))
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = await _context.Payments
                    .Where(p => p.OrderId == orderId)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (payment == null) return false;

                payment.ProviderTransactionId = transactionId;
                payment.TransactionReference = transactionId;

                Orders? order = null;

                if (success)
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;

                    order = await _context.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        // Escrow flow: order moves to Processing (not Completed yet)
                        order.Status = OrderStatus.Processing;
                        order.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();

                if (success && order != null)
                {
                    // Create the EscrowRecord under platform custody
                    await _escrowService.CreateEscrowAsync(order.Id, payment.Id);

                    // Send notification to seller
                    var sellerUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.CompanyId == order.SellerCompanyId);

                    if (sellerUser != null)
                    {
                        await _notificationService.CreateNotificationAsync(
                            sellerUser.Id,
                            "Payment received",
                            $"Order #{order.Id} has been paid successfully via {payment.PaymentMethod}. Funds are held in escrow.",
                            "Order",
                            order.Id);
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // ─── Refund ───────────────────────────────────────────────────────────

        public async Task<bool> InitiateRefundAsync(int orderId, string reason)
        {
            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Paid)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null || string.IsNullOrEmpty(payment.ProviderTransactionId))
                throw new InvalidOperationException($"No completed payment record found to refund for Order #{orderId}.");

            try
            {
                var authToken = await _paymob.AuthenticateAsync();
                var amountCents = (long)Math.Round(payment.Amount * 100, MidpointRounding.AwayFromZero);

                var success = await _paymob.RefundTransactionAsync(
                    authToken, 
                    payment.ProviderTransactionId, 
                    amountCents);

                if (success)
                {
                    payment.Status = PaymentStatus.Refunded;
                    await _context.SaveChangesAsync();

                    // Send notification to buyer
                    var buyerUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.CompanyId == payment.Order.BuyerCompanyId);

                    if (buyerUser != null)
                    {
                        await _notificationService.CreateNotificationAsync(
                            buyerUser.Id,
                            "Refund Processed",
                            $"Payment of {payment.Amount} {payment.Currency} for Order #{orderId} has been refunded to your account.",
                            "Order",
                            orderId);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // ─── Helpers ──────────────────────────────────────────────────────────

        private static bool IsValidEgyptianMobile(string number)
        {
            var clean = number.Trim().Replace(" ", "").Replace("-", "");
            return clean.Length == 11 && clean.StartsWith("01") &&
                   (clean[2] == '0' || clean[2] == '1' || clean[2] == '2' || clean[2] == '5');
        }

        private void ValidateWalletConfig(PaymentMethodType method)
        {
            var id = method switch
            {
                PaymentMethodType.VodafoneCash => _options.VodafoneIntegrationId,
                PaymentMethodType.OrangeMoney => _options.OrangeIntegrationId,
                PaymentMethodType.EtisalatCash => _options.EtisalatIntegrationId,
                PaymentMethodType.WePay => _options.WePayIntegrationId,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException(
                    $"طريقة الدفع {method} غير مُفعَّلة. تأكد من إضافة Integration ID في الـ appsettings.");
        }

        private bool VerifyHmac(JsonElement obj, string receivedHmac)
        {
            string Get(string name) => obj.TryGetProperty(name, out var v)
                ? v.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => v.ToString()
                }
                : "";

            string GetNested(string parent, string child)
            {
                if (!obj.TryGetProperty(parent, out var p)) return "";
                if (!p.TryGetProperty(child, out var v)) return "";
                return v.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => v.ToString()
                };
            }

            var ordered = string.Concat(
                Get("amount_cents"), Get("created_at"), Get("currency"),
                Get("error_occured"), Get("has_parent_transaction"), Get("id"),
                Get("integration_id"), Get("is_3d_secure"), Get("is_auth"),
                Get("is_capture"), Get("is_refunded"), Get("is_standalone_payment"),
                Get("is_voided"), GetNested("order", "id"), Get("owner"),
                Get("pending"), GetNested("source_data", "pan"),
                GetNested("source_data", "sub_type"), GetNested("source_data", "type"),
                Get("success"));

            var keyBytes = Encoding.UTF8.GetBytes(_options.HmacSecret);
            var messageBytes = Encoding.UTF8.GetBytes(ordered);

            using var hmacSha512 = new HMACSHA512(keyBytes);
            var hash = hmacSha512.ComputeHash(messageBytes);
            var computedHex = Convert.ToHexString(hash).ToLowerInvariant();

            return string.Equals(computedHex, receivedHmac, StringComparison.OrdinalIgnoreCase);
        }

        private static PaymentResponseDto MapToDto(Payments p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            TransactionReference = p.TransactionReference,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        };
    }
}