using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServiceDAL.Providers
{
    public class RedisCacheServiceProvider : CacheServiceProvider
    {
        private static AppSettingsReader appSettingsReader = new AppSettingsReader();

        private static Random rand = new Random();

        private static ConnectionMultiplexer
            redisConnectionMultiplexer =
            ConnectionMultiplexer.Connect(CreateEndpointConfiguration()); 

    
        private static ConfigurationOptions CreateEndpointConfiguration()
        {
            ConfigurationOptions configuration = new ConfigurationOptions();

            configuration.AllowAdmin = true;
            configuration.SyncTimeout = 10000;

            EndPointCollection endPointCollection = configuration.EndPoints;

            string cacheServiceAddresses = String.Empty;
            cacheServiceAddresses = appSettingsReader.GetValue("CacheServiceAddress", typeof(string)).ToString();

            string[] cacheServiceAddressesSplit = cacheServiceAddresses.Split(',');
            int defaultConnectionPort = 6379;

            foreach(string cacheServiceAddress in cacheServiceAddressesSplit)
            {
                endPointCollection.Add(cacheServiceAddress, defaultConnectionPort);
            }

            var randomIndex = rand.Next(0, endPointCollection.Count);
            randomIndex = rand.Next(0, endPointCollection.Count);

            var tempEndpoint = endPointCollection[randomIndex];
            endPointCollection.RemoveAt(randomIndex);
            endPointCollection.Add(tempEndpoint);

            return configuration;
        }

        private IServer server;
        private IDatabase db;
        private int userId;

        public RedisCacheServiceProvider()
        {
        }

        private string CurrentUserKey
        {
            get
            {
                return userId.ToString();
            }
        }

        /// <summary>
        /// This method retrieves a connection to the Redis Database and to the Redis Server
        /// </summary>
        /// <param name="obj">the user id of the request</param>
        /// <returns></returns>
        public override bool Connect(object obj)
        {
            var endpoints = redisConnectionMultiplexer.GetEndPoints();
            server = redisConnectionMultiplexer.GetServer(endpoints[0]);
 
            db = redisConnectionMultiplexer.GetDatabase();

            userId = Int32.Parse(obj.ToString());

            return db != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Disconnect()
        {
            db = null;
            userId = 0;
            return true;
        }

        /// <summary>
        /// Checks if there is a Redis Database connection available
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return db != null;
            }
        }

        /// <summary>
        /// Deletes the data in the Redis Database on all nodes/replications
        /// </summary>
        public override void ClearCache()
        {
            //server.FlushAllDatabases(CommandFlags.DemandMaster);

            var endpoints = redisConnectionMultiplexer.GetEndPoints();
            IServer masterServer = redisConnectionMultiplexer.GetServer(endpoints.First());
            
            foreach(var endpoint in endpoints) {
                server = redisConnectionMultiplexer.GetServer(endpoint);

                if (server.IsSlave == false) {
                    masterServer = server;
                }
            }

            masterServer.FlushAllDatabases(CommandFlags.PreferMaster);
        }

        /// <summary>
        /// Saves obj in the Redis Database in the hash table with key T
        /// </summary>
        /// <typeparam name="T">The type of the object to save</typeparam>
        /// <param name="obj">The object to save</param>
        /// <returns>true if the save operation is successful false otherwise</returns>
        public override bool SaveObject<T>(T obj)
        {
            string hashSetName = typeof(T).FullName;
            string hashFieldName = obj.Id.ToString();
            string objJSONString = SerializeValueToJSONString(obj);

            bool saveOperationResult = db.HashSet(hashSetName, hashFieldName, objJSONString);

            return saveOperationResult;
        }

        /// <summary>
        /// Saves obj in the Redis Database as a key value pair with key T and value the JSON string of "value" parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">the value to be cached</param>
        /// <param name="expirationTimeInMinutes"></param>
        /// <returns>True if the operation had succeeded, false otherwise</returns>
        public override bool SaveKeyValuePair<T>(T value, int expirationTimeInMinutes)
        {
            string keyName = typeof(T).FullName;

            string valueJSONString = SerializeValueToJSONString(value);
            TimeSpan expirationSpanInMillliseconds = TimeSpan.FromMilliseconds((expirationTimeInMinutes * 60 * 1000) );


            bool saveOperationResult = db.StringSet(keyName, valueJSONString, expirationSpanInMillliseconds);

            return saveOperationResult;
        }

        /// <summary>
        /// Deletes the object with the specified id from the hash table with key T in the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="id">The id of the object to delete</param>
        /// <returns>true if The operation was successful, false otherwise</returns>
        public override bool DeleteObjectById<T>(string id)
        {
            string hashSetName = typeof(T).FullName;
            string hashFieldName = id.ToString();

            bool saveOperationResult = false;

            if (db.HashExists(hashSetName, hashFieldName) == true)
            {
                saveOperationResult = db.HashDelete(hashSetName, hashFieldName);
            }

            return saveOperationResult;
        }

        /// <summary>
        /// Deletes a key , with name typeof(T) from the Redis database if exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if the operation succeeced , false otherwise</returns>
        public override bool DeleteObjectByKey<T>()
        {
            string keyName = typeof(T).FullName;

            bool deleteOperationResult = false;
            
            if (db.KeyExists(keyName))
            {
                deleteOperationResult = db.KeyDelete(keyName);
            }

            return deleteOperationResult;
        }

        /// <summary>
        /// Returns the object with the specified id from the hash table with the specified key
        /// </summary>
        /// <typeparam name="T">The type of the object, which refers to a redis hash table key</typeparam>
        /// <param name="id">The id of the object to return</param>
        /// <returns>The object with the specified id if found, otherwise null</returns>
        public override T GetObjectById<T>(string id)
        {
            string hashSetName = typeof(T).FullName;
            string hashFieldName = id.ToString();

            string resultJSONString = db.HashGet(hashSetName, hashFieldName, CommandFlags.PreferSlave);

            if (resultJSONString == null)
            {
                return null;
            }
            
            T result = DeserializeValueFromJSONString<T>(resultJSONString);

            return result;
        }

        /// <summary>
        /// Returns all objects from the hash table with key T in the Redis Database if found
        /// </summary>
        /// <typeparam name="T">The type of the objects to return</typeparam>
        /// <returns>IEnumrable collection of objects of type T or an empty collection</returns>
        public override IEnumerable<T> GetObjects<T>()
        {
            string hashSetName = typeof(T).FullName;

            List<T> cachedObjects = new List<T>();

            try
            {
                if (db.KeyExists(hashSetName) == true)
                {
                    HashEntry[] cachedJSONstrings = db.HashGetAll(hashSetName, CommandFlags.PreferSlave);

                    foreach (HashEntry cachedJSONString in cachedJSONstrings)
                    {
                        T cachedObject = DeserializeValueFromJSONString<T>(cachedJSONString.Value);
                        cachedObjects.Add(cachedObject);
                    }
                }

                return cachedObjects;
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("Could not deserialize an item from redis cache. This is usually caused by an old model in cache. Update the whole cache from the /users web page!", ex);
            }
        }

        /// <summary>
        /// Saves a collection of objects of type T into the Redis Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The collection of objects to save</param>
        /// <returns></returns>
        public override bool SaveObjects<T>(IEnumerable<T> list)
        {
            bool saveOperationResult = false;

            foreach (T listObject in list)
            {
                if (this.SaveObject<T>(listObject) == false)
                {
                    saveOperationResult = true;
                }
            }

            return saveOperationResult;
        }

        /// <summary>
        /// Returns an object with the specified type and name from the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="fieldName">The name of the object</param>
        /// <returns></returns>
        public override T GetObjectByFieldName<T>(string fieldName)
        {
            string hashSetName = typeof(T).FullName;
            string hashFieldName = fieldName;

            string resultJSONString = db.HashGet(hashSetName, hashFieldName, CommandFlags.PreferSlave);

            if (resultJSONString == null)
            {
                return null;
            }

            T result = DeserializeValueFromJSONString<T>(resultJSONString);

            return result;
        }

        /// <summary>
        /// Returns an object with the specified type and key from the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The key of the object</param>
        /// <returns></returns>
        public override T GetObjectByKey<T>(string key)
        {
            string hashFieldName = typeof(T).FullName;

            string resultJSONString = db.HashGet(key, hashFieldName, CommandFlags.PreferSlave);

            if (resultJSONString == null)
            {
                return default(T);
            }

            T result = DeserializeValueFromJSONString<T>(resultJSONString);

            return result;
        }

        /// <summary>
        /// Returns an object with the specified key of a key-value item cached, from the Redis Database 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the key value pair</param>
        /// <returns>The value of the key-value pair specified from key</returns>
        public override T GetKeyValuePairByKey<T>()
        {
            string keyName = typeof(T).FullName; 

            string resultJSONString = db.StringGet(keyName, CommandFlags.PreferSlave);

            if (resultJSONString == null)
            {
                return default(T);
            }

            T result = DeserializeValueFromJSONString<T>(resultJSONString);

            return result;
        }

        /// <summary>
        /// Checks if a specified by T key exists in the Redis Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override bool DoesKeyExists<T>()
        {
            string keyName = typeof(T).FullName;

            bool keyExists = db.KeyExists(keyName, CommandFlags.PreferSlave);

            return keyExists;
        }

        /// <summary>
        /// Saves an user feature to the Redis Database for the current user user (user id param in connect method)
        /// </summary>
        /// <typeparam name="T">The type of the feature to save</typeparam>
        /// <param name="value">The value of the feature to save</param>
        /// <returns>true if the operation was successfull, false otherwise</returns>
        public override bool SaveUserFeature<T>(T value)
        {
            string featureName = typeof(T).FullName;
            string valueJSONString = SerializeValueToJSONString(value);

            bool saveOperationResult = db.HashSet(CurrentUserKey, featureName, valueJSONString);

            return saveOperationResult;
        }

        /// <summary>
        /// Saves an user feature to the Redis Database for the specified user 
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="userId">The userId of the user for which we want to save the feature</param>
        /// <param name="value">The value of the feature</param>
        /// <returns></returns>
        public override bool SaveUserFeature<T>(int userId, T value)
        {
            string featureName = typeof(T).FullName;
            string valueJSONString = SerializeValueToJSONString(value);

            bool saveOperationResult = db.HashSet(userId.ToString(), featureName, valueJSONString);

            return saveOperationResult;
        }


        /// <summary>
        /// Deletes an user feature for the connected(current) user
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <returns>true if the operation is successful , false otherwise</returns>
        public override bool DeleteUserFeature<T>()
        {
            string featureName = typeof(T).FullName;
            bool deleteOperationResult = db.HashDelete(CurrentUserKey, featureName);

            return deleteOperationResult;
        }

        /// <summary>
        /// Delete a feature of the specified user 
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="userId">The userId of the specific user, the feature belongs to</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public override bool DeleteUserFeature<T>(int userId)
        {
            string featureName = typeof(T).FullName;
            bool deleteOperationResult = db.HashDelete(userId.ToString(), featureName);

            return deleteOperationResult;
        }

        /// <summary>
        /// Delete the data for the key specified by userId in the Redis Database
        /// </summary>
        /// <param name="userId">The specified userId</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public override bool DeleteUserCache(int userId)
        {
            bool deleteOperationResult = db.KeyDelete(userId.ToString());
            return deleteOperationResult;
        }

        /// <summary>
        /// Deletes a specific feature for multiple users
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <param name="userIds">Collections of user id's on which the feature to bedeleted</param>
        /// <returns>true if the operation was successful, false otherwise</returns>
        public override bool DeleteUserFeatureForMultipleUsers<T>(IEnumerable<int> userIds)
        {
            string featureName = typeof(T).FullName;

            bool deleteOperationResult = true;

            foreach (int userId in userIds)
            {
                bool currentDeleteOperationResult = db.HashDelete(userId.ToString(), featureName);

                if (deleteOperationResult && !currentDeleteOperationResult)
                {
                    deleteOperationResult = false;
                }
            }

            return deleteOperationResult;
        }

        /// <summary>
        /// Return the value of the specifed feature in the Redis Database for the connected user
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <returns>The value of the feature if it exisists, null otherwise</returns>
        public override T GetUserFeature<T>()
        {
            string featureName = typeof(T).FullName;
            string userFeatureJSONStringValue = db.HashGet(CurrentUserKey, featureName, CommandFlags.PreferSlave);

            if (userFeatureJSONStringValue == null)
            {
                return default(T);
            }

            T userFeatureRealValue = DeserializeValueFromJSONString<T>(userFeatureJSONStringValue);
            return userFeatureRealValue;
        }

        /// <summary>
        /// Return the value of the specifed feature in the Redis Database for the specified user
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        ///<param name="userIds">The userId on which the feature to be deleted</param>
        /// <returns>The value of the feature if it exisists, null otherwise</returns>
        public override T GetUserFeature<T>(int userId)
        {
            string featureName = typeof(T).FullName;
            string userFeatureJSONStringValue = db.HashGet(userId.ToString(), featureName, CommandFlags.PreferSlave);

            if (userFeatureJSONStringValue == null)
            {
                return default(T);
            }

            T userFeatureRealValue = DeserializeValueFromJSONString<T>(userFeatureJSONStringValue);
            return userFeatureRealValue;
        }

        /// <summary>
        /// Checks if the connected user posses the specified feature in the Redis Database
        /// </summary>
        /// <typeparam name="T">The type of the feature</typeparam>
        /// <returns>true if the feature exists, false otherwise</returns>
        public override bool DoesUserFeatureExists<T>()
        {
            string featureName = typeof(T).FullName;
            bool doesUserFeatureExists = db.HashExists(CurrentUserKey, featureName, CommandFlags.PreferSlave);

            return doesUserFeatureExists;
        }

        /// <summary>
        /// Serialize the specified value into JSON string
        /// </summary>
        /// <param name="value">The value to be serialized</param>
        /// <returns>The serialized value</returns>
        private string SerializeValueToJSONString(object value)
        {
            string valueJSONString = JsonConvert.SerializeObject(value);
            return valueJSONString;
        }

        /// <summary>
        /// Deserialize the specified JSON string into an object of the specified type
        /// </summary>
        /// <typeparam name="T">The type of the object in which the JSON string must be deserialized</typeparam>
        /// <param name="value">The value of the JSON string</param>
        /// <returns>The deserialized object</returns>
        private T DeserializeValueFromJSONString<T>(string value)
        {
            T realValue = JsonConvert.DeserializeObject<T>(value);

            return realValue;
        }

    }
}
