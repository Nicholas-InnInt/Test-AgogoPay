namespace Neptune.NsPay.Commons
{
    public static class BankNameConst
    {
        public const string ACB = "ACB";
        public const string TCB = "TCB";
        public const string BIDV = "BIDV";
        public const string MB = "MB";
        public const string VCB = "VCB";
        public const string VTB = "VTB";
        public const string PVCOM = "PVCOM";
        public const string BusinessMB = "BusinessMB";
        public const string BusinessTcb = "BusinessTcb";
        public const string BusinessVtb = "BusinessVtb";
        public const string MSB = "MSB";
        public const string SEA = "SEA";
        public const string BVB = "BVBank";
        public const string NAMA = "NamaBank";
        public const string TPBank = "TPBank";
        public const string VPBBank = "VPBBank";
        public const string OCBBank = "OCBBank";
        public const string EXIMBank = "EXIMBank";
        public const string NCBBank = "NCBBank";
        public const string HDBank = "HDBank";
        public const string LPBank = "LPBank";
        public const string PGBank = "PGBank";
        public const string VietBank = "VIETBank";
        public const string BacaBank = "BACABank";

        public static string? GetBankOrderRedisKeyByBankName(string bankName)
        {
            return bankName switch
            {
                ACB => NsPayRedisKeyConst.ACBBankOrder,
                TCB => NsPayRedisKeyConst.TCBBankOrder,
                BIDV => NsPayRedisKeyConst.BIDVBankOrder,
                MB => NsPayRedisKeyConst.MBBankOrder,
                VCB => NsPayRedisKeyConst.VCBBankOrder,
                VTB => NsPayRedisKeyConst.VTBBankOrder,
                PVCOM => NsPayRedisKeyConst.PVcomBankOrder,
                BusinessMB => NsPayRedisKeyConst.BusinessMbBankOrder,
                BusinessTcb => NsPayRedisKeyConst.BusinessTcbBankOrder,
                BusinessVtb => NsPayRedisKeyConst.BusinessVtbBankOrder,
                MSB => NsPayRedisKeyConst.MsbBankOrder,
                SEA => NsPayRedisKeyConst.SeaBankOrder,
                BVB => NsPayRedisKeyConst.BvBankOrder,
                NAMA => NsPayRedisKeyConst.NamaBankOrder,
                TPBank => NsPayRedisKeyConst.TPBankOrder,
                VPBBank => NsPayRedisKeyConst.VPBBankOrder,
                OCBBank => NsPayRedisKeyConst.OCBBankOrder,
                EXIMBank => NsPayRedisKeyConst.EXIMBankOrder,
                NCBBank => NsPayRedisKeyConst.NCBBankOrder,
                HDBank => NsPayRedisKeyConst.HDBankOrder,
                LPBank => NsPayRedisKeyConst.LPBankOrder,
                PGBank => NsPayRedisKeyConst.PGBankOrder,
                VietBank => NsPayRedisKeyConst.VietBankOrder,
                BacaBank => NsPayRedisKeyConst.BacaBankOrder,
                _ => null
            };
        }
    }
}
