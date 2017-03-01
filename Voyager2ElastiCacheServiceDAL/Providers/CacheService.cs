using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheServiceDAL.Helpers;

namespace CacheServiceDAL.Providers
{
    class CacheService
    {
        private static readonly CacheService cacheServiceInstance = new CacheService();
        private bool isInitialized;
        private CacheServiceProvider provider;

        static CacheService()
        {
            Instance.Initialize();
        }

        public static CacheService Instance
        {
            get
            {
                return cacheServiceInstance;
            }
        }

        public virtual CacheServiceProvider Provider
        {
            get
            {
                return provider;
            }
        }

        private void Initialize()
        {
            if (isInitialized == false)
            {
                provider = CacheServiceHelper.GetRedisCacheServiceProvider();
                isInitialized = true;
            }
        }
    }
}
