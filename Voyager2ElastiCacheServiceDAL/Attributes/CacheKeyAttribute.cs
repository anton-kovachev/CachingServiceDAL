using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServiceDAL.Attributes
{
    public class CacheKeyAttribute : Attribute
    {
        public string Key { get; set; }
    }
}
