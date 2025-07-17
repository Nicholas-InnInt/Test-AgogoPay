using System.Collections.Generic;
using Neptune.NsPay.DataExporting.Excel.MiniExcel;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Storage;
using System;
using Neptune.NsPay.Authorization.Users;
using System.Linq;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class PayOrdersExcelExporter : MiniExcelExcelExporterBase, IPayOrdersExcelExporter
    {

        public PayOrdersExcelExporter(
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {

        }

        public FileDto ExportToFile(List<GetPayOrderForViewDto> payOrders, int pageNumber, UserTypeEnum userType)
        {

            var languageList = payOrders.Select(x => x.PayOrder.OrderStatus).Distinct().Select(x => "Enum_PayOrderOrderStatusEnum_" + (int)x);
            languageList.Union(payOrders.Select(x => x.PayOrder.ScoreStatus).Distinct().Select(x => "Enum_PayOrderScoreStatusEnum_" + (int)x));
            var languageDict = languageList.ToDictionary(x => x, x => L(x));

            if (userType == UserTypeEnum.ExternalMerchant)
            {
                var datatime = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
                return CreateExcelPackage(
                    "PayOrders-" + datatime + "_" + pageNumber + ".xlsx",
                    excelPackage =>
                    {
                        
                        var sheet = excelPackage.CreateSheet(L("PayOrders"));
                       
                        AddHeader(
                            sheet,
                            L("MerchantCode"),
                            L("OrderNumber"),
                            L("TransactionNo"),
                            L("OrderMoney"),
                            L("FeeMoney"),
                            L("OrderStatus"),
                            L("ScoreStatus"),
                            L("TransactionTime"),
                            L("CreationTime"),
                            L("OrderMark"),
                            L("ScCode"),
                            L("ScSeri"),
                            L("PayTypeStr")
                            );
                    
                        AddObjects(
                            sheet, payOrders,
                            _ => _.PayOrder.MerchantCode,
                            _ => _.PayOrder.OrderNumber,
                            _ => _.PayOrder.TransactionNo,
                            _ => _.PayOrder.OrderMoney,
                            _ => _.PayOrder.FeeMoney,
                            _ => languageDict.ContainsKey("Enum_PayOrderOrderStatusEnum_" + (int)_.PayOrder.OrderStatus) ? languageDict["Enum_PayOrderOrderStatusEnum_" + (int)_.PayOrder.OrderStatus] : string.Empty,
                            _ => languageDict.ContainsKey("Enum_PayOrderScoreStatusEnum_" + (int)_.PayOrder.ScoreStatus) ? languageDict["Enum_PayOrderScoreStatusEnum_" + (int)_.PayOrder.ScoreStatus] : string.Empty,
                            _ => _.PayOrder.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                             _ => _.PayOrder.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => _.PayOrder.OrderMark,
                            _ => _.PayOrder.ScCode,
                            _ => _.PayOrder.ScSeri,
                            _ => _.PayOrder.PayTypeStr
                            );
                     
                    });
            }
            else
            {
                var datatime = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
                return CreateExcelPackage(
                    "PayOrders-" + datatime + "_" + pageNumber + ".xlsx",
                    excelPackage =>
                    {
                        var sheet = excelPackage.CreateSheet(L("PayOrders"));
                        AddHeader(
                            sheet,
                            L("MerchantCode"),
                            L("OrderNumber"),
                            L("TransactionNo"),
                            L("OrderMoney"),
                            L("FeeMoney"),
                            L("OrderStatus"),
                            L("ScoreStatus"),
                            L("TransactionTime"),
                            L("CreationTime"),
                            L("OrderMark"),
                            L("BankPayMentName"),
                            L("CardNumber"),
                            L("BankUserName"),
                            L("PayTypeStr"),
                            L("ScCode"),
                            L("ScSeri")
                            );
       
                        AddObjects(
                            sheet, payOrders,
                            _ => _.PayOrder.MerchantCode,
                            _ => _.PayOrder.OrderNumber,
                            _ => _.PayOrder.TransactionNo,
                            _ => _.PayOrder.OrderMoney,
                            _ => _.PayOrder.FeeMoney,
                            _ => languageDict.ContainsKey("Enum_PayOrderOrderStatusEnum_" + (int)_.PayOrder.OrderStatus) ? languageDict["Enum_PayOrderOrderStatusEnum_" + (int)_.PayOrder.OrderStatus] : string.Empty,
                            _ => languageDict.ContainsKey("Enum_PayOrderScoreStatusEnum_" + (int)_.PayOrder.ScoreStatus)?languageDict["Enum_PayOrderScoreStatusEnum_" + (int)_.PayOrder.ScoreStatus]:string.Empty,
                            _ => _.PayOrder.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                             _ => _.PayOrder.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => _.PayOrder.OrderMark,
                            _ => _.PayMent == null ? "" : _.PayMent.Name,
                            _ => _.PayMent == null ? "" : _.PayMent.CardNumber,
                            _ => _.PayMent == null ? "" : _.PayMent.Phone,
                            _ => _.PayOrder.PayTypeStr,
                            _ => _.PayOrder.ScCode,
                            _ => _.PayOrder.ScSeri
                            );

                    });
            }
        }
    }
}