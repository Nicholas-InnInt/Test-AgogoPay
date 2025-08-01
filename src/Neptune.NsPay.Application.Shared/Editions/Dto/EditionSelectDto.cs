﻿using System.Collections.Generic;
using Neptune.NsPay.MultiTenancy.Payments;

namespace Neptune.NsPay.Editions.Dto
{
    public class EditionSelectDto
    {
        public int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string DisplayName { get; set; }

        public int? ExpiringEditionId { get; set; }

        public decimal? MonthlyPrice { get; set; }

        public decimal? AnnualPrice { get; set; }

        public int? TrialDayCount { get; set; }

        public int? WaitingDayAfterExpire { get; set; }

        public bool IsFree { get; set; }

        public EditionSelectDto()
        {
        }

        public decimal GetPaymentAmount(PaymentPeriodType? paymentPeriodType)
        {
            switch (paymentPeriodType)
            {
                case PaymentPeriodType.Monthly:
                    return MonthlyPrice ?? 0;
                case PaymentPeriodType.Annual:
                    return AnnualPrice ?? 0;
                default:
                    return 0;
            }
        }
    }
}