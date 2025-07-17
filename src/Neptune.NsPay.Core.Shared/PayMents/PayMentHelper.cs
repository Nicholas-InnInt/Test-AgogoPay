using System.Collections.Generic;

namespace Neptune.NsPay.PayMents
{
    public static class PayMentHelper
    {
        public static readonly List<(PayMentTypeEnum PaymentTypeEnum, int Version)> GetBankList = new List<(PayMentTypeEnum PaymentTypeEnum, int Version)>
        {
            (PayMentTypeEnum.MBBank, 1),
            (PayMentTypeEnum.ACBBank, 1),
            (PayMentTypeEnum.VietcomBank, 2),
            (PayMentTypeEnum.VietinBank, 2),
            (PayMentTypeEnum.BidvBank, 2),
            (PayMentTypeEnum.TechcomBank, 2),
            (PayMentTypeEnum.PVcomBank, 2),
            (PayMentTypeEnum.MsbBank, 2),
            (PayMentTypeEnum.SeaBank, 2),
            (PayMentTypeEnum.NamaBank, 2),
            (PayMentTypeEnum.OCBBank, 2),
            (PayMentTypeEnum.EXIMBank, 2),
            (PayMentTypeEnum.HDBank, 2),
            (PayMentTypeEnum.LPBank, 2),
            (PayMentTypeEnum.PGBank, 2),
            (PayMentTypeEnum.VietBank, 2),
        };

        public static readonly List<PayMentTypeEnum> GetDisabledBankList = new List<PayMentTypeEnum>() {
            PayMentTypeEnum.BusinessMbBank,
            PayMentTypeEnum.BusinessTcbBank,
            PayMentTypeEnum.BusinessVtbBank,
            PayMentTypeEnum.BvBank,
            PayMentTypeEnum.TPBank,
            PayMentTypeEnum.VPBBank,
            PayMentTypeEnum.NCBBank,
            PayMentTypeEnum.BacaBank,
        };

        public static readonly PayMentTypeEnum[] GetCryptoList = new[] {
            PayMentTypeEnum.USDT_TRC20,
            PayMentTypeEnum.USDT_ERC20,
        };
    }
}