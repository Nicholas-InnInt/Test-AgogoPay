using System.Collections.Generic;
using Abp;
using Neptune.NsPay.Chat.Dto;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.Chat.Exporting
{
    public interface IChatMessageListExcelExporter
    {
        FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages);
    }
}
