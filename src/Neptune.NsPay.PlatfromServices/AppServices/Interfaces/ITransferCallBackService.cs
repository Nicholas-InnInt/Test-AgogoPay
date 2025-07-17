using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatfromServices.AppServices.Interfaces
{
    public interface ITransferCallBackService
    {
        Task<bool> TransferCallBackPost(string orderId, string remark = "");
    }
}
