using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DecompileToJsonConsole
{
    public class ArgsHelper:StringDictionary
    {
        public ArgsHelper(string[] args)
        {
            Regex regex = new Regex("(/(?<argName>.+?):(?<value>.+)*)|(/(?<argName>.+))", RegexOptions.IgnoreCase);
            string argName = null;

            foreach (string arg in args)
            {
                Match match = regex.Match(arg);

                if (match.Success)
                {
                    argName = match.Groups["argName"].Value.ToUpper();
                    base.Add(argName, match.Groups["value"].Value);
                }
            }
        }


        public bool IsValueAnEmptyString(string key)
        {
            string UpperKey = key.ToUpper();

            if (!(base.ContainsKey(UpperKey)))
                return true;

            if (string.IsNullOrEmpty(base[UpperKey]))
                return true;

            return false;
        }

    }
}
