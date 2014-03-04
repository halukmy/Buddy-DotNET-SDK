using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BuddyObjectPathAttribute : Attribute
    {
        public string Path { get; set; }
        public BuddyObjectPathAttribute(string path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            this.Path = path;
        }
    }
   
}
