using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace arduino.net
{
    public class ObjectDumper
    {
        public const string ObjDumpCommand = "avr-objdump.exe";


        public static List<string> GetHelp()
        {
            return RunObjectDumper("--help");
        }

        public static List<string> GetSymbolTable(string elfFile)
        {
            return RunObjectDumper("-t " + elfFile);
        }

        public static List<string> GetDisassembly(string elfFile)
        {
            return RunObjectDumper(" -d -w -C " + elfFile);
        }

        public static List<string> GetDwarf(string elfFile)
        {
            return RunObjectDumper("-w -W " + elfFile);
        }

        private static List<string> RunObjectDumper(string arguments)
        {
            string objDumpCommand = Path.Combine(Configuration.ToolsPath, ObjDumpCommand);

            var cmd = new Command() { Program = objDumpCommand, Arguments = arguments };
            CmdRunner.Run(cmd);
            return cmd.Output;
        }

    }
}
