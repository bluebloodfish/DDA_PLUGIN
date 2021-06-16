using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;

namespace DDAApi.Utility
{
    public static class Extensions
    {
        public static string TrimToSize(this string str, int length)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Substring(0, Math.Min(str.Length, length));
            }
            else {
                return "";
            }
        }

        public static string ToBig5(this string str)
        {
            return ChineseConverter.Convert(str, ChineseConversionDirection.SimplifiedToTraditional);

        }

        public static string ToGB(this string str)
        {
            return ChineseConverter.Convert(str, ChineseConversionDirection.TraditionalToSimplified);
        }
    }
}
