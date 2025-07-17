namespace Neptune.NsPay.Web.Api.Models.Payment
{
    public class PayBankResponse
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string QrCode { get; set; }
        public string Money { get; set; }
        public int OrderMoney { get; set; }
        public string OrderMark { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string BankName { get; set; }
        public int QrType { get; set; }
        public int SecondsToExpired { get; set; }
        public string PayType { get; set; } // Use to Indentify Bank Logo from PayMentTypeEnum in string
    }
}

///https://localhost:7021/api/payment/sc?orderid=682b38c3cc2608dda1c4f540