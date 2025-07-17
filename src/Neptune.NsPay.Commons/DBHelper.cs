using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public static  class DBHelper
    {
        private static readonly ThreadLocal<Random> _random = new(() => new Random());
        public static int GetRetryInterval(int attempt)
        {
            var rnd = _random.Value!;
            int baseDelay = (int)Math.Pow(2, attempt) * 50; // e.g., 100ms, 200ms, 400ms
            int jitter = rnd.Next(-baseDelay / 2, baseDelay / 2);
            return Math.Max(50, baseDelay + jitter);
        }
    }
}
