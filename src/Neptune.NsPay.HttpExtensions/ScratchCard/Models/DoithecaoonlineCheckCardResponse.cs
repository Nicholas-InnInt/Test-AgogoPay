namespace Neptune.NsPay.HttpExtensions.ScratchCard.Models
{
    public class DoithecaoonlineCheckCardResponse
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public List<DoithecaoonlineCallbackModel> data { get; set; }
    }
}
