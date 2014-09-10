using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace arduino.net
{
    public class ObjectDumper
    {
        public const string ObjDumpCommand = "avr-objdump.exe";
        public const string NmCommand = "avr-nm.exe";


        public static List<string> GetHelp()
        {
            return RunObjectDump("--help");
        }

        public static List<string> GetSymbolTable(string elfFile)
        {
            return RunObjectDump("-t " + elfFile);
        }

        public static List<string> GetNmSymbolTable(string elfFile)
        {
            return RunNm("-n -l " + elfFile);
        }

        public static List<string> GetDisassembly(string elfFile)
        {
            return RunObjectDump(" -d -w -C " + elfFile);
        }

        public static List<string> GetDwarf(string elfFile)
        {
            return RunObjectDump("-w -W " + elfFile);
        }

        public static string GetSingleString(List<string> listOfStrings)
        {
            var sb = new StringBuilder();

            foreach (var s in listOfStrings) sb.AppendLine(s);

            return sb.ToString();
        }

        private static List<string> RunNm(string arguments)
        {
            string nmCommand = Path.Combine(Configuration.ToolsPath, NmCommand);

            var cmd = new Command() { Program = nmCommand, Arguments = arguments };
            CmdRunner.Run(cmd);
            return cmd.Output;
        }

        private static List<string> RunObjectDump(string arguments)
        {
            string objDumpCommand = Path.Combine(Configuration.ToolsPath, ObjDumpCommand);

            var cmd = new Command() { Program = objDumpCommand, Arguments = arguments };
            CmdRunner.Run(cmd);
            return cmd.Output;
        }


    }
}
