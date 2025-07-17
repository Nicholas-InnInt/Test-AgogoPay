using Neptune.NsPay.HttpExtensions.Bank.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class TechcomBankLoginToken
    {
        public string Account { get;set; }
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
        public int ErrorCount { get; set; } = 0;
    }

    public class BusinessTechcomBankLoginToken
    {
        public string Account { get; set; }
        public string cookie { get; set; }
        public string authorization { get; set; }
        public string arrangementsIds { get; set; }
        public DateTime CreateTime { get; set; }
    }


    public class TechcomBankArrangementsResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string BBAN { get; set; }
    }

    public class TechcomBankTransactionHistoryResponse
    {
        public string id { get; set; }
        public string reference { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public TechcomBankTransactionHistoryAmount transactionAmountCurrency { get; set; }
        public decimal runningBalance { get; set; }
        public TechcomBankTransactionHistoryAdditions additions { get; set; }
        public string creationTime { get; set; }
        public DateTime TransactionTime { get; set; }
    }

    public class TechcomBankTransactionHistoryAmount
    {
        public string amount { get; set; }
    }

    public class TechcomBankTransactionHistoryAdditions
    {
        public string creditBank { get; set; }
        public string creditAcctNo { get; set; }
        public string creditAcctName { get; set; }
        public string debitAcctName { get; set; }
        public string debitBank { get; set; }
        public string debitAcctNo { get; set; }
    }

    #region  出款

    public class TechcomBankCheckCardResponse
    {
        public string accountIdentifier { get; set; }
        public string customerName { get; set; }
    }

    public class TechcomBankPaymentOrderResponse
    {
        public List<TechcomBankPaymentOrderChallenges> challenges { get; set; }
        public TechcomBankPaymentOrderData data { get; set; }
        //public string id { get; set; }
        //public string status { get; set; }
        //public string confirmationId { get; set; }
    }

    public class TechcomBankPaymentOrderChallenges
    {
        public string challengeType { get; set; }
        public string acrValues { get; set; }
        public string scope { get; set; }
    }

    [DataContract]
    public class TechcomBankPaymentOrderData
    {
        [DataMember(Name = "currency")]
        public string currency { get; set; }

        [DataMember(Name = "amount")]
        public string amount { get; set; }

        [DataMember(Name = "paymentAmount")]
        public string paymentAmount { get; set; }

        [DataMember(Name = "payment-order-id")]
        public string paymentorderid { get; set; }

        [DataMember(Name = "counter-party-name")]
        public string counterpartyname { get; set; }

        [DataMember(Name = "counter-party-account")]
        public string counterpartyaccount { get; set; }
    }

    public class TechcomBankAuthOrderResponse
    {
        public List<TechcomBankAuthOrderChallenges> challenges { get; set; }
    }

    public class TechcomBankAuthOrderChallenges
    {
        public string challengeType { get; set; }
        public string actionUrl { get; set; }
        public TechcomBankAuthOrderData data { get; set; }
    }

    public class TechcomBankAuthOrderData
    {
        public TechcomBankAuthOrderTxnData txnData { get; set; }
        public TechcomBankAuthOrderDeviceInformation deviceInformation { get; set; }
    }
    public class TechcomBankAuthOrderTxnData
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string paymentAmount { get; set; }
    }

    public class TechcomBankAuthOrderDeviceInformation
    {
        public string vendor { get; set; }
        public string created { get; set; }
        public string model { get; set; }
    }

    public class TechcomBankTransferRequest
    {
        public string OrderId { get; set; }
        public string ArrangementId { get; set; }
        public string Name { get; set; }
        public string Money { get; set; }
        public string BenAccountNo { get; set; }
        public string BenAccountName { get; set; }
        public string BankName { get; set; }
        public string Remark { get; set; }
    }

    public class TechcomBankTransferResult
    {
        public TransferResultTypeEnums Code { get; set; }
        public string ArrangementId { get; set; }

        public string PayMentsOrderId { get; set; }
    }

    public class TechcomBankPayMentOrdersResponse
    {
        public string id { get; set; }
        public string reasonText { get; set; }
        public string createdBy { get; set; }
        public string createdAt { get; set; }
        public string updatedBy { get; set; }
        public string updatedAt { get; set; }
    }

    #endregion
}
