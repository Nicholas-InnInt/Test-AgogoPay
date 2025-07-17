using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public class SignHelper
    {
        public static string GetSignContent(IDictionary<string, string> parameters)
        {
            IDictionary<string, string> dictionary = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> enumerator = dictionary.GetEnumerator();
            StringBuilder stringBuilder = new StringBuilder("");
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, string> keyValuePair = enumerator.Current;
                string key = keyValuePair.Key;
                string value = keyValuePair.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(key).Append("=").Append(value).Append("&");
                }
            }
            return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
        }
    }
}
