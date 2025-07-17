namespace Neptune.NsPay.Merchants
{
    public class MerchantConsts
    {

        public const int MinNameLength = 3;
        public const int MaxNameLength = 100;

        public const int MinMailLength = 1;
        public const int MaxMailLength = 100;

        public const int MinPhoneLength = 8;
        public const int MaxPhoneLength = 10;
        public const string PhoneRegexPattern = @"^\d+$";

        public const int MinMerchantCodeLength = 1;
        public const int MaxMerchantCodeLength = 50;

        public const int MinMerchantSecretLength = 1;
        public const int MaxMerchantSecretLength = 50;

        public const int MinPlatformCodeLength = 1;
        public const int MaxPlatformCodeLength = 50;

        public const int MinCountryTypeLength = 1;
        public const int MaxCountryTypeLength = 20;

        public const int MinRemarkLength = 1;
        public const int MaxRemarkLength = 2000;

    }
}