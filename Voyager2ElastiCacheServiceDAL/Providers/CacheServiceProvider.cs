using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheServiceDAL.Attributes;
using CacheServiceDAL.Models;

namespace CacheServiceDAL.Providers
{
    public abstract class CacheServiceProvider
    {
        public abstract bool Connect(object obj );
        public abstract bool Disconnect();

        public abstract bool IsConnected { get; }

        public abstract void ClearCache();

        public abstract bool SaveObject<T>(T obj) where T : BasicModel;
        public abstract bool DeleteObjectById<T>(string id) where T : BasicModel;

        public abstract bool SaveKeyValuePair<T>( T value, int expirationTimeInMinutes);
        public abstract bool DeleteObjectByKey<T>();

        public abstract T GetObjectById<T>(string id) where T : BasicModel;
        public abstract T GetObjectByFieldName<T>(string name) where T : BasicModel;
        public abstract T GetObjectByKey<T>(string key);

        public abstract T GetKeyValuePairByKey<T>();

        public abstract IEnumerable<T> GetObjects<T>() where T : BasicModel;
        public abstract bool SaveObjects<T>(IEnumerable<T> list) where T : BasicModel;

        public abstract bool DoesKeyExists<T>() where T : BasicModel;


        public abstract bool SaveUserFeature<T>(T value);

        public abstract bool SaveUserFeature<T>(int userId, T value);
        public abstract bool DeleteUserFeature<T>();
        public abstract bool DeleteUserFeature<T>(int userId);
        public abstract bool DeleteUserCache(int userId);
        public abstract bool DeleteUserFeatureForMultipleUsers<T>(IEnumerable<int> userIds);
        public abstract T GetUserFeature<T>();
        public abstract T GetUserFeature<T>(int userId);
        public abstract bool DoesUserFeatureExists<T>();


        protected string CreateKey<T>()
        {
            CacheKeyAttribute cacheKeyAttribute =
                (CacheKeyAttribute)typeof(T).GetCustomAttributes(typeof(CacheKeyAttribute), false).Single();

            string key = cacheKeyAttribute.Key.Replace('.', '-');
            //string key = typeof(T).FullName.Replace(".", "_");
            return key;
        }

        protected string CreateKey<T>(int id)
        {
            string ids = id.ToString();
            string keyPrefix = typeof(T).FullName.Replace(".", "_") + "_" + ids;
            return keyPrefix;
        }
    }

}
