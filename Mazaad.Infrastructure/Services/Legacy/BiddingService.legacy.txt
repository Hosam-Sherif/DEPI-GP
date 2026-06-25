using Mazaad.Application.DTOs.Bidding;
using Mazaad.Application.Interfaces.Repositories;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Services.Bidding
{
    public class BiddingService : IBiddingService
    {
        private readonly IBiddingRepository _repository;

        public BiddingService(
            IBiddingRepository repository)
        {
            _repository = repository;
        }

        public async Task<BidResultDto> PlaceBidAsync(
            int userId,
            int companyId,
            PlaceBidDto dto)
        {
            var listing =
                await _repository.GetListingAsync(dto.ListingId);

            if (listing == null)
            {
                return new BidResultDto
                {
                    Success = false,
                    Message = "Auction not found"
                };
            }

            if (DateTime.UtcNow > listing.end_date)
            {
                return new BidResultDto
                {
                    Success = false,
                    Message = "Auction ended"
                };
            }

            if (dto.BidAmountPerUnit <= listing.current_price)
            {
                return new BidResultDto
                {
                    Success = false,
                    Message =
                        "Bid must be higher than current price"
                };
            }

            var bid = new Bids
            {
                listing_id = dto.ListingId,
                placed_by_user_id = userId,
                buyer_company_id = companyId,

                bid_amount_per_unit =
                    dto.BidAmountPerUnit,

                total_bid_amount =
                    dto.BidAmountPerUnit * dto.Quantity,

                bid_time = DateTime.UtcNow,

                is_anonymous = dto.IsAnonymous,

                winning_bid = false
            };

            listing.current_price =
                dto.BidAmountPerUnit;

            await _repository.AddBidAsync(bid);

            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BidResultDto
                {
                    Success = false,
                    Message =
                        "Price updated by another bidder"
                };
            }

            return new BidResultDto
            {
                Success = true,

                Message = "Bid placed successfully",

                NewBidId = bid.Id,

                NewPrice = bid.bid_amount_per_unit
            };
        }
    }
}