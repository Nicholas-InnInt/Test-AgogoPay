using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class BankLoginModel
    {
        public string Account { get; set; }
        public DateTime Createtime { get; set; }
        public string Token { get; set; }
        public string Cookies { get; set; }
    }
}
