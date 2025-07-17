using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.StatisticalBankReports.Dto
{
    public class GetStatisticalBankReportInput
    {
        public DateTime? OrderCreationTimeStartDate { get; set; }

        public DateTime? OrderCreationTimeEndDate { get; set; }

        public string CardNumberFilter { get; set; }
    }
}
