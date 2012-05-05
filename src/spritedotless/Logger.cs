using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless
{
    public class Logger
    {
        private static int _tab = 0;

        public static void Indent()
        {
            _tab += 2;
        }

        public static void UnIndent()
        {
            _tab -= 2;
        }

        protected static string GetTab()
        {
            return new string(' ', _tab);
        }

        public static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(GetTab() + message);
        }

        public static void Log(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(GetTab() + message, args);
        }
    }
}
