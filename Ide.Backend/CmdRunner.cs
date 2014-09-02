using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class CmdRunner
    {
        public static int Run(Command cmd)
        {
            Process p = new Process();

            p.StartInfo = new ProcessStartInfo(cmd.Program, cmd.Arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            p.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) cmd.Output.Add(e.Data);
            };

            p.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) cmd.Output.Add(e.Data);
            };


            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();

            return p.ExitCode;
        }
    }

    public class Command
    {
        public string Program;
        public string Arguments;

        public List<string> Output = new List<string>();

        public override string ToString()
        {
            return string.Format("{0} {1}", Program, Arguments);
        }
    }
}
