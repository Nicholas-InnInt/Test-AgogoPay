using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayMents
{
    public enum PayMentTypeEnum
    {
        //默认值
        NoPay = 0,

        //扫码

        [Display(Name = "MB")]
        MBBank = 1,

        //TPBank=2,

        [Display(Name = "ACB")]
        ACBBank = 3,

        [Display(Name = "VCB")]
        VietcomBank = 4,

        [Display(Name = "VTB")]
        VietinBank = 5,

        [Display(Name = "BIDV")]
        BidvBank = 6,

        [Display(Name = "TCB")]
        TechcomBank = 7,

        //eWallet 电子钱包[momo,zalopay]
        MoMoPay = 8,

        ZaloPay = 9,
        ViettelPay = 10,

        //刮刮卡[编号SG]
        ScratchCards = 11,

        [Display(Name = "PVCOM")]
        PVcomBank = 12,

        [Display(Name = "BusinessMB")]
        BusinessMbBank = 13,

        [Display(Name = "BusinessTcb")]
        BusinessTcbBank = 14,

        [Display(Name = "BusinessVtb")]
        BusinessVtbBank = 15,

        [Display(Name = "MSB")]
        MsbBank = 16,

        [Display(Name = "SEA")]
        SeaBank = 17,

        [Display(Name = "BVB")]
        BvBank = 18,

        [Display(Name = "Nama")]
        NamaBank = 19,

        [Display(Name = "TPB")]
        TPBank = 20,

        [Display(Name = "VPB")]
        VPBBank = 21,

        [Display(Name = "OCB")]
        OCBBank = 22,

        [Display(Name = "EXIM")]
        EXIMBank = 23,

        [Display(Name = "NCB")]
        NCBBank = 24,

        [Display(Name = "HDB")]
        HDBank = 25,

        [Display(Name = "LPB")]
        LPBank = 26,

        [Display(Name = "PGB")]
        PGBank = 27,

        [Display(Name = "VIETB")]
        VietBank = 28,

        [Display(Name = "BACAB")]
        BacaBank = 29,

        [Display(Name = "USDT TRC20")]
        USDT_TRC20 = 1001,

        [Display(Name = "USDT ERC20")]
        USDT_ERC20 = 1002,
    }
}