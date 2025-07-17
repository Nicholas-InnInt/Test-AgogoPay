using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.VtbBankScript.Models
{
    public class GetOrderOtpResponse:BaseResponse
    {
        public GetOrderOtpResponseData Data { get; set; }
    }
    public class GetOrderOtpResponseData
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        public string Phone { get; set; }
        public int BankType { get; set; }
        public string Otp { get; set; }
        public int OrderStatus { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
