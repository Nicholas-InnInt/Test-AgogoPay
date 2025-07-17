using Neptune.NsPay.HttpExtensions.ScratchCard.Models;
using Neptune.NsPay.RedisExtensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.ScratchCard
{
    public interface IScDoiTheCaoHelper
    {
        Task<DoithecaoonlineResponse> AddCard(PayMentRedisModel payMent, ScratchCardRequest scratchCardRequest);
        Task<DoithecaoonlineCheckCardResponse> CheckCard(PayMentRedisModel payMent, ScratchCardRequest scratchCardRequest);
    }
}
