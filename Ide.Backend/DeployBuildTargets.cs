using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace arduino.net
{
    public class DeployBuildTarget : BuildTarget
    {
        private string mProgrammerName;

        public DeployBuildTarget(string programmerName)
        {
            CopyToTmp = false;
            DisableTargetDateCheck = true;
            mProgrammerName = programmerName;
        }

        public override void SetupSources(string tempDir)
        {
            EffectiveSourceFile = SourceFile;
        }

        public override void SetupCommand(string boardName)
        {
            var communication = Configuration.Instance.Programmers.GetSection(mProgrammerName)["communication"];
            var protocol = Configuration.Instance.Programmers.GetSection(mProgrammerName)["protocol"];
            var mcu = Configuration.Instance.Boards.GetSection(boardName).GetSection("build")["mcu"];
            var verify = true;

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.Instance.ToolkitPath, "hardware/tools/avr/bin/avrdude"),
                Arguments = string.Format("-C\"{0}\" -v -v -v -v -p{1} -c{2} -P{3} {5} -Uflash:w:{4}:i ",
                    Path.Combine(Configuration.Instance.ToolkitPath, "hardware/tools/avr/etc/avrdude.conf"),
                    mcu,
                    protocol,   // usbasp
                    communication,   // usb
                    EffectiveSourceFile,
                    verify ? "-V" : ""
                    )
            };
        }
    }


    public class BootLoaderDeployBuildTarget: DeployBuildTarget
    {
        private Debugger mDebugger;

        public BootLoaderDeployBuildTarget(Debugger d): base("")
        {
            mDebugger = d;
        }

        public override void SetupCommand(string boardName)
        {
            var comPort = Configuration.Instance.CurrentComPort;
            var baudRate = Configuration.Instance.Boards.GetSection(boardName).GetSection("upload")["speed"];
            var mcu = Configuration.Instance.Boards.GetSection(boardName).GetSection("build")["mcu"];
            var verify = true;

            if (Configuration.Instance.IsWindows)
            {
                comPort = "\\\\.\\" + comPort.ToUpper();
            }
            /*
             * avrdude 
             * -CC:\Program Files (x86)\Arduino\hardware/tools/avr/etc/avrdude.conf 
             * -v -v -v -v 
             * -patmega328p 
             * -carduino 
             * -P\\.\COM4 
             * -b115200 
             * -D 
             * -Uflash:w:C:\Users\dave\AppData\Local\Temp\build619860132019083674.tmp\Blink.cpp.hex:i 
             */

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.Instance.ToolkitPath, "hardware/tools/avr/bin/avrdude"),
                Arguments = string.Format("-C\"{0}\" -v -p{1} -carduino -P{2} -b{3} -D {4} -Uflash:w:{5}:i ",
                    Path.Combine(Configuration.Instance.ToolkitPath, "hardware/tools/avr/etc/avrdude.conf"),
                    mcu,
                    comPort, 
                    baudRate,
                    verify ? "-V" : "",
                    EffectiveSourceFile
                    )
            };

        }

        public override void Build(string tempDir)
        {
            DetachDebugger();
            DtrReset();

            base.Build(tempDir);
        }

        private void DetachDebugger()
        {
            if (mDebugger == null) return;

            mDebugger.Stop();
        }

        private void DtrReset()
        {
            var comPort = Configuration.Instance.CurrentComPort;

            using (var serial = new SerialPort(comPort))
            { 
                serial.RtsEnable = true;  // DTR 1
                serial.DtrEnable = true;  // DTR 0

                Thread.Sleep(100);

                serial.DtrEnable = false;  // DTR 1
                serial.RtsEnable = false;
            }
        }
    }

}
