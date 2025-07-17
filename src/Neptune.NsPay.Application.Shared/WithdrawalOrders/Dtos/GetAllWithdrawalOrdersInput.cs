using Abp.Application.Services.Dto;
using Neptune.NsPay.WithdrawalDevices;
using System;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class GetAllWithdrawalOrdersInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public string OrderNoFilter { get; set; }

        public string PlatformCodeFilter { get; set; }

        public string WithdrawNoFilter { get; set; }

        public string TransactionNoFilter { get; set; }

        public int? OrderStatusFilter { get; set; }

        public int? NotifyStatusFilter { get; set; }

        public string BenAccountNameFilter { get; set; }

        public string BenBankNameFilter { get; set; }

        public string BenAccountNoFilter { get; set; }
        public string WithdrawalDevicePhoneFilter { get; set; }
        public int? WithdrawalDeviceBankTypeFilter { get; set; }

        public DateTime? OrderCreationTimeStartDate { get; set; }

        public DateTime? OrderCreationTimeEndDate { get; set; }

        public string UtcTimeFilter { get; set; } = "GMT7+";

        public WithdrawalReleaseStatusEnum? ReleaseStatus { get; set; }

        public decimal? MinMoneyFilter { get; set; }
        public decimal? MaxMoneyFilter { get; set; }

    }
}