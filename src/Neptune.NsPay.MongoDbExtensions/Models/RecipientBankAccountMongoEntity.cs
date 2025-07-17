using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("recipientbankaccounts")]
    public class RecipientBankAccountMongoEntity : BaseMongoEntity
    {

        public int? MerchantId { get; set; } // During Import , user can select this record belong to which merchant or global

        public string HolderName { get; set; }

        public string AccountNumber { get; set; }

        public string BankCode { get; set; } // from  Neptune.NsPay.BankInfo > BankCode

        public string BankKey { get; set; } 

        public int? VerifyDeviceId { get; set; } 
        public int? VerifyPaymentId { get; set; } 
        public string BankName { get; set; }
        public string CreatedBy { get; set; }

    }
}
