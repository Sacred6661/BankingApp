using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TransactionService.Controllers;
using TransactionService.Data;
using TransactionService.Data.Models;
using TransactionService.DTOs;
using Xunit;
using Messaging;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TransactionService.Mapping;

public class TransactionControllerTests
{
    private readonly TransactionsDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublish;
    private readonly TransactionController _controller;
    private readonly IMapper _mapper;

    private readonly string _accountNumber = Guid.NewGuid().ToString();
    private readonly string _relatedAccountNumber = Guid.NewGuid().ToString();
    private readonly string _userId = Guid.NewGuid().ToString();

    public TransactionControllerTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => {
            builder.AddConsole();
        });

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, loggerFactory);
        _mapper = config.CreateMapper();

        // Використаємо InMemory DB для мокування DbContext
        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TransactionsDbContext(options);
        _mockPublish = new Mock<IPublishEndpoint>();

        _controller = new TransactionController(_dbContext, _mockPublish.Object, _mapper);

        // Мок користувача з user_id claim
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("user_id", _userId),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task Deposit_ReturnsBadRequest_WhenAmountIsZeroOrLess()
    {
        var request = new DepositRequest { AccountNumber = _accountNumber, Amount = 0 };

        var result = await _controller.Deposit(request);

        var badRequest = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        var problemDetails = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Contains("Amount must be greater than zero", problemDetails.Detail);
    }

    [Fact]
    public async Task Deposit_CreatesTransaction_And_PublishesEvent()
    {
        var request = new DepositRequest { AccountNumber = _accountNumber, Amount = 100 };

        var result = await _controller.Deposit(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transaction = Assert.IsType<TransactionDto>(okResult.Value);

        Assert.Equal(TransactionTypeEnum.Deposit, transaction.TransactionType);
        Assert.Equal(request.AccountNumber, transaction.ToAccount);
        Assert.Equal(100, transaction.Amount);
        Assert.Equal(_userId, transaction.PerformedBy);

        var dbTransaction = await _dbContext.Transactions.FindAsync(transaction.TransactionId);
        Assert.NotNull(dbTransaction);

        _mockPublish.Verify(p => p.Publish(It.IsAny<TransactionCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task Withdraw_ReturnsBadRequest_WhenAmountIsZeroOrLess()
    {
        var request = new WithdrawRequest { AccountNumber = _accountNumber, Amount = 0 };

        var result = await _controller.Withdraw(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Amount must be greater than zero.", badRequest.Value);
    }

    [Fact]
    public async Task Withdraw_CreatesTransaction_And_PublishesEvent()
    {
        var request = new WithdrawRequest { AccountNumber = _accountNumber, Amount = 50 };

        var result = await _controller.Withdraw(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transaction = Assert.IsType<TransactionDto>(okResult.Value);

        Assert.Equal(TransactionTypeEnum.Withdraw, transaction.TransactionType);
        Assert.Equal(request.AccountNumber, transaction.FromAccount);
        Assert.Equal(50, transaction.Amount);
        Assert.Equal(_userId, transaction.PerformedBy);

        var dbTransaction = await _dbContext.Transactions.FindAsync(transaction.TransactionId);
        Assert.NotNull(dbTransaction);

        _mockPublish.Verify(p => p.Publish(It.IsAny<TransactionCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenAmountIsZeroOrLess()
    {
        var request = new TransferRequest { FromAccountNumber = _accountNumber, ToAccountNumber = _relatedAccountNumber, Amount = 0 };

        var result = await _controller.Transfer(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Amount must be greater than zero.", badRequest.Value);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenToAccountIsEmpty()
    {
        var request = new TransferRequest { FromAccountNumber = _accountNumber, ToAccountNumber = null, Amount = 100 };

        var result = await _controller.Transfer(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Destination account is required.", badRequest.Value);
    }

    [Fact]
    public async Task Transfer_CreatesTransaction_And_PublishesEvent()
    {
        var request = new TransferRequest { FromAccountNumber = _accountNumber, ToAccountNumber = _relatedAccountNumber, Amount = 75 };

        var result = await _controller.Transfer(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transaction = Assert.IsType<TransactionDto>(okResult.Value);

        Assert.Equal(TransactionTypeEnum.Transfer, transaction.TransactionType);
        Assert.Equal(_accountNumber, transaction.FromAccount);
        Assert.Equal(_relatedAccountNumber, transaction.ToAccount);
        Assert.Equal(75, transaction.Amount);
        Assert.Equal(_userId, transaction.PerformedBy);

        var dbTransaction = await _dbContext.Transactions.FindAsync(transaction.TransactionId);
        Assert.NotNull(dbTransaction);

        _mockPublish.Verify(p => p.Publish(It.IsAny<TransactionCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task GetTransaction_ReturnsNotFound_WhenTransactionDoesNotExist()
    {
        var result = await _controller.GetTransaction(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }



}
