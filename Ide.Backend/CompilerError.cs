using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduino.net
{
    public class CompilerMsg
    {
        public static Regex ErrorRegex = new Regex(@"\s*(?<file>.*):(?<line>[0-9]+): error: (?<msg>.*$)", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex WarningRegex = new Regex(@"\s*(?<file>.*):(?<line>[0-9]+): warning: (?<msg>.*$)", RegexOptions.Singleline | RegexOptions.Compiled);


        public string FileName;
        public string Message;
        public int LineNumber;


        // __ Error output matching ___________________________________________


        public static bool IsErrorLocation(string line, out string fileName, out int lineNumber, out string errMsg)
        {
            lineNumber = 0;
            fileName = null;

            if (IsLineMatch(ErrorRegex, line, out fileName, out lineNumber, out errMsg)) return true;
            if (IsLineMatch(WarningRegex, line, out fileName, out lineNumber, out errMsg)) return true;

            return false;
        }

        private static bool IsLineMatch(Regex regex, string line, out string fileName, out int lineNumber, out string msg)
        {
            var m = regex.Match(line);
            if (m.Success)
            {
                lineNumber = m.Groups["line"].GetIntValue() - 1;
                fileName = m.Groups["file"].Value;
                msg = m.Groups["msg"].Value;
                return true;
            }

            lineNumber = 0;
            fileName = null;
            msg = null;
            return false;
        }

    }
}
