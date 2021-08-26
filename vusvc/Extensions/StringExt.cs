using System;
using System.Collections.Generic;
using System.Linq;

namespace vusvc.Extensions
{
    public static class StringExt
    {
        private static readonly IEnumerable<char> c_AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_".ToCharArray();
        public static string Sanitize(this string p_String)
        {
            return new string(p_String.Select(x => !c_AllowedChars.Contains(x) ? '_' : x).ToArray());
        }
    }
}
