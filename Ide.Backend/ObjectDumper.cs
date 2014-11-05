using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


namespace arduino.net
{
    public class ObjectDumper
    {
        public const string ObjDumpCommand = "avr-objdump.exe";
        public const string NmCommand = "avr-nm.exe";
        public const string SizeCommand = "avr-size";

        private static Regex SizeRegEx = new Regex(@"\s+(?<text>[0-9]+)\s+(?<data>[0-9]+)\s+(?<bss>[0-9]+)\s+(?<dec>[0-9]+)\s+(?<hex>[0-9a-fA-F]+)\s+(?<elf>.*\.elf)", RegexOptions.Compiled | RegexOptions.Multiline);


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

        public static List<string> GetDisassemblyWithSource(string elfFile)
        {
            return RunObjectDump(" -d -w -C -S " + elfFile);
        }

        public static List<string> GetDwarf(string elfFile)
        {
            return RunObjectDump("-w -W " + elfFile);
        }

        public static int GetSize(string elfFile)
        { 
            var s = RunAvrSize(elfFile);
            var m = SizeRegEx.Match(GetSingleString(s));
            if (!m.Success) return 0;

            var textSize = m.Groups["text"].GetIntValue();
            var dataSize = m.Groups["data"].GetIntValue();

            return textSize + dataSize;
        }

        public static string GetSingleString(List<string> listOfStrings)
        {
            var sb = new StringBuilder();

            foreach (var s in listOfStrings) sb.AppendLine(s);

            return sb.ToString();
        }

        private static List<string> RunAvrSize(string arguments)
        {
            string sizeCommand = Path.Combine(Configuration.ToolsPath, SizeCommand);

            var cmd = new Command() { Program = sizeCommand, Arguments = arguments };
            CmdRunner.Run(cmd);
            return cmd.Output;
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
