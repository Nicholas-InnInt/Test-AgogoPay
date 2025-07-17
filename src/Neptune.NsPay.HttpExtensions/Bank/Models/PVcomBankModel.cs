using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class PVcomBankLoginModel
    {
        public string cookie { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }
        public string session_state { get; set; }
        public string scope { get; set; }
        public DateTime CreateTime { get; set; }
        public int ErrorCount { get; set; }
    }

    public class PVcomBankAccountResponse
    {
        public PVcomBankAccountHEADER HEADER { get; set; }
        public PVcomBankAccountREQUESTINPUT REQUESTINPUT { get; set; }
    }
    public class PVcomBankAccountHEADER
    {
        public string MERCHANTID { get; set; }
        public string SESSION_ID { get; set; }
        public string MESSAGEID { get; set; }
        public string HASKEY { get; set; }
        public string SYSTEMID { get; set; }
    }
    public class PVcomBankAccountREQUESTINPUT
    {
        public PVcomBankAccountUSERPROFILE USERPROFILE { get; set; }
    }

    public class PVcomBankAccountUSERPROFILE
    {
        public string USERGROUPTYPE { get; set; }
        public string CIFNUMBER { get; set; }
        public string USERID { get; set; }
    }

    public class PVcomBankTransactionHistoryResponse
    {
        public PVcomBankTransactionHistoryRESULT RESULT { get; set; }
        public PVcomBankTransactionHistoryRESPONSEDATA RESPONSEDATA { get; set; }
    }

    public class PVcomBankTransactionHistoryRESULT
    {
        public string CODE { get; set; }
    }
    public class PVcomBankTransactionHistoryRESPONSEDATA
    {
        public PVcomBankTransactionHistorySTATEMENT STATEMENT { get; set; }
    }

    public class PVcomBankTransactionHistorySTATEMENT
    {
        public List<PVcomBankTransactionHistoryList> LIST_STATEMENT_360 { get; set; }
    }
    public class PVcomBankTransactionHistoryList
    {
        public string CURRENBALANCE { get; set; }
        public string SEQUENNO { get; set; }
        public decimal CREATEAMOUNT { get; set; }
        public decimal DEBITAMOUNT { get; set; }
        public string CONTENT { get; set; }
        public string T24DATE { get; set; }
        public DateTime TransactionTime { get; set; }
    }
}
