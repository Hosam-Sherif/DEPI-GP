using System;
using System.Threading.Tasks;
using Mazaad.Application.DTOs.Payout;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/seller-bank-accounts")]
    [Authorize]
    public class SellerBankAccountController : ControllerBase
    {
        private readonly ISellerBankAccountService _bankAccountService;

        public SellerBankAccountController(ISellerBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        /// <summary>
        /// Retrieves all registered bank accounts and wallets for the logged-in seller company.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Forbid("Access denied: missing company context.");

            // Mask details for normal listing view
            var result = await _bankAccountService.GetAccountsForCompanyAsync(companyId.Value, includeDeleted: false);
            return Ok(result);
        }

        /// <summary>
        /// Registers a new payout account or wallet destination.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSellerBankAccountDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Forbid("Access denied: missing company context.");

            try
            {
                var result = await _bankAccountService.AddBankAccountAsync(companyId.Value, request);
                return CreatedAtAction(nameof(GetMyAccounts), null, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Designates a registered account as the primary default payout destination.
        /// </summary>
        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Forbid("Access denied: missing company context.");

            try
            {
                var result = await _bankAccountService.SetDefaultAccountAsync(companyId.Value, id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soft-deletes a registered account.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Forbid("Access denied: missing company context.");

            try
            {
                var deleted = await _bankAccountService.DeleteAccountAsync(companyId.Value, id);
                return deleted 
                    ? Ok(new { message = "Account successfully deleted." })
                    : NotFound(new { message = "Bank account not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verifies a registered seller bank account.
        /// SuperAdmin only.
        /// </summary>
        [HttpPatch("{id}/verify")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Verify(int id)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId <= 0)
                return Unauthorized(new { message = "Invalid admin user token." });

            try
            {
                var result = await _bankAccountService.VerifyAccountAsync(id, adminUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
