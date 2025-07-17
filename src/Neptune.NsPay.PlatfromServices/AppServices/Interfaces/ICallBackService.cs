using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatfromServices.AppServices.Interfaces
{
    public interface ICallBackService
    {
        Task<bool> CallBackPost(string orderId);
    }
}
