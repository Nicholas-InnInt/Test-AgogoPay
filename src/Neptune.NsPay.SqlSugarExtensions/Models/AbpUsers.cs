using Neptune.NsPay.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Models
{
    [Table("AbpUsers")]
    public class AbpUsers
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Surname { get; set; }
        public UserTypeEnum UserType { get; set; }
        public bool IsDeleted { get; set; }
        public int TenantId { get; set; }
    }
}
