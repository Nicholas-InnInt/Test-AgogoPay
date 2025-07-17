using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantWithdraws.Dtos
{
    public class TurndownOrPassMerchantWithdrawDto: EntityDto<long?>
    {
        public string ReviewMsg { get; set; }
    }
}
