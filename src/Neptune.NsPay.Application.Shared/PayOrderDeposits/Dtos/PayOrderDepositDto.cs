using System;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.PayOrderDeposits.Dtos
{
    public class PayOrderDepositDto : EntityDto<string>
    {
        public string RefNo { get; set; }
        public long BankOrderId { get; set; }
        public PayMentTypeEnum PayType { get; set; }

        /// <summary>
        /// 存款：CRDT 取款：DBIT
        /// </summary>
        public string Type { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 账户信息
        /// </summary>
        public string CreditAmount { get; set; }
        public string DebitAmount { get; set; }
        public string AvailableBalance { get; set; }

        /// <summary>
        /// 存款信息
        /// </summary> 
        public string CreditBank { get; set; }
        public string CreditAcctNo { get; set; }
        public string CreditAcctName { get; set; }

        /// <summary>
        /// 提款信息
        /// </summary>
        public string DebitBank { get; set; }
        public string DebitAcctNo { get; set; }
        public string DebitAcctName { get; set; }

        /// <summary>
        /// 订单信息
        /// </summary>
        public DateTime TransactionTime { get; set; }
        public DateTime CreationTime { get; set; }
        public string OrderId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }


        public string UserName { get; set; }
        public string AccountNo { get; set; }

        public string PayMentName { get; set; }

        public string RejectRemark { get; set; }
        public DateTime OperateTime { get; set; }

        public string OperateUser { get; set; }
        public long UserId { get; set; }

    }
}