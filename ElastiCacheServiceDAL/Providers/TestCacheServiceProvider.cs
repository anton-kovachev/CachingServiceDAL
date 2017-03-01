using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServiceDAL.Providers
{
    public class TestCacheServiceProvider : CacheServiceProvider
    {
        private Dictionary<string, object> cacheDictionary;
        private int userId;

        public TestCacheServiceProvider()
        {

        }

        /// <summary>
        /// Fake method for obtaining caching service connection
        /// </summary>
        /// <param name="obj">The id of the user that connects</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public override bool Connect(object obj)
        {
            if (cacheDictionary == null)
            {
                cacheDictionary = new Dictionary<string, object>();
                userId = Int32.Parse(obj.ToString());
            }

            return cacheDictionary != null;
        }

        /// <summary>
        /// Fake method for closing the caching service connection
        /// </summary>
        /// <returns></returns>
        public override bool Disconnect()
        {
            cacheDictionary = null;
            userId = 0;

            return true;
        }

        /// <summary>
        /// A property that check whether a cacheDictionary instance is present
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return cacheDictionary != null;
            }
        }

        /// <summary>
        /// Remove all keys from the cache dictionary
        /// </summary>
        public override void ClearCache()
        {
            cacheDictionary.Clear();
        }

        /// <summary>
        /// Saves an object into the cache dictionary on key T
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">The value of the object</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public override bool SaveObject<T>(T obj)
        {
            string hashSetName = typeof(T).FullName;
            string hashFieldName = obj.Id.ToString();

            Dictionary<string, object> hashDict ;
            if (cacheDictionary.ContainsKey(hashSetName))
            {
                hashDict = (Dictionary<string, object>)cacheDictionary[hashSetName];
            }
            else
            {
                hashDict = new Dictionary<string, object>();
            }

            hashDict[hashFieldName] = obj;

            cacheDictionary[hashSetName] = hashDict;

            return true;
        }

        public override bool SaveKeyValuePair<T>(T value, int expirationTimeInMinutes)
        {
            string keyName = typeof(T).FullName;

            cacheDictionary[keyName] = value;

            return true;
        }

        /// <summary>
        /// Deletes the object with the specified id from the cache dictionary on key T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool DeleteObjectById<T>(string id)
        {
            string hashSetName = typeof(T).FullName;
            Dictionary<string, object> hashDict =
                 (Dictionary<string, object>)cacheDictionary[hashSetName];

            if (hashDict.ContainsKey(id) == true)
            {
                hashDict.Remove(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a key with name typeof(T) from the cacheDictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override bool DeleteObjectByKey<T>()
        {
            string keyName = typeof(T).FullName;

            bool deleteOperationResult = false;

            if(cacheDictionary.ContainsKey(keyName))
            {
                cacheDictionary.Remove(keyName);
                deleteOperationResult = true;
            }

            return deleteOperationResult;

        }

        public override T GetObjectById<T>(string id)
        {
            string hashSetName = typeof(T).FullName;
            Dictionary<string, object> hashDict =
                 (Dictionary<string, object>)cacheDictionary[hashSetName];

            T result = (T)hashDict[id];
            return result;
        }

        public override IEnumerable<T> GetObjects<T>()
        {
            string hashSetName = typeof(T).FullName;
            Dictionary<string, object> hashDict =
                 (Dictionary<string, object>)cacheDictionary[hashSetName];

            List<T> objects = new List<T>();

            foreach (string key in hashDict.Keys)
            {
                objects.Add((T)hashDict[key]);
            }

            return objects;
        }
        public override bool SaveObjects<T>(IEnumerable<T> list)
        {
            foreach (var obj in list)
            {
                this.SaveObject<T>(obj);
            }

            return true;
        }
        public override T GetObjectByFieldName<T>(string name)
        {
            throw new NotImplementedException();
        }

        public override T GetObjectByKey<T>(string key)
        {
            string hashFieldName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(key))
            {
                var hashDict = (Dictionary<string, object>)cacheDictionary[key];

                if (hashDict.ContainsKey(hashFieldName))
                {
                    return (T)hashDict[hashFieldName];
                }
            }

            return default(T);
        }

        public override T GetKeyValuePairByKey<T>()
        {
            string keyName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(keyName))
            {
                return (T)cacheDictionary[keyName];
            }

            return default(T);
        }

        public override bool DoesKeyExists<T>()
        {
            string hashSetName = typeof(T).FullName;

            return cacheDictionary.ContainsKey(hashSetName);
        }
        /// <summary>
        /// Save a feature of type T for the connected user
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="value">The value of the feature to save</param>
        /// <returns>true if the operation was successful, otherwise false
        /// </returns>
        public override bool SaveUserFeature<T>(T value)
        {
            string featureName = typeof(T).FullName;

            Dictionary<string, object> userFeatures = null;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];
            }
            else
            {
                userFeatures = new Dictionary<string, object>();
                cacheDictionary[userId.ToString()] = userFeatures;
            }

            userFeatures[featureName] = value;

            return true;
        }

        /// <summary>
        /// Saves the feature of the specified type for the specified user in the Cache Dictionary
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="userId">The id of the user </param>
        /// <param name="value">The value of the feature</param>
        /// <returns></returns>
        public override bool SaveUserFeature<T>(int userId, T value)
        {
            string featureName = typeof(T).FullName;

            Dictionary<string, object> userFeatures = null;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];
            }
            else
            {
                userFeatures = new Dictionary<string, object>();
                cacheDictionary[userId.ToString()] = userFeatures;
            }

            userFeatures[featureName] = value;

            return true;
        }

        /// <summary>
        /// Deletes a specified user from the Cache Dictionary for the connected user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override bool DeleteUserFeature<T>()
        {
            string featureName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];

                userFeatures.Remove(featureName);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Deletes a specified user from the Cache Dictionary for the specified user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override bool DeleteUserFeature<T>(int userId)
        {
            string featureName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];

                userFeatures.Remove(featureName);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Deletes the user id key from the Cache Dictionary
        /// </summary>
        /// <param name="userId">The key to delete</param>
        /// <returns></returns>
        public override bool DeleteUserCache(int userId)
        {
            bool deleteOperationResult = cacheDictionary.Remove(userId.ToString());
            return deleteOperationResult;
        }

        public override bool DeleteUserFeatureForMultipleUsers<T>(IEnumerable<int> userIds)
        {
            string featureName = typeof(T).FullName;

            bool deleteOperationResult = true;

            foreach (int userId in userIds)
            {

               Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];
               userFeatures.Remove(featureName);
            }

            return deleteOperationResult;
        }

        /// <summary>
        /// Returns the specified feature for the connected user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T GetUserFeature<T>()
        {
            string featureName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];

                return (T)userFeatures[featureName];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns the specified feature for the specified user
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="userId">The specified user id</param>
        /// <returns>the value of the feature if found , default otherwise</returns>
        public override T GetUserFeature<T>(int userId)
        {
            string featureName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];

                return (T)userFeatures[featureName];
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// Checks if a specified feature exists in the Cache Dictionary for the connected user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override bool DoesUserFeatureExists<T>()
        {
            string featureName = typeof(T).FullName;

            if (cacheDictionary.ContainsKey(userId.ToString()) == true)
            {
                Dictionary<string, object> userFeatures = (Dictionary<string, object>)cacheDictionary[userId.ToString()];

                return userFeatures.ContainsKey(featureName);
            }

            return false;
        }


    }
}
