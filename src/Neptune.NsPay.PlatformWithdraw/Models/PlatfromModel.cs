using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatformWithdraw.Models
{
    public class PlatfromBaseResponse
    {
        public int Code { get; set; }
    }

    #region  登录
    public class PlatfromLoginResponse
    {
        public int Code { get; set; }
        public PlatfromLoginResult Result { get; set; }
    }
    public class PlatfromLoginResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime loginTime { get; set; }
    }

    #endregion

    public class GetVerifyWithdrawReponse
    {
        public int Code { get; set; }
        public GetVerifyWithdrawResult Result { get; set; }
    }
    public class GetVerifyWithdrawResult
    {
        //public long? MinId { get; set; }
        public List<GetVerifyWithdrawData> Data { get; set; }
    }

    public class GetVerifyWithdrawData
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string MemberAccount { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
    }

    public class GetNegotiateConnect
    {
        public string ConnectionToken { get; set; }
        public string ConnectionId { get; set; }
    }

    public class GetWithdrawReponse
    {
        public int Code { get; set; }
        public GetWithdrawResult Result { get; set; }
    }
    public class GetWithdrawResult
    {
        public decimal Amount { get; set; }
        public long Id { get; set; }
        public string State { get; set; }
        public GetWithdrawMember Member { get; set; }
        public GetWithdrawBankAccount BankAccount { get; set; }
    }
    public class GetWithdrawMember
    {
        public string Account { get; set; }
        public GetWithdrawMemberInfo MemberInfo { get; set; }
    }
    public class GetWithdrawMemberInfo
    {
        public string Mobile { get; set; }
        public string Name { get; set; }
    }

    public class GetWithdrawBankAccount
    {
        public string Name { get; set; }
        public string Account { get; set; }
    }

}
