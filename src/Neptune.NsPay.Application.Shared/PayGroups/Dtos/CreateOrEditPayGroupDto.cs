using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayGroups.Dtos
{
    public class CreateOrEditPayGroupDto : EntityDto<int?>
    {

        [StringLength(PayGroupConsts.MaxGroupNameLength, MinimumLength = PayGroupConsts.MinGroupNameLength)]
        public string GroupName { get; set; }

        [StringLength(PayGroupConsts.MaxBankApiLength, MinimumLength = PayGroupConsts.MinBankApiLength)]
        public string BankApi { get; set; }

        [StringLength(PayGroupConsts.MaxVietcomApiLength, MinimumLength = PayGroupConsts.MinVietcomApiLength)]
        public string VietcomApi { get; set; }

        public bool Status { get; set; }

    }
}