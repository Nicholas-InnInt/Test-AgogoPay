namespace Neptune.NsPay.HttpExtensions.ScratchCard.Models
{
    public class DoithecaoonlineCallbackModel
    {
        public string card_transaction_id { get; set; }
        public string card_code { get; set; }
        public string card_series { get; set; }
        public decimal card_real_amount { get; set; }
        public int card_net_amount { get; set; }
        public int card_status { get; set; }
        public string card_content { get; set; }
        public string card_telecom { get; set; }
    }
}
