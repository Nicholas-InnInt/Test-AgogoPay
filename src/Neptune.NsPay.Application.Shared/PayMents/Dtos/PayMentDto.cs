using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.PayMents.Dtos
{
    public class PayMentDto : EntityDto
    {
        public string Name { get; set; }

        public PayMentTypeEnum Type { get; set; }

        public PayMentCompanyTypeEnum CompanyType { get; set; }

        public string Gateway { get; set; }
        public string CompanyKey { get; set; }
        public string CompanySecret { get; set; }
        public bool BusinessType { get; set; }

        public string Mail { get; set; }
        public string QrCode { get; set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string PassWord { get; set; }

        public PayMentStatusEnum Status { get; set; }

        public bool PayMentStatus { get; set; }

        public bool UseMoMo { get; set; }
        public PayMentDispensEnum DispenseType { get; set; }
        public string Remark { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; }

        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal LimitMoney { get; set; }
        public decimal BalanceLimitMoney { get; set; }
        public decimal MoMoRate { get; set; }
        public decimal ZaloRate { get; set; }
        public decimal VittelPayRate { get; set; }
        public string CryptoWalletAddress { get; set; }
        public decimal? CryptoMinConversionRate { get; set; }
        public decimal? CryptoMaxConversionRate { get; set; }
    }
}