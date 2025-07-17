using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class CommonHelper
    {
        public static string GetDataString()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmffff");
        }

        private static readonly Dictionary<PayMentTypeEnum, string> _lastRefNoKeyMap = new()
        {
            { PayMentTypeEnum.MBBank, "MBBank:MBBankLastRefNo_" },
            { PayMentTypeEnum.MoMoPay, "MoMoLastRefNo_" },
            { PayMentTypeEnum.VietinBank, "VietinBank:VietinBankLastRefNo_" },
            { PayMentTypeEnum.TPBank, "TPBank:TPBankLastRefNo_" },
            { PayMentTypeEnum.TechcomBank, "TechcomBank:TechcomBankLastRefNo_" },
            { PayMentTypeEnum.VietcomBank, "VietcomBank:VietcomBankLastRefNo_" },
            { PayMentTypeEnum.BidvBank, "BidvBank:BidvBankLastRefNo_" },
            { PayMentTypeEnum.ACBBank, "AcbBank:AcbBankLastRefNo_" },
            { PayMentTypeEnum.PVcomBank, "PVcomBank:PVcomBankLastRefNo_" },
            { PayMentTypeEnum.BusinessMbBank, "BusinessMbBank:BusinessMbBankLastRefNo_" },
            { PayMentTypeEnum.BusinessTcbBank, "BusinessTechcom:BusinessTechcomLastRefNo_" },
            { PayMentTypeEnum.BusinessVtbBank, "BusinessVtb:BusinessVtbLastRefNo_" },
            { PayMentTypeEnum.MsbBank, "MsbBank:MsbBankLastRefNo_" },
            { PayMentTypeEnum.SeaBank, "SeaBank:SeaBankLastRefNo_" },
            { PayMentTypeEnum.BvBank, "BvBank:BvBankLastRefNo_" },
            { PayMentTypeEnum.NamaBank, "NamaBank:NamaBankLastRefNo_" },
            { PayMentTypeEnum.VPBBank, "VpbBank:VpbBankLastRefNo_" },
            { PayMentTypeEnum.OCBBank, "OcbBank:OcbBankLastRefNo_" },
            { PayMentTypeEnum.EXIMBank, "EXIMBank:EXIMBankLastRefNo_" },
            { PayMentTypeEnum.NCBBank, "NCBBank:NCBBankLastRefNo_" },
            { PayMentTypeEnum.HDBank, "HDBank:HDBankLastRefNo_" },
            { PayMentTypeEnum.LPBank, "LPBank:LPBankLastRefNo_" },
            { PayMentTypeEnum.PGBank, "PGBank:PGBankLastRefNo_" },
            { PayMentTypeEnum.VietBank, "VietBank:VietBankLastRefNo_" },
            { PayMentTypeEnum.BacaBank, "BacaBank:BacaBankLastRefNo_" },
        };

        private static readonly Dictionary<PayMentTypeEnum, string> _cacheBankKeyMap = new()
        {
            { PayMentTypeEnum.MBBank, "MBBank:MBBank_" },
            { PayMentTypeEnum.MoMoPay, "MoMoExtra_" },
            { PayMentTypeEnum.VietinBank, "VietinBank:VietinBank_" },
            { PayMentTypeEnum.TPBank, "TPBank:TPBank_" },
            { PayMentTypeEnum.TechcomBank, "TechcomBank:TechcomBank_" },
            { PayMentTypeEnum.VietcomBank, "VietcomBank:VietcomBank_" },
            { PayMentTypeEnum.BidvBank, "BidvBank:BidvBank_" },
            { PayMentTypeEnum.ACBBank, "AcbBank:AcbBank_" },
            { PayMentTypeEnum.PVcomBank, "PVcomBank:PVcomBank_" },
            { PayMentTypeEnum.BusinessMbBank, "BusinessMbBank:BusinessMbBank_" },
            { PayMentTypeEnum.BusinessTcbBank, "BusinessTechcom:BusinessTechcom_" },
            { PayMentTypeEnum.BusinessVtbBank, "BusinessVtb:BusinessVtb_" },
            { PayMentTypeEnum.MsbBank, "MsbBank:MsbBank_" },
            { PayMentTypeEnum.SeaBank, "SeaBank:SeaBank_" },
            { PayMentTypeEnum.BvBank, "BvBank:BvBank" },
            { PayMentTypeEnum.NamaBank, "NamaBank:NamaBank_" },
            { PayMentTypeEnum.VPBBank, "VpbBank:VpbBank_" },
            { PayMentTypeEnum.OCBBank, "OcbBank:OcbBank_" },
            { PayMentTypeEnum.EXIMBank, "EXIMBank:EXIMBank_" },
            { PayMentTypeEnum.NCBBank, "NCBBank:NCBBank_" },
            { PayMentTypeEnum.HDBank, "HDBank:HDBank_" },
            { PayMentTypeEnum.LPBank, "LPBank:LPBank_" },
            { PayMentTypeEnum.PGBank, "PGBank:PGBank_" },
            { PayMentTypeEnum.VietBank, "VietBank:VietBank_" },
            { PayMentTypeEnum.BacaBank, "BacaBank:BacaBank_" },
        };

        public static string GetBankLastRefNoKey(PayMentTypeEnum type, string account)
        {
            return _lastRefNoKeyMap.TryGetValue(type, out var prefix) ? prefix + account : null;
        }

        public static string GetBankCacheBankKey(PayMentTypeEnum type, string account)
        {
            return _cacheBankKeyMap.TryGetValue(type, out var prefix) ? prefix + account : null;
        }
    }
}