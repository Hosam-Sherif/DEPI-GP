using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    // طريقة الدفع
    public enum PaymentMethodType
    {
        Card = 0,           // كارت أونلاين (الحالي)
        VodafoneCash = 1,   // فودافون كاش
        OrangeMoney = 2,    // أورنج موني
        EtisalatCash = 3,   // اتصالات كاش (e&)
        WePay = 4           // WE Pay
    }

    /// <summary>Sent by the client to start paying for an order.</summary>
    public class CreatePaymentRequestDto
    {
        public int OrderId { get; set; }

        /// <summary>
        /// طريقة الدفع. Default = Card.
        /// </summary>
        public PaymentMethodType Method { get; set; } = PaymentMethodType.Card;

        /// <summary>
        /// رقم المحفظة — مطلوب فقط لو Method مش Card.
        /// لازم يبدأ بـ 01 ويكون 11 رقم.
        /// </summary>
        public string? WalletMobileNumber { get; set; }
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    public class PaymentInitiationResponseDto
    {
        public int PaymentId { get; set; }

        /// <summary>Card: iframe URL. Wallets: null.</summary>
        public string? IframeUrl { get; set; }

        /// <summary>للمحافظ: redirect URL من Paymob.</summary>
        public string? RedirectUrl { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentMethodType Method { get; set; }
    }

    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string? TransactionReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}