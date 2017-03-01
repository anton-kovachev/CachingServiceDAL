using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CacheServiceDAL.Models;

namespace CacheServiceDAL.Providers
{
    /// <summary>
    /// For usage required keys in app.config/web.config file :    <add key="CacheServiceAddress" value="localhost" /> - server address, could be multiple seperated with ','
    ///                                                           <add key="ConnectionPort" value="6379"/>  -> connection port , default 6379
    /// See example at https://github.com/antonk7/CachingServiceDAL
    /// </summary>
    public class CacheWorker
    {

        private CacheServiceProvider cacheProvider;


        /// <summary>
        /// Creates an object to interact with the caching service 
        /// </summary>
        /// <param name="userId">The id of the user that interacts with the caching service</param>
        /// <param name="siteMode">The siteMode in which the user is in</param>
        /// <param name="cacheProvider">The provider of the caching service</param>
        public CacheWorker(int userId, CacheServiceProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
            cacheProvider.Connect(userId);
        }

        /// <summary>
        /// Deletes all of the cached data in the caching service
        /// </summary>
        public void ClearCache()
        {
            cacheProvider.ClearCache();
        }

        /// <summary>
        ///  Saves an object into the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the object to save </typeparam>
        /// <param name="obj">The value of the object to save</param>
        /// <returns>true if the operation was successful,false otherwise</returns>
        public bool SaveObject<T>(T obj) where T : BasicModel
        {
            bool saveOperationResult = cacheProvider.SaveObject<T>(obj);
            return saveOperationResult;
        }


        /// <summary>
        /// Save the "value" parameter as key-value pair , with the type of the "value" parameter as the key , in Redis database with specified expiration time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="expirationTimeInMinutesFromNow">The number of time in minutes, considered from now whe the cache value will expire</param>
        /// <returns>Returns true if the operation had succeeded , false otherwise</returns>
        public bool SaveKeyValuePair<T>(T value, int expirationTimeInMinutesFromNow)
        {
            bool saveOperationResult = cacheProvider.SaveKeyValuePair<T>(value, expirationTimeInMinutesFromNow);
            return saveOperationResult;
        }

        /// <summary>
        /// Removes from cache a resource with key name typeof(T) if exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public bool DeleteKey<T>()
        {
            bool deleteOperationResult = cacheProvider.DeleteObjectByKey<T>();

            return deleteOperationResult;
        }

        /// <summary>
        ///  Deletes an object from the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the object to delete </typeparam>
        /// <param name="obj">The object to delete</param>
        /// <returns>true if the operation was successful,false otherwise</returns>
        public bool DeleteObject<T>(T obj) where T : BasicModel
        {
            bool saveOperationResult = cacheProvider.DeleteObjectById<T>(obj.Id.ToString());
            return saveOperationResult;
        }

        /// <summary>
        /// Saves a collection of objects into the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the objects to save</typeparam>
        /// <param name="objectList">The list of objects to save</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool SaveObjects<T>(IEnumerable<T> objectList) where T : BasicModel
        {
            bool saveOperationResult = cacheProvider.SaveObjects<T>(objectList);
            return saveOperationResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        public T GetObjectById<T>(int id) where T : BasicModel
        {
            T cachedObject = cacheProvider.GetObjectById<T>(id.ToString());
            return cachedObject;
        }

        /// <summary>
        /// Returns an object with the specified type and name from the Redis Database.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="name">The name of the object</param>
        /// <returns></returns>
        public T GetObjectByName<T>(string name) where T : BasicModel
        {
            T cachedObject = cacheProvider.GetObjectByFieldName<T>(name);
            return cachedObject;
        }

        /// <summary>
        /// Returns an object with the specified type and key from the Redis Database.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key in redis cache</param>
        /// <returns>An object for the given params.</returns>
        public T GetObjectByKey<T>(string key)
        {
            T cachedObject = cacheProvider.GetObjectByKey<T>(key);
            return cachedObject;
        }

        /// <summary>
        /// Returns the specified with "T" as  key . key-value pair if exisists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public  T GetKeyValuePairByKey<T>()
        {
            T cachedValue = cacheProvider.GetKeyValuePairByKey<T>();
            return cachedValue;
        }

        /// <summary>
        /// Retrives all objects from the caching service from the specified type
        /// </summary>
        /// <typeparam name="T">The type of the objects</typeparam>
        /// <returns>a collection of the retrieved objecs</returns>
        public IEnumerable<T> GetCachedObjects<T>() where T : BasicModel
        {
            IEnumerable<T> cachedObjects = cacheProvider.GetObjects<T>();
            return cachedObjects;
        }

        public bool DoesKeyExist<T>() where T : BasicModel
        {
            bool checkOperationResult = cacheProvider.DoesKeyExists<T>();
            return checkOperationResult;
        }

        /// <summary>
        /// Save a user claim for the connected user
        /// </summary>
        /// <typeparam name="T">The type of the claim </typeparam>
        /// <param name="val">The value of the claim</param>
        /// <returns>true if the operation was susccessful,false otherwise</returns>
        public bool SaveUserClaim<T>(T val)
        {
            string typeFullName = typeof(T).FullName;

            bool saveOperationResult = cacheProvider.SaveUserFeature<T>(val);

            return saveOperationResult;
        }

        /// <summary>
        /// Save an user claim for the user with the specified id with the specified value
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <param name="userId">The users's id</param>
        /// <param name="val">The value of the claim</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool SaveUserClaim<T>(int userId, T val)
        {
            string typeFullName = typeof(T).FullName;

            bool saveOperationResult = cacheProvider.SaveUserFeature<T>(userId, val);

            return saveOperationResult;
        }


        /// <summary>
        /// Saves an user claim, with value retrived from the Func delegate 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getNewClaimState">The function that returns the value of the claim</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool SaveUserClaim<T>(Func<T> getNewClaimState)
        {
            T val = getNewClaimState();
            bool saveOperationResult = cacheProvider.SaveUserFeature<T>(val);

            return saveOperationResult;
        }

        /// <summary>
        /// Returns the value of a specified claim from the caching service for the connected user
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <returns>the value of the claim if voud, else the default value for the type</returns>
        public T GetUserClaim<T>()
        {
            T userFeature = cacheProvider.GetUserFeature<T>();
            return userFeature;
        }


        /// <summary>
        /// Returns the value of a specified claim for a specified user
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <param name="userId"></param>
        /// <returns>the value of the specified claim for the specified user, if found</returns>
        public T GetUserClaim<T>(int userId)
        {
            T userFeature = cacheProvider.GetUserFeature<T>(userId);
            return userFeature;
        }

        /// <summary>
        /// Returns a specified claim from the caching service for the connected user , with extracted value
        /// </summary>
        /// <typeparam name="T">The claim type</typeparam>
        /// <typeparam name="P">The extracted data type</typeparam>
        /// <param name="parser">The extraction function</param>
        /// <returns>Extracted data by the parser functions from the claim if found</returns>
        public P GetUserClaim<T, P>(Func<T, P> parser)
        {
            T userClaim = cacheProvider.GetUserFeature<T>();
            P parsedUserClaim = parser(userClaim);

            return parsedUserClaim;
        }

        /// <summary>
        /// Deletes a specified claim from the connected user data in the Caching Service
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool DeleteUserClaim<T>()
        {
            bool deleteOperatinResult = cacheProvider.DeleteUserFeature<T>();
            return deleteOperatinResult;
        }

        /// <summary>
        /// Deletes a specified claim for a specified user from the caching serivce
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <param name="userId">The id of the specified user</param>
        /// <returns>true if the opeation was successful, false otherwise</returns>
        public bool DeleteUserClaim<T>(int userId)
        {
            bool deleteOperatinResult = cacheProvider.DeleteUserFeature<T>(userId);
            return deleteOperatinResult;
        }

        /// <summary>
        /// Clear the cahce for a specified user from the caching service
        /// </summary>
        /// <param name="userId">The id of the specified user</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool ClearUserCache(int userId)
        {
            bool deleteOperationResult = cacheProvider.DeleteUserCache(userId);
            return deleteOperationResult;
        }

        /// <summary>
        /// Deletes the claim values from the caching service for the specified users
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <param name="userIds">A collection of user id's for which the operation to be performed</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public bool DeleteUserClaimsForMultipleUsers<T>(IEnumerable<int> userIds)
        {
            bool deleteOperatinResult = cacheProvider.DeleteUserFeatureForMultipleUsers<T>(userIds);
            return deleteOperatinResult;
        }

        /// <summary>
        /// Checks whether the specified claim exists in the caching service for the connected user
        /// </summary>
        /// <typeparam name="T">The type of the claim</typeparam>
        /// <returns>true if the claim exists, false otherwise </returns>
        public bool HasUserClaim<T>()
        {
            bool checkOperationResult = cacheProvider.DoesUserFeatureExists<T>();
            return checkOperationResult;
        }

        public bool Save<T>(IEnumerable<Func<T>> funcs)
        {
            //foreach(Func<T> funct in funcs)
            //{
            //    cacheProvider.SaveUserFeature<
            //}

            return true;
        }

        public bool SaveAccessUserClaim<T, P>(Func<T> getPermissionClaimsState, Func<P> getAccessClaimState)
        {


            return false;
        }

        /// <summary>
        /// Get the claim value for the connected user if found in the caching service, otherwise retrives the claim value from the 
        /// delegate functions , stores the result in the caching service and returns it also
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loadNewClaimState">The function that retrieves the calim value if not found</param>
        /// <returns>the value of the claim</returns>
        public T GetUserClaimOrLoadInCacheIfNotFound<T>(Func<T> loadNewClaimState)
        {
            T claimValue = default(T);

            if (this.HasUserClaim<T>() == false)
            {
                claimValue = loadNewClaimState();
                this.SaveUserClaim<T>(claimValue);
            }
            else
            {
                claimValue = this.GetUserClaim<T>();
                return claimValue;
            }

            return claimValue;
        }

        /// <summary>
        /// Get the claim value for the connected user if found in the caching service, otherwise retrives the claim value from the 
        /// delegate functions , stores the result in the caching service and returns it also
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// /// <param name="userProfileId">The userProfileId of the person you are saving cache for</param>
        /// <param name="loadNewClaimState">The function that retrieves the calim value if not found</param>
        /// <returns>the value of the claim</returns>
        public T LoadInCacheAndGetUserClaimForUser<T>(int userProfileId, Func<T> loadNewClaimState)
        {
            T claimValue = default(T);

            claimValue = loadNewClaimState();

            this.SaveUserClaim<T>(userProfileId, claimValue);

            return claimValue;
        }

        //public P ValidateCache<T,P>( Func<P> getNewState, Func<bool> checkForKeyExistence, Func<bool> cacheNewState)
        //{
        //    if(checkForKeyExistence() == true)
        //    {

        //    }
        //}

        /// <summary>
        /// Locks a Resource, extending the BasicLockModel in the cache.
        /// </summary>
        /// <typeparam name="T">A type extending BasicLockModel</typeparam>
        public void LockResourceInCache<T>() where T : BasicLockModel, new()
        {
            if (!this.DoesKeyExist<T>())
            {
                T resource = new T()
                {
                    Id = 0,
                    Name = String.Empty,
                    CreatedDate = DateTime.UtcNow
                };

                this.SaveObject<T>(resource);
            }
        }

        /// <summary>
        /// Locks a resource like a kye-value pair and sets it's expiration time
        /// If the resource with the same key already exists , the operation overrides it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expirationInMinutes">Expiration of the lock in  minutes</param>
        public void LockResourceInCacheWithExpiration<T>() where T : BasicLockModel, new()
        {
            T resource = new T()
            {
                Id = 0,
                Name = typeof(T).Name,
                CreatedDate = DateTime.UtcNow
            };

            this.SaveKeyValuePair<T>(resource, resource.ExpirationInMinutes);
        }

        /// <summary>
        /// Returns an Action method that when onvoked Locks a resource like a kye-value pair and sets it's expiration time
        /// If the resource with the same key already exists , the operation overrides it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public Action LockResourceInCacheWithExpirationAction<T>() where T : BasicLockModel, new()
        {
            T resource = new T()
            {
                Id = 0,
                Name = typeof(T).Name,
                CreatedDate = DateTime.UtcNow
            };

            return () => { resource.CreatedDate = DateTime.UtcNow; this.SaveKeyValuePair(resource, resource.ExpirationInMinutes); };
        }

        /// <summary>
        /// Unlocks a Resource, extending the BasicLockModel in the cache.
        /// </summary>
        /// <typeparam name="T">A type extending BasicLockModel</typeparam>
        public void UnlockResourceInCache<T>() where T : BasicLockModel
        {
            if (this.DoesKeyExist<T>())
            {
                IEnumerable<T> lockedModels = this.GetCachedObjects<T>();

                foreach (T lockedModel in lockedModels)
                {
                    this.DeleteObject<T>(lockedModel);
                }
            }
        }


        /// <summary>
        /// Removes from cache a resource with key name typeof(T) if exists, that was set there with expiration time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnLockResourceInCacheWithExpiration<T>() where T : BasicLockModel, new()
        {
            this.DeleteKey<T>();
        }

        /// <summary>
        /// Checks if the Resource is locked in cache
        /// </summary>
        /// <typeparam name="T">A type extending BasicLockModel</typeparam>
        /// <param name="expirationHours">The hours after which the locked resource is considered unlocked</param>
        public bool IsResourceLockedInCache<T>(int expirationHours) where T : BasicLockModel
        {
            if (this.DoesKeyExist<T>())
            {
                T lockedModel = this.GetCachedObjects<T>().First();

                if (lockedModel.CreatedDate.AddHours(expirationHours) < DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsResourceLockedInCacheWithExpiration<T>(int expirationHours) where T : BasicLockModel
        {
            if (this.DoesKeyExist<T>())
            {
                T lockedModel = this.GetKeyValuePairByKey<T>();

                if (lockedModel.CreatedDate.AddHours(expirationHours) < DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether or not hte key with value "T" exists in the Cache DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if the key exists , false otherwise</returns>
        public bool IsResourceLockedInCacheWithExpiration<T>() where T : BasicLockModel
        {
            if (this.DoesKeyExist<T>())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void HeartBeatLockForResource<T>(T resource, int expirationInMinutes, int checkInMinutesBeforeExpiration )
        //{
        //    Task.Run(() =>
        //    {

        //        do
        //        {
        //            CacheWorker.LockResourceInCacheWithExpiration<SwitchCompetenciesToNewerVersionLockModel>(expiresInMinutes);
        //            Thread.Sleep((expirationInMinutes - checkInMinutesBeforeExpiration) * 1000);
        //        }
        //        while (response.StatusCode != HttpStatusCode.OK || response.StatusCode != HttpStatusCode.BadRequest);
        //    });

        //}
    }
}
