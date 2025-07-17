using Neptune.NsPay.PayMents.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class GetPayOrderForViewDto
    {
        public PayOrderDto PayOrder { get; set; }
        public string MerchantName { get; set; }
        public PayMentDto PayMent { get; set; }
        public List<PayOrderDto> ListofPayOrder { get; set; }
    }
}