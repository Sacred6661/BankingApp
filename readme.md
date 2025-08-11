# BankingApp

## How to run local:

To start all services, local run scripts/run-services-local.bat. After some time all servers will be started and you will see a message in terminal. To stop services run from terminal stop-services-local.bat. To avoid problems with script running go to the scripts folder in the terminal and run .bat file from it. 

**Note:** before using script you should run docker-compose, that will start databases and RabbitMQ. Main URL is [https://localhost:7023/](https://localhost:7023/)

## How to run on Docker

To run "docker-compose.yml" go to main folder("BankingApp") and run command:

`docker-compose up -d`

This will run all databases, RabbitMQ and all services in docker. Main URL in this case is [https://localhost:5001/](https://localhost:5001/)

## Endpoints:

**User Identity: (register/login/refresh token):**

1. POST /auth/register  
    
    Register new user with Email and Password

    Params:  
    >   public class RegisterRequest  
       {  
           public string Email { get; set; }  
           public string Password { get; set; }  
       }  
       
    
2. POST /auth/login

    Login with Email and Password to get JWT Token and Refresh Token

    Params:  
      > public class LoginRequest  
       {  
           public string Email { get; set; }  
           public string Password { get; set; }  
       }  
3. POST /auth/refresh-token  

    You can use your Refresh Token to get new JWT Token

    Params:  
    >   public class RefreshTokenRequest  
       {  
           public string RefreshToken { get; set; }  
       }  
   

**Account:**

1. POST /accounts  

    Create new account for the loggined user. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   public class CreateAccountRequest  
       {  
           public Guid? UserId { get; set; }  
           public decimal InitialBalance { get; set; } \= 0;  
       }  
   **Note:** *If user is not Admin - balance will be 0, account will be created for logged user id, even if you place any other value in InitialBalance and UserId .*  
2. GET: /accounts/{id}

    Get account by id. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   string id;  
   **Note**: *Only Admin can access ant account info, user can access info only about his accounts.*  
3. GET: /accounts  

   Get all accounts. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   bool getAllAccounts \= false  
   **Note**: *Only Admin can access all existed accounts. User even if changed getAllAccounts value to true will get info only about his accounts.*  
   

**Transactions:**

1. POST /transactions/deposit  

    Deposit money to the account balance. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   public class DepositRequest  
       {  
           public string AccountNumber { get; set; }  
           public decimal Amount { get; set; }  
       }  
2. POST /transactions/withdraw  

    Withdraw money from the account balance. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   public class WithdrawRequest  
       {  
           public string AccountNumber { get; set; }  
           public decimal Amount { get; set; }  
       }  
3. POST /transactions/transfer
    Transfer money from one account to another. **Note: use JWT Token for Authorization(Bearer Token).**
    Params:  
    >   public class TransferRequest  
       {  
           public string FromAccountNumber { get; set; }  
           public string ToAccountNumber { get; set; }  
           public decimal Amount { get; set; }      
       }

**History(Admin access only):**

1. GET: /history/search

    Search history events by filter. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >  string transactionId,  
       string accountNumber,  
       string relatedAccountNumber,  
       int? eventType,  
       int? status,  
       string performedBy,  
       string performedByService,  
       DateTime? from,  
       DateTime? to)  
2. GET: /history/transaction  

    Search history events by transaction id. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   string transactionId;  
3. GET: /history/account  

    Search history events by account id. **Note: use JWT Token for Authorization(Bearer Token).**

    Params:  
    >   string accountNumber;  

## Migrations, Seeded data and Database Droping after Starting ##

By default all users created with "User" role. You can create admin only with code. By default one Admin is created from code. 

`Username: admin@bank.com`

`Password: Admin123!`

Note, that in the code after every run database is dropped and code creates new database with migrations. You can comment(or remove) it from Program.cs file:

TransactionService and AccountServer line: 

`await InitializeDatabase.DropDatabasesAsync(app.Services);`

AccountServer line: `

`await InitializeDatabase.DropIdentityDatabasesAsync(app.Services);`

HistoryService lines:

`await historyContext.DropCollectionAsync("HistoryEvents");`

`await historyContext.DropDatabaseAsync();`

``

``

   