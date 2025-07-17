using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public static class MerchantPartitioner
    {
        private const ulong FNV_OFFSET = 14695981039346656037UL;
        private const ulong FNV_PRIME = 1099511628211UL;

        /// <summary>返回 0‥(buckets-1) 的分区索引</summary>
        public static int GetPartition(string key, int buckets = 2)
        {
            ulong hash = Fnv1a64(key);
            return JumpConsistentHash(hash, buckets);
        }

        // -------- FNV-1a 64 bit --------
        private static ulong Fnv1a64(string str)
        {
            ulong hash = FNV_OFFSET;
            for (int i = 0; i < str.Length; i++)
            {
                hash ^= str[i];
                hash *= FNV_PRIME;
            }
            return hash;
        }

        // -------- Jump Consistent Hash (Google) --------
        private static int JumpConsistentHash(ulong key, int buckets)
        {
            long b = -1, j = 0;
            while (j < buckets)
            {
                b = j;
                key = key * 2862933555777941757UL + 1;
                j = (int)((b + 1) * (1L << 31) / (double)((key >> 33) + 1));
            }
            return (int)b;
        }
    }
}