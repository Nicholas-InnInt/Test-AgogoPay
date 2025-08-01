﻿using System.Threading.Tasks;
using Abp.Dependency;

namespace Neptune.NsPay.MultiTenancy.Accounting
{
    public interface IInvoiceNumberGenerator : ITransientDependency
    {
        Task<string> GetNewInvoiceNumber();
    }
}