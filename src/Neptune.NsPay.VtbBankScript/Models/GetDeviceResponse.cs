using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.VtbBankScript.Models
{
    public class GetDeviceResponse: BaseResponse
    {
        public GetDeviceResponseData Data { get; set; }
    }

    public class GetDeviceResponseData
    {
        public int DeviceId { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Otp { get; set; }
        public bool Status { get; set; }
        public int Process { get; set; }
        public string LoginPassWord { get; set; }
    }
}
