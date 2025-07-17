using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SignalRClient
{
    public interface ISignalRClient
    {

        Task<bool> MerchantPaymentChanged(MerchantPaymentChangedDto merchantPayment);
    }

    
}
