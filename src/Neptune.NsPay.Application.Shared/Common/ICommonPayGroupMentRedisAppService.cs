using Neptune.NsPay.PayMents.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Common
{
    public interface ICommonPayGroupMentRedisAppService
    {
        Task AddPayGroupMentRedisValue(int payGroupId);

        Task AddPayGroupMentRedisValueByPayMentId(int paymentId);

        void AddPayMentRedisValue(PayMentDto payMent);
    }
}
