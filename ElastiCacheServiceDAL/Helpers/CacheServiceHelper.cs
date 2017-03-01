using CacheServiceDAL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServiceDAL.Helpers
{
    public static class CacheServiceHelper
    {
        public static CacheServiceProvider GetRedisCacheServiceProvider()
        {
            CacheServiceProvider redisCacheServiceProvider = new RedisCacheServiceProvider();

            return redisCacheServiceProvider;
        }

        public static CacheServiceProvider GetTestCacheServiceProvider()
        {
            CacheServiceProvider testCacheServiceProvider = null;

            return testCacheServiceProvider;
        }
    }
}
