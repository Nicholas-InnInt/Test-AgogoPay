using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public class NsPaySystemSettingKeyConst
    {
        //登录软件白名单
        public readonly static string IpAddress = "NsPaySetting.IpAddress";

        //银行默认请求地址
        public readonly static string BankApi = "NsPaySetting.BankApi";

        //vcb请求地址
        public readonly static string VcbBankApi = "NsPaySetting.VcbBankApi";

        //内部提现商户
        public readonly static string InternalWithdrawMerchant = "NsPaySetting.InternalWithdrawMerchant";

        //平台编码
        public readonly static string PlatformCode = "NsPaySetting.PlatformCode";

        //国家编码
        public readonly static string Countries = "NsPaySetting.Countries";

        //银行自动任务请求地址
        public readonly static string AcbBankJobApi = "NsPaySetting.AcbBankJobApi";
        public readonly static string BidvBankJobApi = "NsPaySetting.BidvBankJobApi";
        public readonly static string TcbBankJobApi = "NsPaySetting.TcbBankJobApi";
        public readonly static string VcbBankJobApi = "NsPaySetting.VcbBankJobApi";
        public readonly static string VtbBankJobApi = "NsPaySetting.VtbBankJobApi";
        public readonly static string MbBankJobApi = "NsPaySetting.MbBankJobApi";
        public readonly static string PvcomBankJobApi = "NsPaySetting.PvcomBankJobApi";
        public readonly static string BusinessMbBankJobApi = "NsPaySetting.BusinessMbBankJobApi";
        public readonly static string BusinessVtbBankJobApi = "NsPaySetting.BusinessVtbBankJobApi";
        public readonly static string BusinessTcbBankJobApi = "NsPaySetting.BusinessTcbBankJobApi";
        public readonly static string TcboutbBankJobApi = "NsPaySetting.TcboutbBankJobApi";

        //tcb银行自动请求地址【根据商户配置】
        public readonly static string MerchantBankJobApi = "NsPaySetting.MerchantBankJobApi";//json格式保存[{"MerchantCode":"","Url":""}]

		//自动回调成功订单时间【分】
		public readonly static string AutoDepositSuccessTime = "NsPaySetting.AutoDepositSuccessTime";
        //更新订单超时时间【分】
		public readonly static string AutoDepositFailTime = "NsPaySetting.AutoDepositFailTime";

        //刮刮卡类型
        public readonly static string ScratchCardType = "NsPaySetting.ScratchCardType";//json保存{"Name":"ss","Value":"bb"}

        public readonly static string NsPayGroupName = "NsPaySetting.NsPayGroupName";

        //SocketApi服务器
        public readonly static string SocketChangIpApi = "NsPaySetting.SocketChangIpApi";

        public readonly static string SocketTcbPort = "NsPaySetting.SocketTcbPort";//json保存{"Name":"ss","Value":"bb"}

        public readonly static string SocketVcbPort = "NsPaySetting.SocketVcbPort";//json保存{"Name":"ss","Value":"bb"}

        public readonly static string OrderBankRemark = "NsPaySetting.OrderBankRemark";//订单备注

        public readonly static string UnableWithdrawBank = "NsPaySetting.UnableWithdrawBank";//无法出款银行

        public readonly static string WithdrawBankRule = "NsPaySetting.WithdrawBankRule";//订单备注
        public readonly static string PayOrderQRgenerate = "NsPaySetting.PayOrderQrgenerate";//入款二维码生成方式


        public readonly static string TelegramPayHelperBotId = "NsPaySetting.TelegramPayHelperBotId";//查单机器人
        public readonly static string TelegramPayHelperChatId = "NsPaySetting.TelegramPayHelperChatId";//查单机器人

        public readonly static string WithdrawDeviceMinBalance = "NsPaySetting.WithdrawDeviceMinBalance";//代付设备最低余额 ， 空值就不会检查

        public readonly static string TelegramMidStopHelperBotId = "NsPaySetting.TelegramMidStopHelperBotId";//查单中转机器人


        #region 商户子账户

        public readonly static string MerchantUserRoles = "NsPaySetting.MerchantUserRoles"; // 商户子账户角色设置

        #endregion


        #region 收款人姓名检测配置

        public readonly static string AccountNameCheckerServerUrl = "NsPaySetting.AccountNameCheckerServerUrl"; // 收款人姓名提取接服务器网址

        public readonly static string AccountNameCheckerMerchants = "NsPaySetting.AccountNameCheckerMerchants"; // 可使用支付组的商户号 ， 可输入多个商户号 , 以逗号隔开

        public readonly static string AccountNameCheckerOnOff = "NsPaySetting.AccountNameCheckerOnOff"; // 开启或关闭收款人姓名检查 , 默认是关闭
        #endregion


       public readonly static string LargeWithdrawalOrderAmount = "NsPaySetting.LargeWithdrawalOrderAmount"; // 大额出款的配置项
       public readonly static string MaxDepositAmount = "NsPaySetting.MaxDepositAmount"; // 最大收款额度
    }
}
