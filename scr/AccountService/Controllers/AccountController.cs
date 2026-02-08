using AccountService.Data;
using AccountService.Data.Models;
using AccountService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AccountService.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class AccountController(AccountDbContext dbContext, IMapper mapper, HttpClient client) : ControllerBase
    {
        private readonly AccountDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly HttpClient _client = client;

        [Authorize(Policy = "RequireUserId")]
        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var userId = User.FindFirst("user_id")?.Value;
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            decimal initialBalance = 0;

            // Only Admin have access to create account for any user with any balance
            // Users can add accounts for themselves with with initial balanve 0
            if (userRole?.ToLower()?.Contains("Admin".ToLower()) ?? false)
            {
                if (request.UserId != null)
                    userId = request.UserId.ToString();

                initialBalance = request.InitialBalance;
            }

            if (userRole == null)
            {
                return Problem(
                    detail: "Cannot find user role in the database. contact the administrator",
                    statusCode: 401,
                    title: "Unauthorized",
                    type: "https://httpstatuses.com/401"
                );
            }

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                Balance = initialBalance
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            var result = new AccountsDto();
            _mapper.Map(account, result);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> GetAccount(Guid? accountId) 
        {
            var userId = User.FindFirst("user_id")?.Value;
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (accountId == null)
            {
                return Problem(
                    detail: $"Id {accountId} is not valid",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    type: "https://httpstatuses.com/400"
                );
            }

            if (userRole == null)
            {
                return Problem(
                    detail: "Cannot find user role in the database. contact the administrator",
                    statusCode: 401,
                    title: "Unauthorized",
                    type: "https://httpstatuses.com/401"
                );
            }

            var account = await _dbContext.Accounts.Where(a => a.Id == accountId).FirstOrDefaultAsync();

            if(account == null)
            {
                return Problem(
                    detail: $"Account with id {accountId} not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    type: "https://httpstatuses.com/404"
                );
            }

            // only Admin can access any accounts info, Users have access only to their accounts
            if (!userRole?.ToLower()?.Contains("Admin".ToLower()) ?? false)
            {
                if(account.UserId != Guid.Parse(userId))
                     return Problem(
                        detail: "You have no access to other people's accounts",
                        statusCode: 403,
                        title: "Forbidden",
                        type: "https://httpstatuses.com/403"
                    );
            }


            var result = new AccountsDto();
            _mapper.Map(account, result);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllAccounts(bool everyoneAccounts = false)
        {
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (userRole == null)
            {
                return Problem(
                    detail: "Cannot find user role in the database. contact the administrator",
                    statusCode: 401,
                    title: "Unauthorized",
                    type: "https://httpstatuses.com/401"
                );
            }

            var accounts = new List<Account>();
            var result = new List<AccountsDto>();

            if(everyoneAccounts && (userRole?.ToLower().Contains("admin") ?? false) )
            {
                accounts = await _dbContext.Accounts.ToListAsync();
                _mapper.Map(accounts, result);

                return Ok(result);
            }

            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);

            accounts = await _dbContext.Accounts.Where(a => a.UserId == userId).ToListAsync();
            _mapper.Map(accounts, result);

            return Ok(result);
        }
    }
}
