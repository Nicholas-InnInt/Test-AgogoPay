using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Neptune.NsPay.Utils
{
    public static class UtilsHelper
    {
        /// <summary>
        /// Add simple polyfill for TextEncoder
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Byte[] StringToUint8Array(this string content)
        {
            if (typeof(Encoding).IsAssignableFrom(typeof(Encoding)))
            {
                return Encoding.UTF8.GetBytes(content);
            }

            // Polyfill for TextEncoder
            // This is a simplified implementation of the TextEncoder API
            // Source: https://gist.github.com/Yaffle/5458286
            List<byte> octets = new List<byte>();
            int length = content.Length;
            int i = 0;
            while (i < length)
            {
                int codePoint = char.ConvertToUtf32(content, i);
                int c = 0;
                int bits = 0;
                if (codePoint <= 0x0000007F)
                {
                    c = 0;
                    bits = 0x00;
                }
                else if (codePoint <= 0x000007FF)
                {
                    c = 6;
                    bits = 0xC0;
                }
                else if (codePoint <= 0x0000FFFF)
                {
                    c = 12;
                    bits = 0xE0;
                }
                else if (codePoint <= 0x001FFFFF)
                {
                    c = 18;
                    bits = 0xF0;
                }
                octets.Add((byte)(bits | (codePoint >> c)));
                c -= 6;
                while (c >= 0)
                {
                    octets.Add((byte)(0x80 | ((codePoint >> c) & 0x3F)));
                    c -= 6;
                }
                i += (codePoint >= 0x10000) ? 2 : 1;
            }
            return octets.ToArray();
        }

        public static string VietnameseToEnglish(this string vnText)
        {
            string result = vnText.RemoveDiacritics();
            result = Regex.Replace(result, "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            result = Regex.Replace(result, "[éèẻẽẹêếềểễệ]", "e");
            result = Regex.Replace(result, "[íìỉĩị]", "i");
            result = Regex.Replace(result, "[óòỏõọôốồổỗộơớờởỡợ]", "o");
            result = Regex.Replace(result, "[úùủũụưứừửữự]", "u");
            result = Regex.Replace(result, "[ýỳỷỹỵ]", "y");
            result = Regex.Replace(result, "[đ]", "d");
            result = Regex.Replace(result, "[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]", "A");
            result = Regex.Replace(result, "[ÉÈẺẼẸÊẾỀỂỄỆ]", "E");
            result = Regex.Replace(result, "[ÍÌỈĨỊ]", "I");
            result = Regex.Replace(result, "[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]", "O");
            result = Regex.Replace(result, "[ÚÙỦŨỤƯỨỪỬỮỰ]", "U");
            result = Regex.Replace(result, "[ÝỲỶỸỴ]", "Y");
            result = Regex.Replace(result, "[Đ]", "D");
            return result;
        }

        private static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static decimal GetRandomDecimal(decimal min, decimal max)
        {
            if (min > max)
                throw new ArgumentException("min must be less than or equal to max.");

            Random random = new Random();
            double range = (double)(max - min);
            double sample = random.NextDouble();
            decimal scaled = (decimal)(sample * range) + min;

            return scaled;
        }
    }
}