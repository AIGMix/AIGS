using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGS
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.ConfigHelper.GetValue("TETS");


            Helper.ScreenShotHelper aScreen = new Helper.ScreenShotHelper();
            aScreen.StartCut();

          
        }
    }
}
