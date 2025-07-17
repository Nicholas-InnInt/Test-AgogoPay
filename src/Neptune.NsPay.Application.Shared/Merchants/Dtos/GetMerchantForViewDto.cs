using Neptune.NsPay.MerchantFunds.Dtos;
using Neptune.NsPay.MerchantRates.Dtos;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class GetMerchantForViewDto
    {
        public MerchantDto Merchant { get; set; }

        public MerchantRateDto MerchantRate { get; set; }    

        public MerchantFundDto MerchantFund { get; set; }
    }
}