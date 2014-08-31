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
        public static List<string> Run(string program, string arguments)
        {
            Process p = new Process();

            p.StartInfo = new ProcessStartInfo(program, arguments)
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
                if (e.Data != null) output.Add(e.Data);
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
