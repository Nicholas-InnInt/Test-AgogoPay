using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class BankBalanceService: IBankBalanceService, IDisposable
    {
        public async Task<decimal> GetBalance(int payMentId, decimal balance, decimal balance2, int islogin)
        {
            //默认使用balance
            //检查银行多久没有获取记录，3分钟没有获取，使用web的余额记录
            //如果银行离线，使用web的余额记录

            var tempBalanc = 0M;
            var dateTime = DateTime.Now.AddMinutes(-3);
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.PayMentId, payMentId)
                                & f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(dateTime)));
            if (result.Count > 0)
            {
                if (islogin == 1)
                {
                    tempBalanc = balance;
                }
                else
                {
                    tempBalanc = balance2;
                }
            }
            else
            {
                //判断是否登录
                if (islogin == 1)
                {
                    tempBalanc = balance2;
                }
                else
                {
                    tempBalanc = balance;
                }
            }
            if (tempBalanc == 0)
            {
                if (balance2 == 0)
                {
                    tempBalanc = balance;
                }
                if (balance == 0)
                {
                    tempBalanc = balance2;
                }
            }
            return tempBalanc;
        }

        public void Dispose()
        {
        }
    }
}
