using System.Collections.Generic;
using Neptune.NsPay.DataExporting.Excel.MiniExcel;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Storage;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.PayOrders;
using System.Linq;

namespace Neptune.NsPay.WithdrawalOrders.Exporting
{
    public class WithdrawalOrdersExcelExporter : MiniExcelExcelExporterBase, IWithdrawalOrdersExcelExporter
    {

        public WithdrawalOrdersExcelExporter(
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {

        }

        public FileDto ExportToFile(List<GetWithdrawalOrderForViewDto> withdrawalOrders, UserTypeEnum userType)
        {
            var languageList = withdrawalOrders.Select(x => x.WithdrawalOrder.OrderStatus).Distinct().Select(x => "Enum_WithdrawalOrderStatusEnum_" + (int)x);
            languageList.Union(withdrawalOrders.Select(x => x.WithdrawalOrder.NotifyStatus).Distinct().Select(x => "Enum_WithdrawalNotifyStatusEnum_" + (int)x));
            var languageDict = languageList.ToDictionary(x => x, x => L(x));


            if (userType == UserTypeEnum.ExternalMerchant)
            {
                return CreateExcelPackage(
                    "WithdrawalOrders.xlsx",
                    excelPackage =>
                    {
                        var sheet = excelPackage.CreateSheet(L("WithdrawalOrders"));

                        AddHeader(
                            sheet,
                            L("MerchantCode"),
                            L("OrderNo"),
                            L("WithdrawalDevice"),
                            L("TransactionNo"),
                            L("OrderStatus"),
                            L("OrderMoney"),
                            L("FeeMoney"),
                            L("NotifyStatus"),
                            L("BenAccountName"),
                            L("BenBankName"),
                            L("BenAccountNo"),
                            L("TransactionTime"),
                            L("CreationTime")
                            );

                        AddObjects(
                            sheet, withdrawalOrders,
                            _ => _.WithdrawalOrder.MerchantCode,
                            _ => _.WithdrawalOrder?.OrderNo,
                            _ => _.WithdrawalDevice?.Phone,
                            _ => _.WithdrawalOrder.TransactionNo,
                            _ => languageDict.ContainsKey("Enum_WithdrawalOrderStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus) ? languageDict["Enum_WithdrawalOrderStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus] : string.Empty,
                            _ => _.WithdrawalOrder.OrderMoney,
                            _ => _.WithdrawalOrder.FeeMoney,
                            _ => languageDict.ContainsKey("Enum_WithdrawalNotifyStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus) ? languageDict["Enum_WithdrawalNotifyStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus] : string.Empty,
                            _ => _.WithdrawalOrder.BenAccountName,
                            _ => _.WithdrawalOrder.BenBankName,
                            _ => _.WithdrawalOrder.BenAccountNo,
                            _ => _.WithdrawalOrder.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => _.WithdrawalOrder.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                            );

                    });
            }
            else
            {
                return CreateExcelPackage(
                    "WithdrawalOrders.xlsx",
                    excelPackage =>
                    {
                        var sheet = excelPackage.CreateSheet(L("WithdrawalOrders"));

                        AddHeader(
                            sheet,
                            L("MerchantCode"),
                            L("OrderNo"),
                            L("WithdrawalName"),
                            L("WithdrawalDevice"),
                            L("TransactionNo"),
                            L("OrderStatus"),
                            L("OrderMoney"),
                            L("FeeMoney"),
                            L("NotifyStatus"),
                            L("BenAccountName"),
                            L("BenBankName"),
                            L("BenAccountNo"),
                            L("TransactionTime"),
                            L("CreationTime")
                            );

                        AddObjects(
                            sheet, withdrawalOrders,
                            _ => _.WithdrawalOrder.MerchantCode,
                            _ => _.WithdrawalOrder.OrderNo,
                            _ => _.WithdrawalDevice?.Name,
                            _ => _.WithdrawalDevice?.Phone,
                            _ => _.WithdrawalOrder.TransactionNo,
                            _ => languageDict.ContainsKey("Enum_WithdrawalOrderStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus) ? languageDict["Enum_WithdrawalOrderStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus] : string.Empty,
                            _ => _.WithdrawalOrder.OrderMoney,
                            _ => _.WithdrawalOrder.FeeMoney,
                            _ => languageDict.ContainsKey("Enum_WithdrawalNotifyStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus) ? languageDict["Enum_WithdrawalNotifyStatusEnum_" + (int)_.WithdrawalOrder.OrderStatus] : string.Empty,
                            _ => _.WithdrawalOrder.BenAccountName,
                            _ => _.WithdrawalOrder.BenBankName,
                            _ => _.WithdrawalOrder.BenAccountNo,
                            _ => _.WithdrawalOrder.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => _.WithdrawalOrder.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                            );

                    });
            }
        }
    }
}