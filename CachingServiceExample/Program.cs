using CacheServiceDAL.Helpers;
using CacheServiceDAL.Providers;
using CachingServiceExample.ExampleModels;
using CachingServiceExample.ExampleModels.CacheMarkerLockModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingServiceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            List<UserAddressCacheModel> addresses = new List<UserAddressCacheModel>();

            for(int i = 0; i < 100; i++)
            {
                //id - user id
                UserAddressCacheModel address = new UserAddressCacheModel { Id = i, Name = i + " street", Number = i };
                addresses.Add(address);
            }

            int userId = 1;
            CacheWorker cacheWorker = new CacheWorker(userId, CacheServiceHelper.GetRedisCacheServiceProvider());
            cacheWorker.SaveObjects(addresses);

            var address1 = cacheWorker.GetObjectById<UserAddressCacheModel>(11);

            Console.WriteLine("{0} {1} {2}", address1.Id, address1.Name, address1.Number);


            //Lock operation described by model in cache -> creates a cache entry that expires with time set in the model
            cacheWorker.LockResourceInCacheWithExpiration<ValidateDatabaseLockModel>();

            Action lockResource = cacheWorker.LockResourceInCacheWithExpirationAction<DefaultRolePermissionsLockModel>();
            lockResource();
        }
    }
}
