using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.PayGroupMents
{
    [Table("PayGroupMents")]
    public class PayGroupMent : FullAuditedEntity
    {

        public virtual int GroupId { get; set; }

        public virtual int PayMentId { get; set; }

        public virtual bool Status { get; set; }

    }
}