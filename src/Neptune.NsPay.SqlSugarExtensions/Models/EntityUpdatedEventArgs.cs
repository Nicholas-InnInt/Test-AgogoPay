using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions
{
    public class EntityUpdatedEventArgs : EventArgs
    {
        public string TableName { get; set; }
        public object Entity { get; set; }
        public string Operation { get; set; } // "Insert", "Update", "Delete"
    }
}
