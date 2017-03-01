using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServiceDAL.Models
{
    public abstract class BasicLockModel : BasicModel
    {
        public DateTime CreatedDate { get; set; }

        public abstract  int ExpirationInMinutes { get; }
    }
}
