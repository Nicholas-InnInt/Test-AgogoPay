namespace Neptune.NsPay.PayMents
{
    public class PayMentConsts
    {

        public const int MinNameLength = 1;
        public const int MaxNameLength = 200;


        public const int MinGatewayLength = 1;
        public const int MaxGatewayLength = 300;

        public const int MinCompanyKeyLength = 1;
        public const int MaxCompanyKeyLength = 500;

        public const int MinCompanySecretLength = 1;
        public const int MaxCompanySecretLength = 500;

        public const int MinFullNameLength = 1;
        public const int MaxFullNameLength = 200;

        public const int MinPhoneLength = 8;
        public const int MaxPhoneLength = 20;
        public const string PhoneRegexPattern = @"^\d+$";

        public const int MinMailLength = 0;
        public const int MaxMailLength = 100;

		public const int MinQrCodeLength = 1;
        public const int MaxQrCodeLength = 1000;

        public const int MinPassWordLength = 1;
        public const int MaxPassWordLength = 50;

        public const int MinCardNumberLength = 5;
        public const int MaxCardNumberLength = 30;
		public const string CardNumberRegex = @"^\d+$";


        public const int MinMoMoCheckSumLength = 1;
        public const int MaxMoMoCheckSumLength = 500;

        public const int MinMoMoPHashLength = 1;
        public const int MaxMoMoPHashLength = 500;

        public const int MinRemarkLength = 1;
        public const int MaxRemarkLength = 500;

        public const int MaxLoginAccountLength = 50;

    }
}