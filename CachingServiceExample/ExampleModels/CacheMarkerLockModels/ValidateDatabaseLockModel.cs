using CacheServiceDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingServiceExample.ExampleModels.CacheMarkerLockModels
{
    public class ValidateDatabaseLockModel : BasicLockModel
    {
        private const int expirationTimeInMinutes = 30;
        public override int ExpirationInMinutes
        {
            get { return expirationTimeInMinutes; }
        }
    }
}
