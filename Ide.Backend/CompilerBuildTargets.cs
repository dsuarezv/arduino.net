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
        public bool TargetIsUpToDate = false;
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

        protected virtual bool IsTargetUpToDate()
        {
            return IsTargetUpToDate(SourceFile, TargetFile);
        }

        protected bool IsTargetUpToDate(string sourceFile, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                if (File.GetLastWriteTime(sourceFile) < File.GetLastWriteTime(targetFile))
                {
                    TargetIsUpToDate = true;
                    return true;
                }
            }

            TargetIsUpToDate = false;
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
            if (TargetIsUpToDate) return string.Format("* \"{0}\" is up to date", TargetFile);
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

            var config = Configuration.Boards.GetSection(boardName).GetSection("build");
            var usbvid = config["vid"];
            var usbpid = config["pid"];
            var includePaths = GetIncludeArgument(config);

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, compiler),
                //Arguments = string.Format("-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu={0} -DF_CPU={1} -MMD -DUSB_VID={2} -DUSB_PID={3} -DARDUINO={4} {5} \"{6}\" -o \"{7}\"",
                Arguments = string.Format("-c -g {8} -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu={0} -DF_CPU={1} -MMD -DUSB_VID={2} -DUSB_PID={3} -DARDUINO={4} {5} \"{6}\" -o \"{7}\"",
                    config["mcu"],
                    config["f_cpu"],
                    (usbvid == null) ? "null" : usbvid,
                    (usbpid == null) ? "null" : usbpid,
                    "105",
                    includePaths,
                    EffectiveSourceFile,
                    TargetFile,
                    GetOptimizationSetting())
            };
        }

        private static bool IsCFile(string file)
        {
            return (Path.GetExtension(file).ToLower() == ".c");
        }

        public static string[] GetIncludePaths(ConfigurationFile config)
        {
            return new string[] {
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/cores/" + config["core"]),
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/variants/" + config["variant"]),
                Path.Combine(Configuration.ToolkitPath, "debugger/" + config["core"])
            };
        }

        public static string GetIncludeArgument(ConfigurationFile config)
        {
            var sb = new StringBuilder();

            foreach (var path in GetIncludePaths(config)) sb.AppendFormat("-I\"{0}\" ", path);
            
            return sb.ToString();
        }

        protected virtual string GetOptimizationSetting()
        {
            return "-Os";   // Optimize for size.
        }
    }

    public class AssemblerBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            var compiler = "hardware/tools/avr/bin/avr-gcc";

            var config = Configuration.Boards.GetSection(boardName).GetSection("build");
            var usbvid = config["vid"];
            var usbpid = config["pid"];
            var includePaths = CppBuildTarget.GetIncludeArgument(config);

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, compiler),
                //Arguments = string.Format("-c -g -mmcu={0} {1} \"{2}\" -o \"{3}\"",
                //config["mcu"),
                //includePaths,
                //EffectiveSourceFile,
                //TargetFile)
                Arguments = string.Format("-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu={0} -DF_CPU={1} -MMD -DUSB_VID={2} -DUSB_PID={3} -DARDUINO={4} {5} \"{6}\" -o \"{7}\"",
                    config["mcu"],
                    config["f_cpu"],
                    (usbvid == null) ? "null" : usbvid,
                    (usbpid == null) ? "null" : usbpid,
                    "105",
                    includePaths,
                    EffectiveSourceFile,
                    TargetFile)                
            };
        }
    }


    public class ArBuildTarget : BuildTarget
    {
        public List<string> SourceFiles = new List<string>();

        public override void SetupCommand(string boardName)
        {
            DisableTargetDateCheck = true;

            var sb = new StringBuilder();
            foreach (var file in SourceFiles) sb.AppendFormat("\"{0}\" ", file);

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-ar"),
                Arguments = string.Format("rcs \"{0}\" {1}",
                    TargetFile, sb)
            };
        }

        protected override bool IsTargetUpToDate()
        {
            foreach (var s in SourceFiles)
            {
                if (!IsTargetUpToDate(s, TargetFile)) return false;
            }

            return true;
        }
    }


    public class ElfBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            DisableTargetDateCheck = true;

            var config = Configuration.Boards.GetSection(boardName).GetSection("build");

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, "hardware/tools/avr/bin/avr-gcc"),
                Arguments = string.Format("-Os -Wl,--gc-sections -mmcu={0} -o {1} {2} -L{3} -lm",
                    config["mcu"],
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
                var brs = Debugger.BreakPoints.GetBreakpointsForFile(SourceFile);

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

        protected virtual void ProcessFile(List<BreakPointInfo> breakpoints)
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
                    "#line 1",
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
                        return new string[] { string.Format("  SOFTDEBUGGER_BREAK({0});  {1}", br.Id, line) };
                    }
                }
            }

            return new string[] { line };
        }

        protected override string GetOptimizationSetting()
        {
            return "-O0";  // In debug, disable optimization. Most debug info is lost if enabled.
        }
    }

    public class InoBuildTarget : DebugBuildTarget
    {
        private SketchFileParser mParser;

        protected override void ProcessFile(List<BreakPointInfo> breakpoints)
        {
            // First pass: analyze

            mParser = new SketchFileParser(SourceFile);
            mParser.Parse();

            // Second pass: edit through ProcessLine
            base.ProcessFile(breakpoints);
        }

        protected override string[] ProcessLine(int lineNumber, string line, List<BreakPointInfo> breakpoints)
        {
            if (lineNumber == 1)
            {
                if (Debugger != null)
                {
                    return new string[] {
                        "#include \"soft_debugger.h\"",
                        string.Format("#line {0} \"{1}\"", lineNumber, EscapePath(SourceFile)),
                        line
                    };
                }
                else
                {
                    return new string[] {
                        string.Format("#line {0} \"{1}\"", lineNumber, EscapePath(SourceFile)),
                        line 
                    };
                }
            }
            else if (lineNumber == mParser.LastIncludeLineNumber + 2)
            {
                var result = new List<string>();
                
                result.Add("#include \"Arduino.h\"");
                foreach (var prototype in mParser.UniqueFunctionDeclarations) result.Add(prototype + ";");

                if (Debugger != null && !mParser.HasSetupFunction)
                {
                    result.Add("void setup()");
                    result.Add("{ ");
                    result.Add("    SOFTDEBUGGER_CONNECT");
                    result.Add("}");
                }

                result.Add("#line " + lineNumber);
                result.Add(line);

                return result.ToArray<string>();
            }
            else if (lineNumber == mParser.SetupFunctionFirstLine + 1)
            {
                if (Debugger != null && mParser.HasSetupFunction)
                { 
                    // Inserting this as the first line in the setup() function effectively prevents user code from 
                    // being executed first. If there is a watchdog reset (soft reset) in place, the watchdog 
                    // registers should be set first-thing, and this would enter a reset loop. May brick a bootloader 
                    // arduino (needs a programmer to fix).

                    return new string[] {
                        "SOFTDEBUGGER_CONNECT",
                        "#line " + lineNumber,
                        line
                    };
                }
            }

            return ProcessLineForBreakpoints(lineNumber, line, breakpoints);
        }

        private static string EscapePath(string path)
        {
            return path.Replace("\\", "\\\\");
        }

        public override void SetupSources(string tempDir)
        {
            List<BreakPointInfo> brs = null;

            if (Debugger != null)
            {
                brs = Debugger.BreakPoints.GetBreakpointsForFile(SourceFile);
                DisableTargetDateCheck = true;
            }

            CalculateEffectiveSourceFile(tempDir);
            ProcessFile(brs);
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
            var communication = Configuration.Programmers.GetSection(mProgrammerName)["communication"];
            var protocol = Configuration.Programmers.GetSection(mProgrammerName)["protocol"];
            var mcu = Configuration.Boards.GetSection(boardName).GetSection("build")["mcu"];

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
