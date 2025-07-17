using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.AbpUserMerchants
{
    [Table("AbpUserMerchants")]
    public class AbpUserMerchant : AuditedEntity
    {

        public virtual int MerchantId { get; set; }

        public virtual long UserId { get; set; }

        public virtual int? TenantId { get; set; }

    }
}