using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class MathHelper
    {
        /// <summary>
        /// 获取线段长度
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double GetLineLength(double x1, double y1, double x2, double y2)
        {
            double lineLength = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            return lineLength;
        }

        /// <summary>
        /// 获取点到线的距离
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pointx"></param>
        /// <param name="pointy"></param>
        /// <returns></returns>
        public static double GetPointToLineDistrance(double x1, double y1, double x2, double y2, double pointx, double pointy)
        {
            double space = 0;
            double a, b, c;
            a = GetLineLength(x1, y1, x2, y2);// 线段的长度    
            b = GetLineLength(x1, y1, pointx, pointy);// (x1,y1)到点的距离    
            c = GetLineLength(x2, y2, pointx, pointy);// (x2,y2)到点的距离    
            if (c <= 0.000001 || b <= 0.000001)
            {
                space = 0;
                return space;
            }
            if (a <= 0.000001)
            {
                space = b;
                return space;
            }
            if (c * c >= a * a + b * b)
            {
                space = b;
                return space;
            }
            if (b * b >= a * a + c * c)
            {
                space = c;
                return space;
            }
            double p = (a + b + c) / 2;// 半周长    
            double s = Math.Sqrt(p * (p - a) * (p - b) * (p - c));// 海伦公式求面积    
            space = 2 * s / a;// 返回点到线的距离（利用三角形面积公式求高）    
            return space;
        } 
    }
}
