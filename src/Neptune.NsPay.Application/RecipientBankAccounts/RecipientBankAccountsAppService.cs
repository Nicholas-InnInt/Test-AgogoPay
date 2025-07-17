using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Common;
using Neptune.NsPay.Localization;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.RecipientBankAccounts.Dtos;
using Abp.Collections.Extensions;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Abp.BackgroundJobs;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.MerchantBills.Exporting;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.PayOrders.Exporting;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Merchants.Dtos;
using Abp.Domain.Repositories;
using Twilio.Rest.Trunking.V1;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using MongoDB.Bson;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MerchantRates;
using Abp.Json;
using Neptune.NsPay.Dto;
using CsvHelper;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Abp.UI;
using CsvHelper.Configuration;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.BankInfo;
using Org.BouncyCastle.Asn1.X9;
using Neptune.NsPay.AbpUserMerchants.Dtos;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using NPOI.SS.UserModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Neptune.NsPay.Authorization.Users;
using Abp.Localization;
using PayPalCheckoutSdk.Payments;
using Abp.Extensions;

namespace Neptune.NsPay.RecipientBankAccounts
{
    [AbpAuthorize(AppPermissions.Pages_RecipientBankAccounts)]
    public class RecipientBankAccountsAppService : NsPayAppServiceBase, IRecipientBankAccountsAppService
    {
        private readonly IRecipientBankAccountMongoService _recipientBankAccountMongoService;
        private readonly ILogRecipientBankAccountMongoService _recipientlogBankAccountMongoService;
        private readonly IRepository<Neptune.NsPay.Merchants.Merchant> _merchantRepository;
        public RecipientBankAccountsAppService(IRecipientBankAccountMongoService recipientBankAccountMongoService, ILogRecipientBankAccountMongoService recipientlogBankAccountMongoService, IRepository<Neptune.NsPay.Merchants.Merchant> merchantRepository)
        {
            _recipientBankAccountMongoService = recipientBankAccountMongoService;
            _recipientlogBankAccountMongoService = recipientlogBankAccountMongoService;
            _merchantRepository = merchantRepository;
        }

        public virtual async Task<PagedResultDto<GetRecipientBankAccountsDtos>> GetAll(GetAllRecipientBankAccountsInput input)
        {
            var user = await GetCurrentUserAsync();


            var filteredRecipientBankAccountsDtos = await _recipientBankAccountMongoService.GetAllWithPagination(input);

            int count = filteredRecipientBankAccountsDtos.TotalCount;

            if (filteredRecipientBankAccountsDtos != null)
            {
                var results = new List<GetRecipientBankAccountsDtos>();

                var merchants = user.Merchants.Select(r => new GetRecipientBankAccountMerchantViewDto
                {
                    MerchantCode = r.MerchantCode,
                    MerchantName = r.Name,
                    MerchantId = r.Id
                }).ToList();


                results.AddRange(filteredRecipientBankAccountsDtos.Items.Select(o => new GetRecipientBankAccountsDtos()
                {
                    RecipientBankAccounts = new RecipientBankAccountsDtos
                    {
                        Id = o.ID,
                        MerchantId = o.MerchantId,
                        HolderName = o.HolderName,
                        AccountNumber = o.AccountNumber,
                        BankCode = o.BankCode,
                        BankKey = o.BankKey,
                        VerifyDeviceId = o.VerifyDeviceId,
                        VerifyPaymentId = o.VerifyPaymentId,
                        CreatedBy = o.CreatedBy,
                        MerchantCode = o.MerchantId != null
                                       ? merchants.FirstOrDefault(m => m.MerchantId == o.MerchantId)?.MerchantCode
                                       : null
                    }
                }));

                return new PagedResultDto<GetRecipientBankAccountsDtos>(
                    count,
                    results
                );
            }
            return new PagedResultDto<GetRecipientBankAccountsDtos>(0, new List<GetRecipientBankAccountsDtos>());
        }

        public virtual async Task<GetRecipientBankAccountForEditOutput> GetRecipientBankAccountById(EntityDto<string> input)
        {
            var recipientBankAccount = await _recipientBankAccountMongoService.GetById(input.Id);

            var output = new GetRecipientBankAccountForEditOutput { RecipientBankAccount = ObjectMapper.Map<CreateOrEditRecipientBankAccountsDto>(recipientBankAccount) };

            return output;
        }


        public async Task<ImportResponseDto> CreateOrEdit(CreateOrEditRecipientBankAccountsDto input)
        {
            if (input.Id.IsNullOrEmpty())
            {
                var result = await Create(input);

                return result;
            }
            else
            {
                var result = await Update(input);

                return result;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_RecipientBankAccounts_Create)]
        protected virtual async Task<ImportResponseDto> Create(CreateOrEditRecipientBankAccountsDto input)
        {
            var user = await GetCurrentUserAsync();
            var recipientBankAccounts = ObjectMapper.Map<RecipientBankAccountMongoEntity>(input);
            recipientBankAccounts.BankCode = BankApp.BanksObject[input.BankName].code;

            var existingAccount = await _recipientBankAccountMongoService.GetAsync(x =>
              (x.AccountNumber == input.AccountNumber &&
               x.HolderName == input.HolderName &&
               x.BankName == recipientBankAccounts.BankCode)
              ||
              (x.MerchantId != null && x.MerchantId == input.MerchantId &&
               x.AccountNumber == input.AccountNumber &&
               x.HolderName == input.HolderName &&
               x.BankName == recipientBankAccounts.BankCode)
              ||
              (x.MerchantId == null && input.MerchantId == null &&
               x.AccountNumber == input.AccountNumber &&
               x.HolderName == input.HolderName &&
               x.BankName == recipientBankAccounts.BankCode)
          );

            if (existingAccount != null)
            {
                Neptune.NsPay.Merchants.Merchant result = null; 

                if (input.MerchantId != null)
                {
                    result = await _merchantRepository.GetAsync(input.MerchantId.Value);
                }

                return new ImportResponseDto
                {
                    Success = false,
                    Message = @L("RecipientBankDuplicate"),
                    Duplicates = new List<string> {
                    $"{@L("AccountNumber")}: {existingAccount.AccountNumber}",
                    $"{@L("Holdername")}: {existingAccount.HolderName}",
                    $"{@L("BankName")}: {existingAccount.BankName ?? "N/A"}",
                    $"{@L("MerchantCode")}: {result?.MerchantCode?.ToString() ?? "N/A"}" // Use null check
                    }
                };

            }

            recipientBankAccounts.BankKey = input.BankName;  //BankKey will directy store as input in create and update
            recipientBankAccounts.CreatedBy = user.UserName;
            recipientBankAccounts.BankName = recipientBankAccounts.BankCode.ToUpper();
            recipientBankAccounts.CreationTime = DateTime.Now;
            recipientBankAccounts.CreationUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);

            await _recipientBankAccountMongoService.AddAsync(recipientBankAccounts);

            return new ImportResponseDto
            {
                Success = true,
                Message = @L("SavedSuccessfully")
            };
        }


        [AbpAuthorize(AppPermissions.Pages_RecipientBankAccounts_Edit)]
        public async Task<ImportResponseDto> Update(CreateOrEditRecipientBankAccountsDto input)
        {
            var user = await GetCurrentUserAsync();
            var recipientBankAccount = await _recipientBankAccountMongoService.GetById(input.Id);
            if (recipientBankAccount == null)
            {
                throw new Exception("Recipient bank account not found.");
            }
            // Handle no action
            if (recipientBankAccount.AccountNumber == input.AccountNumber && recipientBankAccount.HolderName == input.HolderName && recipientBankAccount.BankKey == input.BankName)
            {
                return new ImportResponseDto
                {
                    Success = true
                };
            }

            // Check for duplicates (excluding the current record)
            var isDuplicate = await _recipientBankAccountMongoService.ExistsAsync(x =>
                x.ID != input.Id &&
                x.HolderName == (input.HolderName ?? recipientBankAccount.HolderName) &&
                x.MerchantId == (input.MerchantId ?? recipientBankAccount.MerchantId) &&
                x.AccountNumber == (input.AccountNumber ?? recipientBankAccount.AccountNumber) &&
                x.BankCode == (input.BankName.IsNullOrWhiteSpace()? BankApp.BanksObject[input.BankName].code : recipientBankAccount.BankCode)
            );

            if (isDuplicate)
            {
                return new ImportResponseDto
                {
                    Success = false,
                    Message = @L("DuplicateRecordsFound")
                };
            }

            recipientBankAccount.MerchantId = input.MerchantId;
            recipientBankAccount.HolderName = input.HolderName ?? recipientBankAccount.HolderName;
            recipientBankAccount.AccountNumber = input.AccountNumber ?? recipientBankAccount.AccountNumber;
            recipientBankAccount.BankKey =input.BankName;  //BankKey will directy store as input in create and update
            recipientBankAccount.BankCode = BankApp.BanksObject[input.BankName].code;
            recipientBankAccount.BankName = recipientBankAccount.BankCode.ToUpper();

            recipientBankAccount.CreatedBy = user.UserName;
            recipientBankAccount.CreationTime = DateTime.Now;
            recipientBankAccount.CreationUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);


            await _recipientBankAccountMongoService.UpdateAsync(recipientBankAccount);

            return new ImportResponseDto
            {
                Success = true,
                Message = @L("SavedSuccessfully")
            };

        }

        [AbpAuthorize(AppPermissions.Pages_RecipientBankAccounts_Delete)]
        public async Task Delete(string input)
        {
            var user = await GetCurrentUserAsync();
            var recipient = await _recipientBankAccountMongoService.GetById(input);

            if (recipient == null)
            {
                throw new Exception("Recipient bank account not found.");
            }
            var logRecipient = ObjectMapper.Map<LogRecipientBankAccountsMongoEntity>(recipient);

            logRecipient.DeletedBy = user.UserName;
            logRecipient.DeletedDate = DateTime.Now;

            await _recipientlogBankAccountMongoService.AddAsync(logRecipient);

            await _recipientBankAccountMongoService.DeleteAsync(input);

        }

        public async Task<ImportResponseDto> ImportRecipeintBankAccountExcel(ImportRecipientBankAccountExcelDto input)
        {
            if (input.FileBytes == null || input.FileBytes.Length == 0)
            {
                throw new UserFriendlyException("No file uploaded.");
            }

            var user = await GetCurrentUserAsync();
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
            };

            bool isExcelValid = true;
            var merchantDict = new Dictionary<string, int>();
            List<RecipientBankAccountMongoEntity> importList = new List<RecipientBankAccountMongoEntity>();
            List<ImportRecipientBankExcelContentDto> excelContent = new List<ImportRecipientBankExcelContentDto>();
            List<string> duplicateRecords = new List<string>();

            try
            {
                using (var stream = new MemoryStream(input.FileBytes))
                {
                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        excelContent = csv.GetRecords<ImportRecipientBankExcelContentDto>().ToList();
                    }
                }

                if(excelContent.Any(x => x.HolderName.Length > 200))
                {
                    return new ImportResponseDto { Success = false, Message = @L("HolderNameMustNotExceed200Characters") };
                }

                if(excelContent.Any(x => x.AccountNumber.Length > 30))
                {
                    return new ImportResponseDto { Success = false, Message = @L("AccountNumberMustNotExceed30Values") };
                }

                foreach (var record in excelContent)
                {
                    var bankKey = WithdrawalOrderBankMapper.FindBankByName(record.BankName);
                    var mongoEntityItem = new RecipientBankAccountMongoEntity { AccountNumber = record.AccountNumber };

                    if (bankKey.IsNullOrEmpty() || record.HolderName.IsNullOrEmpty() || record.AccountNumber.IsNullOrEmpty())
                    {
                        isExcelValid = false;
                        break;
                    }

                    if (BankApp.BanksObject.ContainsKey(bankKey))
                    {
                        var bankObj = BankApp.BanksObject[bankKey];

                        if (!record.MerchantCode.IsNullOrEmpty())
                        {
                            if (!merchantDict.ContainsKey(record.MerchantCode))
                            {
                                var result = _merchantRepository.GetAll().FirstOrDefault(x => x.MerchantCode == record.MerchantCode);
                                if (result != null)
                                {
                                    merchantDict.Add(record.MerchantCode, result.Id);
                                }
                            }

                            if (merchantDict.ContainsKey(record.MerchantCode))
                            {
                                mongoEntityItem.MerchantId = merchantDict[record.MerchantCode];
                            }
                            else
                            {
                                isExcelValid = false;
                                break;
                            }
                        }

                        mongoEntityItem.BankCode = bankObj.code;
                        mongoEntityItem.BankKey = bankObj.key;
                        mongoEntityItem.HolderName = record.HolderName;
                        mongoEntityItem.CreatedBy = user.UserName;
                        mongoEntityItem.CreationTime = DateTime.Now;
                        mongoEntityItem.BankName =bankObj.code.ToUpper();
                        mongoEntityItem.CreationUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);


                        var existingAccount = await _recipientBankAccountMongoService.GetAsync(x =>
                            (x.AccountNumber == mongoEntityItem.AccountNumber &&
                             x.HolderName == mongoEntityItem.HolderName &&
                             x.BankName == mongoEntityItem.BankName)
                            ||
                            (x.MerchantId != null && x.MerchantId == mongoEntityItem.MerchantId &&
                             x.AccountNumber == mongoEntityItem.AccountNumber &&
                             x.HolderName == mongoEntityItem.HolderName &&
                             x.BankName == mongoEntityItem.BankName)
                            ||
                            (x.MerchantId == null && mongoEntityItem.MerchantId == null &&
                             x.AccountNumber == mongoEntityItem.AccountNumber &&
                             x.HolderName == mongoEntityItem.HolderName &&
                             x.BankName == mongoEntityItem.BankName)
                        );

                        if (existingAccount != null)
                        {
                            duplicateRecords.Add($"{@L("AccountNumber")}: {record.AccountNumber}, {@L("Holdername")}: {record.HolderName}, {@L("BankName")}: {record.BankName}, {@L("MerchantCode")}: {record.MerchantCode}");
                            continue;
                        }

                        if (importList.Any(x => x.AccountNumber == mongoEntityItem.AccountNumber && x.HolderName == mongoEntityItem.HolderName &&
                            x.BankName == record.BankName &&
                           (x.MerchantId == mongoEntityItem.MerchantId || (x.MerchantId == null && mongoEntityItem.MerchantId == null))))
                        {
                            duplicateRecords.Add($"AccountNumber: {record.AccountNumber}, Holder: {record.HolderName}, Bank: {record.BankName}, Merchant: {record.MerchantCode} (Duplicate in Excel)");
                            continue; // Skip adding this duplicate to the import list
                        }

                        importList.Add(mongoEntityItem);
                    }
                    else
                    {
                        isExcelValid = false;
                        break;
                    }
                }

                if (!isExcelValid)
                {
                    return new ImportResponseDto { Success = false, Message = @L("WrongExcelFormat") };
                }

                if (duplicateRecords.Any())
                {
                    return new ImportResponseDto { Success = false, Duplicates = duplicateRecords };
                }

                await _recipientBankAccountMongoService.AddListAsync(importList);

                return new ImportResponseDto { Success = true, Message = $" {@L("SuccessfullyImported")} : {importList.Count} {@L("records")}." };
            }
            catch (Exception ex)
            {
                return new ImportResponseDto { Success = false, Message = $" {@L("ErrorImport")}: {ex.Message}" };
            }
        }


        public async Task<IList<GetRecipientBankAccountMerchantViewDto>> GetOrderMerchants()
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants.Select(r => new GetRecipientBankAccountMerchantViewDto
            {
                MerchantCode = r.MerchantCode,
                MerchantName = r.Name,
                MerchantId = r.Id
            }).ToList();
            return merchants;
        }


        public async Task<IList<GetBankViewDto>> GetBankNames()
        {
            var bankList = BankApp.BankApps
                .Where(bank => !string.IsNullOrEmpty(bank.bank))
                .Select(bank => new GetBankViewDto
                {
                    BankCode = bank.bank,
                    BankName = BankApp.BanksObject.ContainsKey(bank.bank)
                        ? BankApp.BanksObject[bank.bank].key
                        : "Unknown Bank",
                    BankShortName = BankApp.BanksObject.ContainsKey(bank.bank)
                        ? BankApp.BanksObject[bank.bank].shortName
                        : "Unknown Bank"
                })
                .ToList();

            return await Task.FromResult(bankList);
        }


    }
}
