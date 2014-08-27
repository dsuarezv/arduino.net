using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace ArduinoIDE.net
{
    public class ObjectDumper
    {
        public const string ObjDumpCommand = "avr-objdump.exe";
        //-S Debugger.cpp.elf 


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
            //return RunObjectDumper("-S -l -C -F" + elfFile);
            return RunObjectDumper("-S -l -C -w -F " + elfFile);
        }

        public static List<string> GetDwarf(string elfFile)
        {
            return RunObjectDumper("-w -W " + elfFile);
        }

        private static List<string> RunObjectDumper(string arguments)
        {
            Process p = new Process();
            
            string objDumpCommand = Path.Combine(Configuration.ToolsPath, ObjDumpCommand);

            p.StartInfo = new ProcessStartInfo(objDumpCommand, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };


            var output = new List<string>();

            p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null) 
                        output.Add(e.Data);
                };

            p.ErrorDataReceived += (s, e) =>
                {
                    //var w = e.Data;
                };


            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();

            return output;
        }

    }
}
