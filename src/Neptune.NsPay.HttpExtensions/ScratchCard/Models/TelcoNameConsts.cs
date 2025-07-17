namespace Neptune.NsPay.HttpExtensions.ScratchCard.Models
{
    public class TelcoNameConsts
    {
        public const string Viettel = "VTT";
        public const string Mobifone = "VMS";
        public const string Vinaphone = "VNP";
        public const string Vietnammobile = "VNM";

        public static List<string> GetTelcoNames()
        {
            List<string> list = new List<string>();
            list.Add(Viettel);
            list.Add(Mobifone);
            list.Add(Vinaphone);
            list.Add(Vietnammobile);
            return list;
        }
    }
}
