namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class PayManagerResponseItem
    {
        public int Id { get; set; }
        public string PayType { get; set; }
        public string PayName { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public decimal Balance { get; set; }
        public int LimitStatus { get; set; }
        public decimal BalanceLimitMoney { get; set; }

        /// <summary>
        /// 登录状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 启用状态
        /// </summary>
        public int Status { get; set; }

        public bool IsAutoClose { get; set; }
        public bool IsUse { get; set; }
        public string PassWord { get; set; }
        public string CardNo { get; set; }
        public string IsBusiness { get; set; }
        public string BusinessNo { get; set; }
        public string Account { get; set; }
    }
}
