using CacheServiceDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingServiceExample.ExampleModels.CacheMarkerLockModels
{
    public class DefaultRolePermissionsLockModel : BasicLockModel
    {
        private const int  expirationInMinutes = 180;
        public override int ExpirationInMinutes
        {
            get { return expirationInMinutes; }
        }
    }
}
