using System.Collections.Generic;
using Abp.Dependency;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.DataImporting.Excel;

public interface IExcelInvalidEntityExporter<TEntityDto> : ITransientDependency
{
    FileDto ExportToFile(List<TEntityDto> entities);
}