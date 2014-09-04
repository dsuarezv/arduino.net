using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public abstract class BuildTarget
    {
        public string TargetFile;
        public string SourceFile;
        public bool CopyToTmp = false;
        public string FileExtensionOnTmp;
        public Command BuildCommand;
        public bool TargetUpToDate = false;
        public bool DisableTargetDateCheck = false;
        public bool FinishedSuccessfully = true;

        protected string EffectiveSourceFile;

        public BuildTarget()
        { 
            
        }

        public abstract void SetupCommand(string boardName);

        public virtual void SetupSources(string tempDir)
        {
            CalculateEffectiveSourceFile(tempDir);
            CopySourceToTemp();
        }

        public virtual void Build(string tempDir)
        {
            if (BuildCommand == null) return;

            if (DisableTargetDateCheck || !IsTargetUpToDate())
            {
                FinishedSuccessfully = (CmdRunner.Run(BuildCommand) == 0);
            }
        }

        protected bool IsTargetUpToDate()
        {
            if (File.Exists(TargetFile))
            {
                if (File.GetLastWriteTime(SourceFile) < File.GetLastWriteTime(TargetFile))
                {
                    TargetUpToDate = true;
                    return true;
                }
            }

            TargetUpToDate = false;
            return false;
        }

        protected void CalculateEffectiveSourceFile(string tempDir)
        {
            if (!CopyToTmp) 
            {
                EffectiveSourceFile = SourceFile;
                return;
            }

            if (FileExtensionOnTmp != null)
            {
                var newFileName = Path.GetFileNameWithoutExtension(SourceFile) + FileExtensionOnTmp;
                EffectiveSourceFile = Path.Combine(tempDir, newFileName);
                return;
            }

            var newFileName2 = Path.GetFileName(SourceFile);
            EffectiveSourceFile = Path.Combine(tempDir, newFileName2);
        }

        protected void CopySourceToTemp()
        {
            if (!CopyToTmp) return;

            File.Copy(SourceFile, EffectiveSourceFile, true);
        }

        protected string GetTmpFileName(string tempDir, string destFileName)
        {
            if (destFileName == null) destFileName = Path.GetFileName(SourceFile);
            var destFullPath = Path.Combine(tempDir, destFileName);
            return destFullPath;
        }

        public override string ToString()
        {
            if (TargetUpToDate) return string.Format("* \"{0}\" is up to date", TargetFile);
            if (BuildCommand == null) return "";

            return BuildCommand.ToString();
        }
    }

    public class CopyBuildTarget : BuildTarget
    {
        public CopyBuildTarget()
        {
            CopyToTmp = true;
        }

        public override void SetupCommand(string boardName)
        {
            
        }

        public override string ToString()
        {
            return string.Format("Copy {0} to {1}", SourceFile, EffectiveSourceFile);
        }
    }

    public class CppBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            var compiler = "hardware/tools/avr/bin/" + (IsCFile(EffectiveSourceFile) ? "avr-gcc" : "avr-g++");

            var config = Configuration.Boards[boardName]["build"];
            var usbvid = config.Get("vid");
            var usbpid = config.Get("pid");
            var includePaths = string.Format("-I\"{0}\" -I\"{1}\"",
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/cores/" + config.Get("core")),
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/variants/" + config.Get("variant"))
                );

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, compiler),
                Arguments = string.Format("-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu={0} -DF_CPU={1} -MMD -DUSB_VID={2} -DUSB_PID={3} -DARDUINO={4} {5} \"{6}\" -o \"{7}\"",
                config.Get("mcu"),
                config.Get("f_cpu"),
                (usbvid == null) ? "null" : usbvid,
                (usbpid == null) ? "null" : usbpid,
                "105",
                includePaths,
                EffectiveSourceFile,
                TargetFile)
            };
        }

        private static bool IsCFile(string file)
        {
            return (Path.GetExtension(file).ToLower() == ".c");
        }
    }


    public class ArBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            DisableTargetDateCheck = true;

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-ar"),
                Arguments = string.Format("rcs \"{0}\" \"{1}\"",
                    TargetFile, SourceFile)
            };
        }
    }


    public class ElfBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            DisableTargetDateCheck = true;

            var config = Configuration.Boards[boardName]["build"];

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-gcc"),
                Arguments = string.Format("-Os -Wl,--gc-sections -mmcu={0} -o {1} {2} -L{3} -lm",
                    config.Get("mcu"),
                    TargetFile,
                    EffectiveSourceFile,
                    Path.GetDirectoryName(TargetFile))
            };
        }
    }


    public class EepBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-objcopy"),
                Arguments = string.Format("-O ihex -j .eeprom --set-section-flags=.eeprom=alloc,load --no-change-warnings --change-section-lma .eeprom=0 {0} {1}",
                    EffectiveSourceFile,
                    TargetFile)
            };
        }
    }


    public class HexBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-objcopy"),
                Arguments = string.Format("-O ihex -R .eeprom {0} {1}",
                    EffectiveSourceFile,
                    TargetFile)
            };
        }
    }

    public class DebugBuildTarget: CppBuildTarget
    {
        public Debugger Debugger = null;

        public override void SetupSources(string tempDir)
        {
            if (Debugger != null)
            {
                var brs = Debugger.GetBreakpointsForFile(SourceFile);

                if (brs.Count > 0)
                {
                    CalculateEffectiveSourceFile(tempDir);
                    ProcessFile(brs);
                    return;
                }
            }

            base.SetupSources(tempDir);
            return;
        }

        protected void ProcessFile(List<BreakPointInfo> breakpoints)
        { 
            using (var reader = new StreamReader(SourceFile))
            { 
                using (var writer = new StreamWriter(EffectiveSourceFile))
                {
                    string line; int lineNumber = 1;

                    while ( (line = reader.ReadLine()) != null)
                    {
                        foreach (var s in ProcessLine(lineNumber++, line, breakpoints)) writer.WriteLine(s);
                    }
                }
            }
        }

        protected virtual string[] ProcessLine(int lineNumber, string line, List<BreakPointInfo> breakpoints)
        {
            if (lineNumber == 1 && Debugger != null)
            {
                return new string[] {
                    "#include \"soft_debugger.h\"",
                    "#line 2",
                    line
                };
            }

            return ProcessLineForBreakpoints(lineNumber, line, breakpoints);
        }

        protected string[] ProcessLineForBreakpoints(int lineNumber, string line, List<BreakPointInfo> breakpoints)
        {
            if (breakpoints != null)
            { 
                foreach (var br in breakpoints)
                {
                    if (br.LineNumber == lineNumber)
                    {
                        return new string[] { string.Format("    SOFTDEBUGGER_BREAK({0}); {1}", br.Id, line) };
                    }
                }
            }

            return new string[] { line };
        }
    }

    public class InoBuildTarget : DebugBuildTarget
    {
        protected override string[] ProcessLine(int lineNumber, string line, List<BreakPointInfo> breakpoints)
        {
            // DAVE: Hay que poner las declaraciones de las funciones después de los includes porque aquellas pueden usar algún tipo declarado en éstos.
            // Todo esto hay que rescribirlo para que coloque las definiciones en su sitio y procese los breakpoints.

            if (lineNumber == 1)
            {
                if (Debugger != null)
                {
                    return new string[] {
                        "#include \"soft_debugger.h\"",
                        "#include \"Arduino.h\"",
                        "void setup();",
                        "void loop();",
                        "#line 2",
                        line
                    };
                }
                else
                {
                    return new string[] {
                        "#include \"Arduino.h\"",
                        "void setup();",
                        "void loop();",
                        "#line 2",
                        line
                    };
                }
            }

            return ProcessLineForBreakpoints(lineNumber, line, breakpoints);
        }

        public override void SetupSources(string tempDir)
        {
            List<BreakPointInfo> brs = null;

            if (Debugger != null)
            {
                brs = Debugger.GetBreakpointsForFile(SourceFile);
                DisableTargetDateCheck = true;
            }

            CalculateEffectiveSourceFile(tempDir);
            ProcessFile(brs);
        }


        public override void SetupCommand(string boardName)
        {
            base.SetupCommand(boardName);
        }
    }


    public class DeployBuildTarget: BuildTarget
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
            var communication = Configuration.Programmers[mProgrammerName].Get("communication");
            var protocol = Configuration.Programmers[mProgrammerName].Get("protocol");
            var mcu = Configuration.Boards[boardName]["build"].Get("mcu");

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avrdude"),
                Arguments = string.Format("-C\"{0}\" -v -v -v -v -p{1} -c{2} -P{3} -Uflash:w:{4}:i ",
                    Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/etc/avrdude.conf"),
                    mcu,
                    protocol,   // usbasp
                    communication,   // usb
                    EffectiveSourceFile
                    )
            };
        }
    }
}
