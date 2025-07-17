using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Neptune.NsPay.DataExporting.Excel.MiniExcel;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.MerchantBills.Exporting
{
    public class MerchantBillsExcelExporter : MiniExcelExcelExporterBase, IMerchantBillsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public MerchantBillsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetMerchantBillForViewDto> merchantBills, int pageNumber)
        {

            return CreateExcelPackage(
                 "MerchantBills_"+pageNumber+".xlsx",
                 excelPackage =>
                 {
                     var sheet = excelPackage.CreateSheet(L("MerchantBills"));

                     AddHeader(
                         sheet,
                         L("MerchantCode"),
                         L("BillNo"),
                         L("BillType"),
                         L("Money"),
                         L("Rate"),
                         L("FeeMoney"),
                         L("BalanceBefore"),
                         L("BalanceAfter"),
                         L("CreationTime"),
                         L("TransactionTime")
                         );

                     AddObjects(
                         sheet, merchantBills,
                         _ => _.MerchantBill.MerchantCode,
                         _ => _.MerchantBill.BillNo,
                         _ => _.MerchantBill.BillType,
                         _ => _.MerchantBill.Money,
                         _ => _.MerchantBill.Rate,
                         _ => _.MerchantBill.FeeMoney,
                         _ => _.MerchantBill.BalanceBefore,
                         _ => _.MerchantBill.BalanceAfter,
                         _ => _.MerchantBill.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                         _ => _.MerchantBill.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss")

                         );
                     //sheet.AutoSizeColumn(9);
                 });
        }
    }
}