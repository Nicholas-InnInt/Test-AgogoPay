using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Entities;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("logrecipientbankaccounts")]
    public class LogRecipientBankAccountsMongoEntity : BaseMongoEntity
    {
        public string MerchantId { get; set; } // During Import , user can select this record belong to which merchant or global

        public string HolderName { get; set; }

        public string AccountNumber { get; set; }

        public string BankCode { get; set; } // from  Neptune.NsPay.BankInfo > BankCode

        public string BankKey { get; set; }

        public int? VerifyDeviceId { get; set; } // From Payout Device

        public int? VerifyPaymentId { get; set; } // From Payment Method Account

        public string BankName { get; set; } // From Payment Method Account
        public string CreatedBy { get; set; }

        public string DeletedBy { get; set; } // From Payment Method Account
        public DateTime DeletedDate { get; set; } // From Payment Method Account
    }
}
