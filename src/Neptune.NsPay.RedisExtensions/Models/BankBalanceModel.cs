using Neptune.NsPay.PayMents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class BankBalanceModel
    {
        public int PayMentId { get; set; }
        public PayMentTypeEnum Type { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 接口返回的余额，默认用接口返回，如果没有使用软件返回余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 软件接口返回余额
        /// </summary>
        public decimal Balance2 { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
