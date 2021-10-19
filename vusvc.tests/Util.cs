using System;
using System.Linq;

namespace vusvc.tests
{
    public static class Util
    {
        private static Random m_Random = new Random((int)DateTime.Now.Ticks);

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[m_Random.Next(s.Length)]).ToArray());
        }
    }
}
