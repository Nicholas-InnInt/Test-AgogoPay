﻿using System.Collections.Generic;
using Abp.Runtime.Validation;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class GetUsersToExcelInput : IGetUsersInput, IShouldNormalize
    {
        public string Filter { get; set; }
        
        public List<string> Permissions { get; set; }
        
        public List<string> SelectedColumns { get; set; }
        
        public int? Role { get; set; }
        
        public bool OnlyLockedUsers { get; set; }
        
        public string Sorting { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "Name,Surname";
            }

            Filter = Filter?.Trim();
        }
    }
}