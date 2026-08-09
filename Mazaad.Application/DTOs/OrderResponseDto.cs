using System;
using Mazaad.Domain.Enums;
using Mazaad.Application.DTOs.Escrow;

namespace Mazaad.Application.DTOs
{
    /// <summary>Order detail response returned after finalizing a winning bid.</summary>
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public int SellerCompanyId { get; set; }
        public string SellerCompanyName { get; set; } = string.Empty;
        public int BuyerCompanyId { get; set; }
        public string BuyerCompanyName { get; set; } = string.Empty;
        public decimal AgreedQuantity { get; set; }
        public decimal AgreedUnitPrice { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Lightweight escrow status summary for display on the order page.
        /// Null if the buyer has not paid yet (escrow not created).
        /// </summary>
        public EscrowStatusSummaryDto? Escrow { get; set; }
    }
}

