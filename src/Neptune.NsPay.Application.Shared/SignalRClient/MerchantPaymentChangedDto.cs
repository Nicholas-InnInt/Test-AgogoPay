using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.PayMents.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.SignalRClient
{

    public class MerchantPaymentChangedDto
    {
        public List<OldNewDataSet<PayMentDto>> Payment { get; set; }
        public List<OldNewDataSet<MerchantDto>> Merchant { get; set; }
        public List<OldNewDataSet<PayGroupMentDto>> PayGroupMent { get; set; }

        public MerchantPaymentChangedDto()
        {
            Payment = new List<OldNewDataSet<PayMentDto>>();
            Merchant = new List<OldNewDataSet<MerchantDto>>();
            PayGroupMent = new List<OldNewDataSet<PayGroupMentDto>>();
        }
    }

    public class OldNewDataSet<T>
    {
        public T OldData { get; set; }

        public T NewData { get; set; }

        public OldNewDataSet(T oldData, T newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }


}
