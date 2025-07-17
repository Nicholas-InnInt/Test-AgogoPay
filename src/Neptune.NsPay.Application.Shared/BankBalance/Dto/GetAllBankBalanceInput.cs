using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.BankBalance.Dto
{
    public class GetAllBankBalanceInput
    {
        public string Filter { get; set; }
        public int OrderPayTypeFilter { get; set; }
        public DateTime? MaxTransactionTimeFilter { get; set; }
        public DateTime? MinTransactionTimeFilter { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT8+";
    }
}
