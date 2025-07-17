using Neptune.NsPay.MerchantWithdraws;

using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantWithdraws.Dtos
{
    public class MerchantWithdrawDto : EntityDto<long>
    {
        public string MerchantCode { get; set; }

        public string WithDrawNo { get; set; }

        public string Money { get; set; }

        public string BankName { get; set; }

        public string ReceivCard { get; set; }

        public string ReceivName { get; set; }

        public MerchantWithdrawStatusEnum Status { get; set; }

        public DateTime ReviewTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string ReviewMsg { get; set; }

    }
}