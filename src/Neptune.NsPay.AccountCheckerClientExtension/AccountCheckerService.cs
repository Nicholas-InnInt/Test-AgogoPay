using Neptune.NsPay.AccountNameChecker;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Utils;
using Neptune.NsPay.VietQR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Neptune.NsPay.AccountCheckerClientExtension
{
    public class AccountCheckerService
    {
        private readonly IRecipientBankAccountMongoService _recipientBankAccountMongoService;
        private readonly IAccountNameCheckerClient _accountNameCheckerClient;
        public AccountCheckerService(IRecipientBankAccountMongoService recipientBankAccountMongoService , IAccountNameCheckerClient accountNameCheckerClient)
        {

            _recipientBankAccountMongoService = recipientBankAccountMongoService;
            _accountNameCheckerClient = accountNameCheckerClient;
        }

        public async Task<bool?> CheckAccountName(string BankKey , string AccountNumber , string AccountName , int MerchantId)
        {
            // if return null meaning not able to verify
            // if return false meaning name is error
            // if return true meaning name is correct

            bool? isSuccess = null;
            if(!string.IsNullOrEmpty(AccountName)&&BankApp.BanksObject.ContainsKey(BankKey))
            {
                var bankCode = BankApp.BanksObject[BankKey].code;
                string actualName = string.Empty;
                var dbResult = await _recipientBankAccountMongoService.GetByAccountDetails(bankCode, AccountNumber);

                if (dbResult.Count > 0)
                {
                    var merchantRecord = dbResult.FirstOrDefault();

                    if (dbResult.Any(x => x.MerchantId.HasValue && x.MerchantId.Value == MerchantId))
                    {
                        actualName = dbResult.First(x => x.MerchantId.HasValue && x.MerchantId.Value == MerchantId).HolderName;
                    }
                    else if (dbResult.Any(x => !x.MerchantId.HasValue))
                    {
                        actualName = dbResult.First(x => !x.MerchantId.HasValue).HolderName;
                    }
                    else
                    {
                        actualName = dbResult.First().HolderName;
                    }
                }
                else
                {
                    // get from accountChecker
                   var clientResult = await  _accountNameCheckerClient.Get(BankKey, AccountNumber);

                    if(clientResult.success.HasValue&& !string.IsNullOrEmpty( clientResult.holderName))
                    {
                        actualName = clientResult.holderName;
                       await _recipientBankAccountMongoService.AddAsync(new MongoDbExtensions.Models.RecipientBankAccountMongoEntity() { AccountNumber = AccountNumber, BankCode = bankCode, BankKey = BankKey, HolderName = clientResult.holderName , CreatedBy="SYSTEM" , VerifyPaymentId = clientResult.paymentId });
                    }

                }

                if(!string.IsNullOrEmpty(actualName))
                {
                    string regexActualName = Regex.Replace(UtilsHelper.VietnameseToEnglish(actualName).ToUpper(), "[^A-Z]", "");
                    string regexAccountName = Regex.Replace(UtilsHelper.VietnameseToEnglish(AccountName).ToUpper(), "[^A-Z]", "");

                    isSuccess = regexActualName==regexAccountName;
                }
            }

            return isSuccess;
        }
    }
}
