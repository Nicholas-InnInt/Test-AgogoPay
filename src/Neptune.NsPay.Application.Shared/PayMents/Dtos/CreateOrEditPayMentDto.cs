using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayMents.Dtos
{
    public class CreateOrEditPayMentDto : EntityDto<int?>
    {

        [StringLength(PayMentConsts.MaxNameLength, MinimumLength = PayMentConsts.MinNameLength)]
        public string Name { get; set; }

        public PayMentTypeEnum Type { get; set; }

        public PayMentCompanyTypeEnum CompanyType { get; set; }

        public string Gateway { get; set; }

        public string CompanyKey { get; set; }

        public string CompanySecret { get; set; }

        public bool BusinessType { get; set; }

        public string FullName { get; set; }

        public string Phone { get; set; }

        public string Mail { get; set; }

        public string QrCode { get; set; }

        public string PassWord { get; set; }

        public string CardNumber { get; set; }

        public decimal MinMoney { get; set; }

        public decimal MaxMoney { get; set; }

        public decimal LimitMoney { get; set; }

        public decimal BalanceLimitMoney { get; set; }

        public bool UseMoMo { get; set; }

        public PayMentDispensEnum DispenseType { get; set; }

        public string Remark { get; set; }

        public int Status { get; set; }
        public decimal MoMoRate { get; set; }
        public decimal ZaloRate { get; set; }
        public decimal VittelPayRate { get; set; }
        public string CryptoWalletAddress { get; set; }
        public decimal? CryptoMinConversionRate { get; set; }
        public decimal? CryptoMaxConversionRate { get; set; }

    }
}