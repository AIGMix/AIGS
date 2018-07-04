using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGS.Common
{
    public class Property
    {
        public object Key { get; set; }
        public object Value { get; set; }

        public Property(object sKey, object sValue)
        {
            Key = sKey;
            Value = sValue;
        }
        
        public Property(string sKey) 
        {
            Key = sKey;
            Value = "";
        }
        public Property() { }
    }
}
