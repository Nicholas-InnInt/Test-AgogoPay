namespace Neptune.NsPay.WithdrawalDevices
{
    public class WithdrawalDeviceConsts
    {

        public const int MinNameLength = 1;
        public const int MaxNameLength = 200;

        public const int MinPhoneLength = 1;
        public const int MaxPhoneLength = 20;
        public const string PhonePattern = @"^\d+$";

        public const int MinBankOtpLength = 1;
        public const int MaxBankOtpLength = 20;
        public const string BankOtpPattern = @"^\d+$";

        public const int MinLoginPassWordLength = 1;
        public const int MaxLoginPassWordLength = 20;

        public const int MinCardNameLength = 1;
        public const int MaxCardNameLength = 200;

        public const int MinCardNumberLength = 5;
        public const int MaxCardNumberLength = 30;
        public const string CardPattern = @"^\d+$";


        public const int MinMerchantCodeLength = 1;
        public const int MaxMerchantCodeLength = 30;

    }
}