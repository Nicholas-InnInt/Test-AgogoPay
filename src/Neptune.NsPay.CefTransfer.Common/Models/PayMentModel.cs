namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class PayMentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public PayMentTypeEnum Type { get; set; }
        //public PayMentCompanyTypeEnum CompanyType { get; set; }
        public string Gateway { get; set; }
        public string CompanyKey { get; set; }
        public string CompanySecret { get; set; }
        public bool BusinessType { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public string QrCode { get; set; }
        public string PassWord { get; set; }
        public string CardNumber { get; set; }
        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal LimitMoney { get; set; }
        public decimal BalanceLimitMoney { get; set; }
        public bool UseMoMo { get; set; }
        //public PayMentDispensEnum DispenseType { get; set; }
        public string Remark { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; }
        //public PayMentStatusEnum ShowStatus { get; set; }
        public bool UseStatus { get; set; }
    }
}
