using Neptune.NsPay.WithdrawalDevices.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.SignalRClient
{
    public class WithdrawalDeviceChangedDto
    {
        public List<OldNewDataSet<WithdrawalDeviceDto>> WithdrawalDevice { get; set; }
        public WithdrawalDeviceChangedDto()
        {
            WithdrawalDevice = new List<OldNewDataSet<WithdrawalDeviceDto>>();
        }
    }
}
