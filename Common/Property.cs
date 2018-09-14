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
        public object Desc { get; set; }

        public Property() { }

        public Property(object sKey, object sValue = null, object sDesc = null )
        {
            Key = sKey;
            Value = sValue;
            Desc = sDesc;
        }
        
        public Property(string sKey) 
        {
            Key = sKey;
            Value = "";
        }


    }
}
