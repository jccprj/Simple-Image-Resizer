using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple_Image_Resizer.Extensions
{
    public static class Int32Extensions
    {
        /// <summary>
        /// Converts this int to double!
        /// 
        /// By JC
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double ToDouble(this int number)
        {
            return double.Parse(number.ToString());
        }
    }
}
