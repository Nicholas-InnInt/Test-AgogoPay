using Neptune.NsPay.MerchantBills;

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class CreateOrEditMerchantBillDto : EntityDto<string>
    {

        [StringLength(MerchantBillConsts.MaxMerchantCodeLength, MinimumLength = MerchantBillConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        [StringLength(MerchantBillConsts.MaxBillNoLength, MinimumLength = MerchantBillConsts.MinBillNoLength)]
        public string BillNo { get; set; }

        public MerchantBillTypeEnum BillType { get; set; }

        public decimal Money { get; set; }

        public decimal Rate { get; set; }

        public decimal FeeMoney { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        [StringLength(MerchantBillConsts.MaxPlatformCodeLength, MinimumLength = MerchantBillConsts.MinPlatformCodeLength)]
        public string PlatformCode { get; set; }

        public long OrderId { get; set; }

        public long CreationUnixTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string Remark { get; set; }

    }
}