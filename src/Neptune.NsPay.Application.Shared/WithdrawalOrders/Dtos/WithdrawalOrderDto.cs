using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class WithdrawalOrderDto : EntityDto<string>
    {
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public string PlatformCode { get; set; }

        public string OrderNo { get; set; }

        public string WithdrawNo { get; set; }

        public string TransactionNo { get; set; }

        public WithdrawalOrderStatusEnum OrderStatus { get; set; }

        public string OrderMoney { get; set; }

        public decimal OrderMoneyDec { get; set; }
        public decimal Rate { get; set; }

        public string FeeMoney { get; set; }

        public int DeviceId { get; set; }

        public WithdrawalNotifyStatusEnum NotifyStatus { get; set; }

        public string BenAccountName { get; set; }

        public string BenBankName { get; set; }

        public string BenAccountNo { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime TransactionTime { get; set; }

        public string ReceiptUrl { get; set; }

        public string Remark { get; set; }

        public bool HaveProof { get; set; }

        public bool IsShowSuccessCallBack { get; set; }

        public bool IsManualPayout { get; set; }
        public WithdrawalReleaseStatusEnum ReleaseStatus { get; set; }  // null meaning no need to relase , true mean already releases , false mean still onhold the locked balance
        public bool IsBilled { get; set; }  // indicate the merchant bills had inserted or not 

    }
}