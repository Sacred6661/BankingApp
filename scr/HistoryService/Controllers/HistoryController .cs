using HistoryService.Data;
using HistoryService.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace HistoryService.Controllers
{
    [ApiController]
    [Route("api/v1/history")]
    public class HistoryController(HistoryContext dbContext) : ControllerBase
    {
        private readonly HistoryContext _dbContext = dbContext;

        /// <summary>
        /// Get history data by filter
        /// </summary>
        /// <param name="transactionId">Transaction ID (exact match)</param>
        /// <param name="accountNumber">Account number (exact match)</param>
        /// <param name="relatedAccountNumber">Related account (transfer only)</param>
        /// <param name="eventType">Event type</param>
        /// <param name="status">Transaction status</param>
        /// <param name="performedBy">Who performed action (userId)</param>
        /// <param name="performedByService">Service name that performed </param>
        /// <param name="from">Period begining (UTC)</param>
        /// <param name="to">Period ending (UTC)</param>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string transactionId,
            [FromQuery] string accountNumber,
            [FromQuery] string relatedAccountNumber,
            [FromQuery] int? eventType,
            [FromQuery] int? status,
            [FromQuery] string performedBy,
            [FromQuery] string performedByService,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var builder = Builders<HistoryEvent>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(transactionId))
                filter &= builder.Eq(e => e.TransactionId, transactionId);

            if (!string.IsNullOrEmpty(accountNumber))
                filter &= builder.Eq(e => e.AccountNumber, accountNumber);

            if (!string.IsNullOrEmpty(relatedAccountNumber))
                filter &= builder.Eq(e => e.RelatedAccountNumber, relatedAccountNumber);

            if (eventType.HasValue)
                filter &= builder.Eq(e => e.EventType, eventType.Value);

            if (status.HasValue)
                filter &= builder.Eq(e => e.TransactionStatus, status.Value);

            if (!string.IsNullOrEmpty(performedBy))
                filter &= builder.Eq(e => e.PerformedBy, performedBy);

            if (!string.IsNullOrEmpty(performedByService))
                filter &= builder.Eq(e => e.PerformedByService, performedByService);

            if (from.HasValue)
                filter &= builder.Gte(e => e.Timestamp, from.Value);

            if (to.HasValue)
                filter &= builder.Lte(e => e.Timestamp, to.Value);

            var results = await _dbContext.HistoryEvents.Find(filter).SortByDescending(e => e.Timestamp).ToListAsync();

            if (results.Count == 0)
                return NotFound("No history events found matching the criteria.");

            return Ok(results);
        }

        /// <summary>
        /// Get all events for transaction (by transactionId)
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetByTransactionId(string transactionId)
        {
            var events = await _dbContext.HistoryEvents.Find(e => e.TransactionId == transactionId).SortBy(e => e.Timestamp).ToListAsync();

            if (events.Count == 0)
                return NotFound($"No events found for transaction ID {transactionId}");

            return Ok(events);
        }

        /// <summary>
        /// All events for some account (accountNumber)
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("account/{accountNumber}")]
        public async Task<IActionResult> GetByAccountNumber(string accountNumber)
        {
            var events = await _dbContext.HistoryEvents.Find(e => e.AccountNumber == accountNumber).SortByDescending(e => e.Timestamp).ToListAsync();

            if (events.Count == 0)
                return NotFound($"No events found for account number {accountNumber}");

            return Ok(events);
        }
    }
}
