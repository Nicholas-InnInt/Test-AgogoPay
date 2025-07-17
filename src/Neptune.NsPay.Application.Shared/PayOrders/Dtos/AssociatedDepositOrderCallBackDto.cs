using Abp.Application.Services.Dto;
using Neptune.NsPay.PayMents;


namespace Neptune.NsPay.PayOrders.Dtos
{
    public class AssociatedDepositOrderCallBackDto: EntityDto<string>
    {
        public PayMentTypeEnum PayType { get; set; }
        public string OrderNumber { get; set; }
    }
}
