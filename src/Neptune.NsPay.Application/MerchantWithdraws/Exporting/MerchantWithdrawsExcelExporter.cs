using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Neptune.NsPay.DataExporting.Excel.MiniExcel;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Storage;
using Amazon.Runtime.Internal.Transform;

namespace Neptune.NsPay.MerchantWithdraws.Exporting
{
    public class MerchantWithdrawsExcelExporter : MiniExcelExcelExporterBase, IMerchantWithdrawsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public MerchantWithdrawsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetMerchantWithdrawForViewDto> merchantWithdraws)
        {

            var items = new List<Dictionary<string, object>>();
            var headerName = new Dictionary<string, string>()
            {
                {"MerchantCode" ,L("MerchantCode") },
                 {"WithDrawNo" ,L("WithDrawNo") },
                  {"Money" ,L("Money") },
                   {"BankName" ,L("BankName") },
                    {"ReceivCard" ,L("ReceivCard") },
                     {"ReceivName" ,L("ReceivName") },
                      {"Status" ,L("Status") },
                       {"ReviewTime" ,L("ReviewTime") }

            };

            foreach (var merchantWithdraw in merchantWithdraws)
            {
                items.Add(new Dictionary<string, object>()
                    {
                        {headerName["MerchantCode"], merchantWithdraw.MerchantWithdraw.MerchantCode},
                        {headerName["WithDrawNo"], merchantWithdraw.MerchantWithdraw.WithDrawNo},
                        {headerName["Money"], merchantWithdraw.MerchantWithdraw.Money},
                        {headerName["BankName"], merchantWithdraw.MerchantWithdraw.BankName},
                        {headerName["ReceivCard"], merchantWithdraw.MerchantWithdraw.ReceivCard},
                        {headerName["ReceivName"], merchantWithdraw.MerchantWithdraw.ReceivName},
                        {headerName["Status"], merchantWithdraw.MerchantWithdraw.Status},
                        {headerName["ReviewTime"], merchantWithdraw.MerchantWithdraw.ReviewTime},

                    });
            }

            return CreateExcelPackage("MerchantWithdrawsList.xlsx", items);

        }
    }
}