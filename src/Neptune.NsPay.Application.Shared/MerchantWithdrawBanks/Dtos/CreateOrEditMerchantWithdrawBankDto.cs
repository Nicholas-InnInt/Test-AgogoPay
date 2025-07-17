using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantWithdrawBanks.Dtos
{
    public class CreateOrEditMerchantWithdrawBankDto : EntityDto<int?>
    {

        [StringLength(MerchantWithdrawBankConsts.MaxMerchantCodeLength, MinimumLength = MerchantWithdrawBankConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxBankNameLength, MinimumLength = MerchantWithdrawBankConsts.MinBankNameLength)]
        public string BankName { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxReceivCardLength, MinimumLength = MerchantWithdrawBankConsts.MinReceivCardLength)]
        public string ReceivCard { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxReceivNameLength, MinimumLength = MerchantWithdrawBankConsts.MinReceivNameLength)]
        public string ReceivName { get; set; }

        public bool Status { get; set; }

    }
}