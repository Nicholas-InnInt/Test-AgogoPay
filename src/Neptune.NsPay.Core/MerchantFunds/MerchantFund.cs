using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.MerchantFunds
{
    [Table("MerchantFunds")]
    public class MerchantFund : Entity
    {

        [StringLength(MerchantFundConsts.MaxMerchantCodeLength, MinimumLength = MerchantFundConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        //总代收金额
        public virtual decimal DepositAmount { get; set; }

        //商户提现金额
        public virtual decimal WithdrawalAmount { get; set; }

        //总代付金额
        public virtual decimal TranferAmount { get; set; }

        //总手续费
        public virtual decimal RateFeeBalance { get; set; }

        //当前余额
        public virtual decimal Balance { get; set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual DateTime UpdateTime { get; set; }

    }
}