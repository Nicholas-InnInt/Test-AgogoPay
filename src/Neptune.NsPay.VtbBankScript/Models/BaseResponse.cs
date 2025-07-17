using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.VtbBankScript.Models
{
    public class BaseResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
