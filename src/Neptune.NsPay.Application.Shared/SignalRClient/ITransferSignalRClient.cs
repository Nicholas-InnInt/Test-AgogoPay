using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SignalRClient
{
    public interface ITransferSignalRClient
    {
        Task<bool> WithdrawalDeviceChanged(WithdrawalDeviceChangedDto deviceChanged);
    }

}
