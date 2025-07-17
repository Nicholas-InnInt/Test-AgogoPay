using System.Collections.Generic;
using Neptune.NsPay.Auditing.Dto;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.Auditing.Exporting
{
    public interface IAuditLogListExcelExporter
    {
        FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos);

        FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
    }
}
