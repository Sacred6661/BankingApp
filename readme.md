# BankingApp

## How to run local:

To start all services, local run scripts/run-services-local.bat. After some time all servers will be started and you will see a message in terminal. To stop services run from terminal stop-services-local.bat.  
**Note:** before using script you should run docker-compose, that will start databases and RabbitMQ. Main URL is [https://localhost:7023/](https://localhost:7023/) .

## Endpoints:

**User Identity: (register/login/refresh token):**

1. POST https://localhost:7023/auth/register  
    Data:  
      > public class RegisterRequest  
       {  
           public string Email { get; set; }  
           public string Password { get; set; }  
       }  
2. POST https://localhost:7023/auth/login  
    Data:  
      > public class LoginRequest  
       {  
           public string Email { get; set; }  
           public string Password { get; set; }  
       }  
3. POST https://localhost:7023/auth/refresh-token  
    Data:  
    >   public class RefreshTokenRequest  
       {  
           public string RefreshToken { get; set; }  
       }  
   

**Account:**

1. POST https://localhost:7023/accounts  
    Data:  
    >   public class CreateAccountRequest  
       {  
           public Guid? UserId { get; set; }  
           public decimal InitialBalance { get; set; } \= 0;  
       }  
   **Note:** *If user is not Admin \- balance will be 0, even if you place any other value in InitialBalance .*  
2. GET: https://localhost:7023/accounts/{id}  
    Data:  
    >   string id;  
   **Note**: *Only Admin can access ant account info, user can access info only about his accounts.*  
3. GET: https://localhost:7023/accounts  
    Data:  
    >   bool getAllAccounts \= false  
   **Note**: *Only Admin can access all existed accounts. User even if changed getAllAccounts value to true will get info only about his accounts.*  
   

**Transactions:**

1. POST https://localhost:7023/transactions/deposit  
    Data:  
    >   public class DepositRequest  
       {  
           public string AccountNumber { get; set; }  
           public decimal Amount { get; set; }  
       }  
2. POST https://localhost:7023/transactions/deposit  
    Data:  
    >   public class WithdrawRequest  
       {  
           public string AccountNumber { get; set; }  
           public decimal Amount { get; set; }  
       }  
3. POST https://localhost:7023/transactions/transfer  
     Data:  
    >   public class TransferRequest  
       {  
           public string FromAccountNumber { get; set; }  
           public string ToAccountNumber { get; set; }  
           public decimal Amount { get; set; }      
       }

**History(Admin access only):**

1. GET: https://localhost:7023/history/search  
    Data:  
    >   string transactionId,  
       string accountNumber,  
       string relatedAccountNumber,  
       int? eventType,  
       int? status,  
       string performedBy,  
       string performedByService,  
       DateTime? from,  
       DateTime? to)  
2. GET: https://localhost:7023/history/transaction  
    Data:  
    >   string transactionId;  
3. GET: https://localhost:7023/history/account  
    Data:  
    >   string accountNumber;  
   