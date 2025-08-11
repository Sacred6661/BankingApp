using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountService.Controllers;
using AccountService.Data;
using AccountService.Data.Models;
using AccountService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AccountService.Tests
{
    public class AccountControllerTests
    {
        private AccountDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return (AccountDbContext)Activator.CreateInstance(typeof(AccountDbContext), options);
        }

        private ClaimsPrincipal CreateUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim("user_id", userId)
            };
            if (role != null)
                claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private AccountController CreateController(AccountDbContext db, Mock<IMapper> mapperMock, ClaimsPrincipal user)
        {
            var httpClient = new System.Net.Http.HttpClient();
            var controller = new AccountController(db, mapperMock.Object, httpClient);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task CreateAccount_AdminCanCreateForAnyUser_WithInitialBalance()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(CreateAccount_AdminCanCreateForAnyUser_WithInitialBalance));
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(It.IsAny<Account>(), It.IsAny<AccountsDto>()))
                .Callback<Account, AccountsDto>((a, d) =>
                {
                    d.Id = a.Id;
                    d.UserId = a.UserId;
                    d.Balance = a.Balance;
                });

            var adminUserId = Guid.NewGuid().ToString();
            var userToCreateFor = Guid.NewGuid().ToString();
            var user = CreateUser(adminUserId, "Admin");

            var controller = CreateController(db, mapperMock, user);

            var req = new CreateAccountRequest
            {
                UserId = Guid.Parse(userToCreateFor),
                InitialBalance = 123.45m
            };

            // act
            var result = await controller.CreateAccount(req) as OkObjectResult;

            // assert
            Assert.NotNull(result);
            var dto = Assert.IsType<AccountsDto>(result.Value);
            Assert.Equal(req.InitialBalance, dto.Balance);
            Assert.Equal(req.UserId, dto.UserId);
        }



        [Fact]
        public async Task CreateAccount_NoRole_ReturnsProblem_401()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(CreateAccount_NoRole_ReturnsProblem_401));
            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(Guid.NewGuid().ToString(), role: null);

            var controller = CreateController(db, mapperMock, user);
            var req = new CreateAccountRequest { InitialBalance = 0 };

            // act
            var result = await controller.CreateAccount(req) as ObjectResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task GetAccount_InvalidGuid_ReturnsBadRequestProblem()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAccount_InvalidGuid_ReturnsBadRequestProblem));
            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(Guid.NewGuid().ToString(), "User");
            var controller = CreateController(db, mapperMock, user);

            // act
            var result = await controller.GetAccount("not-a-guid") as ObjectResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetAccount_NoRole_Returns401Problem()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAccount_NoRole_Returns401Problem));
            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(Guid.NewGuid().ToString(), role: null);
            var controller = CreateController(db, mapperMock, user);

            var id = Guid.NewGuid().ToString();

            // act
            var result = await controller.GetAccount(id) as ObjectResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task GetAccount_NotFound_Returns404Problem()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAccount_NotFound_Returns404Problem));
            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(Guid.NewGuid().ToString(), "Admin");
            var controller = CreateController(db, mapperMock, user);

            var id = Guid.NewGuid().ToString();

            // act
            var result = await controller.GetAccount(id) as ObjectResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetAccount_NonAdminAccessingOthersAccount_Returns401Problem()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAccount_NonAdminAccessingOthersAccount_Returns401Problem));
            var mapperMock = new Mock<IMapper>();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var account = new Account { Id = Guid.NewGuid(), UserId = ownerId, Balance = 10m };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var user = CreateUser(otherUserId.ToString(), "User");
            var controller = CreateController(db, mapperMock, user);

            // act
            var result = await controller.GetAccount(account.Id.ToString()) as ObjectResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task GetAccount_AdminCanGetAnyAccount_ReturnsOk()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAccount_AdminCanGetAnyAccount_ReturnsOk));
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(It.IsAny<Account>(), It.IsAny<AccountsDto>()))
                .Callback<Account, AccountsDto>((a, d) =>
                {
                    d.Id = a.Id;
                    d.UserId = a.UserId;
                    d.Balance = a.Balance;
                });

            var ownerId = Guid.NewGuid();
            var account = new Account { Id = Guid.NewGuid(), UserId = ownerId, Balance = 50m };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var user = CreateUser(Guid.NewGuid().ToString(), "Admin");
            var controller = CreateController(db, mapperMock, user);

            // act
            var result = await controller.GetAccount(account.Id.ToString()) as OkObjectResult;

            // assert
            Assert.NotNull(result);
            var dto = Assert.IsType<AccountsDto>(result.Value);
            Assert.Equal(account.Id, dto.Id);
        }

        [Fact]
        public async Task GetAllAccounts_GetAllFlagTrue_ReturnsAll()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAllAccounts_GetAllFlagTrue_ReturnsAll));
            db.Accounts.AddRange(new[]
            {
                new Account { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Balance = 1 },
                new Account { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Balance = 2 }
            });
            await db.SaveChangesAsync();

            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(Guid.NewGuid().ToString(), "Admin");
            var controller = CreateController(db, mapperMock, user);

            // act
            var result = await controller.GetAllAccounts(getAllAccounts: true) as OkObjectResult;

            // assert
            Assert.NotNull(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Account>>(result.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetAllAccounts_DefaultFiltersToUserAccounts()
        {
            // arrange
            var db = CreateInMemoryDb(nameof(GetAllAccounts_DefaultFiltersToUserAccounts));
            var userId = Guid.NewGuid();
            db.Accounts.AddRange(new[]
            {
                new Account { Id = Guid.NewGuid(), UserId = userId, Balance = 1 },
                new Account { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Balance = 2 }
            });
            await db.SaveChangesAsync();

            var mapperMock = new Mock<IMapper>();
            var user = CreateUser(userId.ToString(), "User");
            var controller = CreateController(db, mapperMock, user);

            // act
            var result = await controller.GetAllAccounts() as OkObjectResult;

            // assert
            Assert.NotNull(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Account>>(result.Value);
            Assert.Single(list);
            Assert.All(list, a => Assert.Equal(userId, a.UserId));
        }
    }
}
