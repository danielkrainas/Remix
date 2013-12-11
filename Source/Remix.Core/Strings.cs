/*
 * Created by SharpDevelop.
 * User: Brendan
 * Date: 7/11/2009
 * Time: 2:04 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Atlana
{
    /// <summary>
    /// Description of Strings.
    /// </summary>
    public static class Strings
    {
        public static string Capitalize(this string s)
        {
            if (s.Length == 0)
            {
                return s;
            }

            if (s.Length == 1)
            {
                return s.ToUpperInvariant();
            }

            return string.Format("{0}{1}", s.ToUpperInvariant()[0], s.ToLowerInvariant().Substring(1));
        }

        public static string OneArgument(ref string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            if (!value.Contains(" ") && !value.Contains("\""))
            {
                return value;
            }

            string arg = "";
            bool readstring = false;
            int chars = 0;
            foreach (char c in value)
            {
                chars++;
                if (c == '"')
                {
                    if (readstring)
                    {
                        readstring = false;
                        break;
                    }
                    else
                    {
                        readstring = true;
                        continue;
                    }
                }

                if (readstring)
                {
                    arg += c;
                    continue;
                }

                if (c == ' ')
                {
                    break;
                }
                else
                {
                    arg += c;
                    continue;
                }
            }

            if (chars == value.Length)
            {
                value = "";
                arg = "";
            }
            else
            {
                value = value.Substring(chars);
            }

            return arg;
        }
    }
}
