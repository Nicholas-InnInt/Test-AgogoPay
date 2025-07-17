using System.Collections.Generic;

namespace Neptune.NsPay.StatisticalBankReports.Dto
{
    public class GetStatisticalBankReportViewDto
    {
        public List<GetOrderBankDetailViewDto> BankAcbs { get; set; }
        public List<GetOrderBankDetailViewDto> BankBidvs { get; set; }
        public List<GetOrderBankDetailViewDto> BankMbs { get; set; }
        public List<GetOrderBankDetailViewDto> BankTechoms { get; set; }
        public List<GetOrderBankDetailViewDto> BankVietcoms { get; set; }
        public List<GetOrderBankDetailViewDto> BankVietins { get; set; }
        public List<GetOrderBankDetailViewDto> BankSeas { get; set; }
        public List<GetOrderBankDetailViewDto> BankMsbs { get; set; }
        public List<GetOrderBankDetailViewDto> BankBvs { get; set; }
        public List<GetOrderBankDetailViewDto> BankNama { get; set; }
        public List<GetOrderBankDetailViewDto> BankTPs { get; set; }
        public List<GetOrderBankDetailViewDto> BankVPBs { get; set; }
        public List<GetOrderBankDetailViewDto> BankOCBs { get; set; }
        public List<GetOrderBankDetailViewDto> BankEXIMs { get; set; }
        public List<GetOrderBankDetailViewDto> BankNCBs { get; set; }
        public List<GetOrderBankDetailViewDto> BankHDs { get; set; }
        public List<GetOrderBankDetailViewDto> BankLPs { get; set; }
        public List<GetOrderBankDetailViewDto> BankPGs { get; set; }
    }
}
