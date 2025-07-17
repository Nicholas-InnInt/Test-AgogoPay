using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.StatisticalReports.Dto
{
    public class GetStatisticalReportInput
    {
        public DateTime? OrderCreationTimeStartDate { get; set; }

        public DateTime? OrderCreationTimeEndDate { get; set; }

        public string MerchantCodeFilter { get; set; }
    }
}
