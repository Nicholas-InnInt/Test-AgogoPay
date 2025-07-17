using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class MerchantBillDto : EntityDto<string>
    {
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public string BillNo { get; set; }

        public MerchantBillTypeEnum BillType { get; set; }

        public string Money { get; set; }

        public decimal Rate { get; set; }

        public decimal FeeMoney { get; set; }

        public string BalanceBefore { get; set; }

        public string BalanceAfter { get; set; }

        public string PlatformCode { get; set; }

        public long OrderId { get; set; }

        public long CreationUnixTime { get; set; }

        public DateTime CreationTime { get; set; }

        public long TransactionUnixTime { get; set; }

        public DateTime TransactionTime { get; set; }

        public string Remark { get; set; }

    }
}