using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;
using System.Security.Cryptography;

namespace Neptune.NsPay.Web.Api.Helpers
{
    public class OrderHelper
    {
        private const string BASECODE = "0123456789";
        private const string BASECODECHAR = "ABCDEFGHJKLMNPQRSTUVWXYZ";

        /// <summary>
        /// 生成订单编号
        /// </summary>
        /// <returns></returns>
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string GenerateMark(string bankOrderMark, int length = 7)
        {
            //Random ranNum = new Random(Guid.NewGuid().GetHashCode());
            //StringBuilder builder = new StringBuilder();
            //int rnNum = ranNum.Next(BASECODECHAR.Length);
            //builder.Append(BASECODECHAR[rnNum]);
            //rnNum = ranNum.Next(BASECODECHAR.Length);
            //builder.Append(BASECODECHAR[rnNum]);
            //for (int i = 0; i < length - 2; i++)
            //{
            //    Random rnd = new Random(Guid.NewGuid().GetHashCode());
            //    var temprnNum = rnd.Next(BASECODE.Length);
            //    builder.Append(BASECODE[temprnNum]);
            //}
            var guid = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
            var characters = "ABCDEF123456789GHJKLMNPQRSTUVWXYZ123456789";
            List<string> list = new List<string>();
            for (int i = 0; i < length; i++)
            {
                int randomIndex = GetSecureRandomNumber(0, characters.Length);
                string randomPhoneNumber = characters[randomIndex].ToString();
                list.Add(randomPhoneNumber);
            }
            var resultString = string.Join("", list);
            var tempMark = bankOrderMark;
            if (!string.IsNullOrEmpty(bankOrderMark))
            {
                var arr = bankOrderMark.Split(',');
                if (arr.Length > 1)
                {
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var temprnNum = rnd.Next(arr.Length);
                    tempMark = arr[temprnNum];
                }
            }
            return tempMark + resultString.ToString();
        }

        private static int GetSecureRandomNumber(int minValue, int maxValue)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                int value = BitConverter.ToInt32(randomNumber, 0) & int.MaxValue;
                return (int)(value % (maxValue - minValue)) + minValue;
            }
        }

        public static string GetScTransactionid(int merchantId, string orderNo)
        {
            return merchantId + "_" + orderNo;
        }

        public static decimal GetFeeMoney(decimal money, decimal rate)
        {
            return (rate / 100) * money;
        }

        public static decimal GetRate(MerchantRateRedisModel merchant, PayMentTypeEnum paytype)
        {
            if (paytype == PayMentTypeEnum.ScratchCards)
            {
                return merchant.ScratchCardRate;
            }
            else if (paytype == PayMentTypeEnum.MoMoPay)
            {
                return merchant.MoMoRate;
            }
            return merchant.ScanBankRate;
        }

        public static decimal GetTransferRate(MerchantRateRedisModel merchant)
        {
            return 0;
        }
    }
}