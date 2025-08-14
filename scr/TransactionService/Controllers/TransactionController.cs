using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Data.Models;
using TransactionService.Data;
using TransactionService.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Messaging;
using MassTransit;
using AutoMapper;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TransactionController(TransactionsDbContext dbContext, IPublishEndpoint publish, IMapper _mapper) : ControllerBase
    {
        private readonly TransactionsDbContext _dbContext = dbContext;
        private readonly IPublishEndpoint _publish = publish;

        [Authorize(Policy = "RequireUserId")]
        [HttpPost("transactions/deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var userId = User.FindFirst("user_id")?.Value;

            if (request.Amount <= 0)
            {
                return Problem(
                    detail: $"Amount must be greater than zero.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Reques",
                    type: "https://httpstatuses.com/400"
                );
            }

            // TODO: Add Http call to account service to check if account exists

            var transaction = new Transaction
            {
                TransactionTypeEnum = TransactionTypeEnum.Deposit,
                TransactionStatusEnum = TransactionStatusEnum.Pending,
                ToAccount = request.AccountNumber,
                Amount = request.Amount,
                PerformedBy = userId
            };

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            var transactionCreated = new TransactionCreated()
            {
                AccountNumber = request.AccountNumber,
                Amount = request.Amount.ToString(),
                TransactionType = (int)TransactionTypeEnum.Deposit,
                PerformedBy = userId,
                TransactionId = transaction.TransactionId.ToString(),
                TransactionStatus = transaction.TransactionStatusId,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            await _publish.Publish(transactionCreated);

            var result = _mapper.Map<TransactionDto>(transaction);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            var userId = User.FindFirst("user_id")?.Value;
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            // TODO: Add Http call to account service to check if account exists
            // TODO: ALso if user is not Admin check if from account is users one

            var transaction = new Transaction
            {
                TransactionTypeEnum = TransactionTypeEnum.Withdraw,
                TransactionStatusEnum = TransactionStatusEnum.Pending,
                FromAccount = request.AccountNumber,
                Amount = request.Amount,
                PerformedBy = userId
            };

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            var transactionCreated = new TransactionCreated()
            {
                AccountNumber = request.AccountNumber,
                Amount = request.Amount.ToString(),
                TransactionType = (int)TransactionTypeEnum.Withdraw,
                PerformedBy = userId,
                TransactionId = transaction.TransactionId.ToString(),
                TransactionStatus = transaction.TransactionStatusId,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            await _publish.Publish(transactionCreated);

            var result = new TransactionDto();
            _mapper.Map(transaction, result);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPost("transactions/transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            var userId = User.FindFirst("user_id")?.Value;
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            if (string.IsNullOrEmpty(request.ToAccountNumber))
                return BadRequest("Destination account is required.");

            // TODO: Add Http call to account service to check if accounts exist
            // TODO: ALso if user is not Admin check if from account is users one

            var transaction = new Transaction
            {
                TransactionTypeEnum = TransactionTypeEnum.Transfer,
                TransactionStatusEnum = TransactionStatusEnum.Pending,
                FromAccount = request.FromAccountNumber,
                ToAccount = request.ToAccountNumber,
                Amount = request.Amount,
                PerformedBy = userId
            };

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            var transactionCreated = new TransactionCreated()
            {
                AccountNumber = request.FromAccountNumber,
                RelatedAccountNumber = request.ToAccountNumber,
                Amount = request.Amount.ToString(),
                TransactionType = (int)TransactionTypeEnum.Transfer,
                PerformedBy = userId,
                TransactionId = transaction.TransactionId.ToString(),
                TransactionStatus = transaction.TransactionStatusId,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            await _publish.Publish(transactionCreated);

            var result = _mapper.Map<TransactionDto>(transaction);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {

            // TODO: if user is not Admin check if from account is users one

            var transaction = await _dbContext.Transactions
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            var result = _mapper.Map<TransactionDto>(transaction);

            return result is null ? NotFound() : Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst("user_id")?.Value;
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var transactions = await _dbContext.Transactions
                .Include(t => t.TransactionType)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            var result = _mapper.Map<List<TransactionDto>>(transactions);

            //TODO: if user is not admin show only transactions with his accounts

            return Ok(result);
        }
    }
}
