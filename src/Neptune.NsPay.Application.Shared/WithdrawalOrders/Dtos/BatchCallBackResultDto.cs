using System.Collections.Generic;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class BatchCallBackResultDto
    {
        public List<string> SuccessOrder { get; set; }
        public List<string> FailedOrder { get; set; }
        public bool IsSuccess { get; set; }
    }
}