using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MultiTenancy.Accounting.Dto;

namespace Neptune.NsPay.MultiTenancy.Accounting
{
    public interface IInvoiceAppService
    {
        Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

        Task CreateInvoice(CreateInvoiceDto input);
    }
}
