using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple_Image_Resizer.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts this string to double!
        /// 
        /// By JC
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double ToDouble(this string number)
        {
            return double.Parse(number);
        }

        /// <summary>
        /// Converts this string to int!
        /// 
        /// By JC
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double ToInt(this string number)
        {
            return Int32.Parse(number);
        }
    }
}
