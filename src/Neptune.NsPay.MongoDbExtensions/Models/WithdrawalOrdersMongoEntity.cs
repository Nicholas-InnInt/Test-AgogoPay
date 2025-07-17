using MongoDB.Entities;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("withdrawalorders")]
    public class WithdrawalOrdersMongoEntity: BaseMongoEntity
    {
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        public virtual string PlatformCode { get; set; }

        public virtual string WithdrawNo { get; set; }

        public virtual WithdrawalOrderStatusEnum OrderStatus { get; set; }

        public virtual decimal OrderMoney { get; set; }

        public virtual decimal Rate { get; set; }

        public virtual decimal FeeMoney { get; set; }

        public virtual string TransactionNo { get; set; }

        //public virtual DateTime TransactionTime { get; set; }
        //public long TransactionUnixTime { get; set; }

        public virtual string BenAccountName { get; set; }

        public virtual string BenAccountNo { get; set; }

        public virtual string BenBankName { get; set; }

        public virtual string NotifyUrl { get; set; }

        public virtual string OrderNumber { get; set; }

        public virtual string ReceiptUrl { get; set; }

        public virtual int DeviceId { get; set; }

        public virtual WithdrawalOrderTypeEnum OrderType { get; set; }

        public virtual WithdrawalNotifyStatusEnum NotifyStatus { get; set; }

        public virtual int NotifyNumber { get; set; }

        public virtual string Description { get; set; }

        public virtual string Remark { get; set; }
        public virtual long CreatorUserId { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual long LastModifierUserId { get; set;}
        public virtual bool IsDeleted { get; set; }
        public virtual long DeleterUserId { get; set; }
        public virtual DateTime DeletionTime { get; set; }
        public virtual string DeviceLog { get; set; }

        public virtual string ProofContent { get; set; }
        public virtual string ContentMIMEType { get; set; }
        public virtual Guid? BinaryContentId { get; set; }

        public virtual bool IsManualPayout { get; set; }

        public virtual string ReleasedBy { get; set; }

        public virtual DateTime ReleasedDate { get; set; }

        public virtual WithdrawalReleaseStatusEnum ReleaseStatus { get; set; }// null meaning no need to relase , true mean already releases , false mean still onhold the locked balance

        public virtual bool isBilled { get; set; }// Indicate already generated merchantbills record

    }
}
