using System.Collections.Generic;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders
{
    public class EditWithdrawalOrderDeviceModel 
    {
        public string MerchantName { get; set; }
        public List<WithdrawalOrderModel> WithdrawalOrders { get; set; }

        //for display drop downlist purpose
        public List<WithdrawalDeviceDto> WithdrawalDevice { get; set; }

        //finalize update dto
        public EditWithdrawalOrderDeviceDto EditWithdrawalOrderDeviceDto { get; set; }

    }
    public class WithdrawalOrderModel
    {
        public string WithdrawId { get; set; }
        public string OrderNo { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantName { get; set; }

    }
}