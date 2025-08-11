using AccountService.Data;
using AccountService.Data.Models;
using MassTransit;
using Messaging;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace AccountService.Consumers
{
    public class TransactionCreatedConsumer(AccountDbContext dbContext, ISendEndpointProvider  sendProvider) : IConsumer<TransactionCreated>
    {
        private readonly AccountDbContext _dbContext = dbContext;
        private readonly ISendEndpointProvider _sendProvider = sendProvider;

        public async Task Consume(ConsumeContext<TransactionCreated> context)
        {
            var msg = context.Message;

            var sendEndpointHistory = await _sendProvider.GetSendEndpoint(new Uri("queue:history-account-action-done"));
            var sendEndpointTransaction = await _sendProvider.GetSendEndpoint(new Uri("queue:transaction-account-action-done"));

            var transactionResult = new AccountActionDone()
            {
                PerformedBy = msg?.PerformedBy,
                TransactionType = msg.TransactionType,
                Details = "Transaction is done",
                Amount = msg.Amount,
                AccountNumber = msg.AccountNumber,
                RelatedAccountNumber = msg.RelatedAccountNumber,
                TransactionStatus = msg.TransactionStatus,
                PerformedByService = "AccountService",
                TransactionId = msg.TransactionId
            };

            var isGuid = Guid.TryParse(msg.AccountNumber, out var accountNumber);
            var relatedAccountNumber = new Guid();

            if (!isGuid)
            {
                transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                transactionResult.Details = $"Account id {msg.AccountNumber} is not valid(bad format)";
                transactionResult.IsError = true;

                await sendEndpointHistory.Send(transactionResult);
                await sendEndpointTransaction.Send(transactionResult);
                return;
            }

            if (!string.IsNullOrEmpty(msg?.RelatedAccountNumber))
            {
                isGuid = Guid.TryParse(msg.RelatedAccountNumber, out relatedAccountNumber);

                if (!isGuid)
                {
                    transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                    transactionResult.Details = $"Related account id {msg.RelatedAccountNumber} is not valid(bad format)";
                    transactionResult.IsError = true;

                    await sendEndpointHistory.Send(transactionResult);
                    await sendEndpointTransaction.Send(transactionResult);
                    return;
                }
            }

            var transactionAmout = decimal.Parse(msg.Amount);
            var account = await _dbContext.Accounts.Where(a => a.Id == accountNumber).FirstOrDefaultAsync();
            var relatedAccount = await _dbContext.Accounts.Where(a => a.Id == relatedAccountNumber).FirstOrDefaultAsync();

            if (account == null)
            {
                transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                transactionResult.Details = $"Cannot find account with id {accountNumber} (from account)";
                transactionResult.IsError = true;

                await sendEndpointHistory.Send(transactionResult);
                await sendEndpointTransaction.Send(transactionResult);
                return;
            }

            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                switch (msg.TransactionType)
                {
                    case (int)TransactionTypeEnum.Deposit:
                        account.Balance += transactionAmout;                     

                        transactionResult.TransactionStatus = (int)TransactionStatusEnum.Accepted;
                        transactionResult.Details = $"Deposit to the account {accountNumber}";
                        await sendEndpointHistory.Send(transactionResult);
                        break;
                    case (int)TransactionTypeEnum.Withdraw:
                        if (account.Balance < transactionAmout)
                        {
                            transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                            transactionResult.Details = $"There are not enough balance in the account id {accountNumber}";
                            transactionResult.IsError = true;

                            await sendEndpointHistory.Send(transactionResult);
                            await sendEndpointTransaction.Send(transactionResult);
                            return;
                        }

                        account.Balance -= transactionAmout;                      

                        transactionResult.TransactionStatus = (int)TransactionStatusEnum.Accepted;
                        transactionResult.Details = $"Withdraw from the account {accountNumber}";

                        await sendEndpointHistory.Send(transactionResult);
                        break;
                    case (int)TransactionTypeEnum.Transfer:
                        if (account.Balance < transactionAmout)
                        {
                            transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                            transactionResult.Details = $"There are not enough balance in the account id {accountNumber}";
                            transactionResult.IsError = true;

                            await sendEndpointHistory.Send(transactionResult);
                            await sendEndpointTransaction.Send(transactionResult);
                            return;
                        }

                        if (relatedAccount == null)
                        {
                            transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                            transactionResult.Details = $"Cannot find account with id {relatedAccountNumber} (to account)";
                            transactionResult.IsError = true;

                            await sendEndpointHistory.Send(transactionResult);
                            await sendEndpointTransaction.Send(transactionResult);
                            return;
                        }

                        account.Balance -= transactionAmout;
                        
                        transactionResult.TransactionStatus = (int)TransactionStatusEnum.Accepted;
                        transactionResult.TransactionType = (int)TransactionTypeEnum.Withdraw;
                        transactionResult.Details = $"Withdraw from the account {accountNumber}";
                        await sendEndpointHistory.Send(transactionResult);

                        relatedAccount.Balance += transactionAmout;

                        // we add RelatedAccountNumber as AccountNumber to have in history separate record about
                        // changes in the account balance
                        transactionResult.TransactionStatus = (int)TransactionStatusEnum.Accepted;
                        transactionResult.TransactionType = (int)TransactionTypeEnum.Deposit;
                        transactionResult.AccountNumber = relatedAccount.Id.ToString();
                        transactionResult.RelatedAccountNumber = "";
                        transactionResult.Details = $"Deposit to the account {accountNumber}";
                        await sendEndpointHistory.Send(transactionResult);
                        break;
                }

                // make changes back, we send all to the History, now change message for Transaction service
                if (msg.TransactionType == (int)TransactionTypeEnum.Transfer)
                {
                    transactionResult.TransactionStatus = (int)TransactionStatusEnum.Accepted;
                    transactionResult.TransactionType = (int)TransactionTypeEnum.Transfer;
                    transactionResult.AccountNumber = msg.AccountNumber;
                    transactionResult.RelatedAccountNumber = msg.RelatedAccountNumber;
                    transactionResult.Details = $"Transfer from accoutn {msg.AccountNumber} to account {msg.RelatedAccountNumber}";
                }

                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // if we have an error while saving to the db - reject and rollback all
                await dbTransaction.RollbackAsync();

                transactionResult.TransactionStatus = (int)TransactionStatusEnum.Rejected;
                transactionResult.Details = $"Error while saving data to the database.";
                await sendEndpointHistory.Send(transactionResult);

                throw new Exception("Error while saving data to the database", ex);
            }

            await sendEndpointTransaction.Send(transactionResult);
        }
    }
}
