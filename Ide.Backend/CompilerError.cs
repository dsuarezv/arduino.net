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

        // Linker error: to be added
        // C:\Users\dave\Documents\develop\Arduino\ArduinoMotionSensorExample/ArduinoMotionSensorExample.ino:23: undefined reference to `DbgBreak'

        public string Type { get; private set; }
        public string FileName { get; private set; }
        public string Message { get; private set; }
        public int LineNumber { get; private set; }

        
        public static CompilerMsg GetMsgForLine(string line)
        {
            var err = GetLineMatch(ErrorRegex, line, "Error");
            if (err != null) return err;

            var warn = GetLineMatch(WarningRegex, line, "Warning");
            if (warn != null) return warn;

            return null;
        }

        private static CompilerMsg GetLineMatch(Regex regex, string line, string type)
        {
            var m = regex.Match(line);

            if (!m.Success) return null;
            
            return new CompilerMsg()
            {
                Type = type,
                LineNumber = m.Groups["line"].GetIntValue() - 1,
                FileName = m.Groups["file"].Value,
                Message = m.Groups["msg"].Value
            };
        }
    }
}
