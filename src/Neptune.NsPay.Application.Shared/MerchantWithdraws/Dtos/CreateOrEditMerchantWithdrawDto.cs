using Neptune.NsPay.MerchantWithdraws;

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantWithdraws.Dtos
{
    public class CreateOrEditMerchantWithdrawDto : EntityDto<long?>
    {

        [StringLength(MerchantWithdrawConsts.MaxMerchantCodeLength, MinimumLength = MerchantWithdrawConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxWithDrawNoLength, MinimumLength = MerchantWithdrawConsts.MinWithDrawNoLength)]
        public string WithDrawNo { get; set; }

        public decimal Money { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxBankNameLength, MinimumLength = MerchantWithdrawConsts.MinBankNameLength)]
        public string BankName { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxReceivCardLength, MinimumLength = MerchantWithdrawConsts.MinReceivCardLength)]
        public string ReceivCard { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxReceivNameLength, MinimumLength = MerchantWithdrawConsts.MinReceivNameLength)]
        public string ReceivName { get; set; }

        public MerchantWithdrawStatusEnum Status { get; set; }

        public DateTime ReviewTime { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxRemarkLength, MinimumLength = MerchantWithdrawConsts.MinRemarkLength)]
        public string Remark { get; set; }

        public int BankId { get; set; }

    }
}