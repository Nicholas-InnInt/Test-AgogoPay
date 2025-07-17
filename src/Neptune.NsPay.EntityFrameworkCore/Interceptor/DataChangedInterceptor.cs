using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.Merchants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neptune.NsPay.Interceptor
{
    public class DataChangedInterceptor : SaveChangesInterceptor
    {
        private readonly IDataChangedEvent _dataChangedEvent;
        public DataChangedInterceptor(IDataChangedEvent dataChangedEvent) { 
            _dataChangedEvent = dataChangedEvent;
            
        }
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken)
        {
            if (eventData.Context is not null)
            {
                _dataChangedEvent.EFCoreChanged(eventData.Context);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
    }