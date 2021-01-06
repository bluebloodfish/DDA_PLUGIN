using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public static class FullWidthString
    {
        public static string Get(string str, bool skipLinefeed) {
            StringBuilder newstr = new StringBuilder();
            int count = 0;

            //str.Replace((char)10, (char)13);

            foreach (var l in str)
            {
                if (skipLinefeed && l == 10) {
                    continue;
                }

                int length = Encoding.Default.GetByteCount(new char[] { l });
                if (length > 1)
                {
                    if (count % 2 == 0)
                    {
                        newstr.Append(l);
                    }
                    else
                    {
                        newstr.Append(" ");
                        newstr.Append(l);
                    }
                    count = 0;
                }
                else if (length == 1)
                {
                    //if (l == 10 && count % 2 == 1)
                    //{
                    //    newstr.Append(" ");
                    //    newstr.Append((char)13);
                    //    newstr.Append(l);
                    //    count = 0;
                    //    continue;
                    //}
                    //else if (l == 10 && count % 2 == 0) {
                    //    //newstr.Append(" ");
                    //    newstr.Append((char)13);
                    //    newstr.Append(l);
                    //    count = 0;
                    //    continue;
                    //}

                    newstr.Append(l);
                    count++;
                }
            }

            var temp = newstr.ToString();
            return newstr.ToString();
        }
    }
}
