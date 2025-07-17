using System.Collections.Generic;
using Neptune.NsPay.DataExporting.Excel.MiniExcel;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Storage;
using System;

namespace Neptune.NsPay.PayOrderDeposits.Exporting
{
    public class PayOrderDepositsExcelExporter : MiniExcelExcelExporterBase, IPayOrderDepositsExcelExporter
    {
        public PayOrderDepositsExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportToFile(List<GetPayOrderDepositForViewDto> payOrderDeposits, int pageNumber)
        {

            var datatime = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
            return CreateExcelPackage(
                "PayOrderDeposits" + datatime + "_(" + pageNumber + ").xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("PayOrderDeposits"));

                    AddHeader(
                        sheet,
                        L("MerchantCode"),
                        L("BankPayMentName"),
                        L("UserName"),
                        L("AccountNo"),
                        L("PayType"),
                        L("Type"),
                        L("RefNo"),
                        L("CreditAmount"),
                        L("UserMember"),
                        L("OrderNo"),
                        L("OrderNumber"),
                        L("OrderStatus"),
                        L("ScoreStatus"),
                        L("TransactionTime"),
                        L("CreationTime"),
                        L("OperateUser"),
                        L("Description")
                    );

                    AddObjects(
                    sheet, payOrderDeposits,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.MerchantCode) ? "" : _.PayOrderDeposit.MerchantCode,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.PayMentName) ? "" : _.PayOrderDeposit.PayMentName,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.UserName) ? "" : _.PayOrderDeposit.UserName,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.AccountNo) ? "" : _.PayOrderDeposit.AccountNo,
                    _ => _.PayOrderDeposit.PayType,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.Type) ? "" : _.PayOrderDeposit.Type,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.RefNo) ? "" : _.PayOrderDeposit.RefNo,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.CreditAmount) ? "" : _.PayOrderDeposit.CreditAmount,
                    _ => _.PayOrder != null ? _.PayOrder.UserNo : "",
                    _ => _.PayOrder != null ? _.PayOrder.OrderNo : "",
                    _ => _.PayOrder != null ? _.PayOrder.OrderNumber : "",
                    _ => _.PayOrder != null ? _.PayOrder.OrderStatus : GetOrderStatus(_.PayOrderDeposit.OrderId, _.PayOrderDeposit.UserId),
                    _ => _.PayOrder != null ? _.PayOrder.ScoreStatus : _.PayOrderDeposit.OrderId == "-1" ? _.PayOrderDeposit.RejectRemark : GetOrderStatus(_.PayOrderDeposit.OrderId, _.PayOrderDeposit.UserId),
                    _ => _.PayOrderDeposit.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => _.PayOrderDeposit.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.OperateUser) ? "" : _.PayOrderDeposit.OperateUser,
                    _ => string.IsNullOrEmpty(_.PayOrderDeposit.Description) ? "" : _.PayOrderDeposit.Description
                    );

                });
        }

        private string GetOrderStatus(string orderId, long userid)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return L("DepositOrderStatusEnum_2");
            }
            if (orderId == "-1")
            {
                return L("RejectOrder");
            }
            if (orderId != null && orderId != "-1" && userid == 0)
            {
                return L("DepositOrderStatusEnum_1");
            }
            if (orderId != null && orderId != "-1" && userid > 0)
            {
                return L("AssociatedDepositOrder");
            }
            return "";
        }
    }
}