using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGS.Common
{
    public enum Status
    {
        Err = -1,
        Success = 0,
        Wait = 1,
        Working = 2,
        Suspend = 3,
    }
}
