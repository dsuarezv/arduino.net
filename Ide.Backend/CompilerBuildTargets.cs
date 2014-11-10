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
        public List<string> AdditionalIncludePaths;

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

        protected virtual IList<string> GetIncludePaths(ConfigSection config)
        {
            var result = new List<string>() 
            {
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/cores/" + config["core"]),
                Path.Combine(Configuration.ToolkitPath, "hardware/arduino/variants/" + config["variant"]),
                Path.GetDirectoryName(EffectiveSourceFile)
            };

            if (AdditionalIncludePaths != null) result.AddRange(AdditionalIncludePaths);

            return result;
        }

        protected virtual string GetIncludeArgument(ConfigSection config)
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


    public class AssemblerBuildTarget : CppBuildTarget
    {
        public override void SetupCommand(string boardName)
        {
            var compiler = "hardware/tools/avr/bin/avr-gcc";

            var config = Configuration.Boards.GetSection(boardName).GetSection("build");
            var usbvid = config["vid"];
            var usbpid = config["pid"];
            var includePaths = GetIncludeArgument(config);

            BuildCommand = new Command()
            {
                Program = Path.Combine(Configuration.ToolkitPath, compiler),

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

        protected virtual IList<string> ProcessLine(int lineNumber, string line, List<BreakPointInfo> breakpoints)
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

        protected IList<string> ProcessLineForBreakpoints(int lineNumber, string line, List<BreakPointInfo> breakpoints)
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

        protected override IList<string> GetIncludePaths(ConfigSection config)
        {
            var includePaths = base.GetIncludePaths(config);

            if (Debugger != null) includePaths.Add(Path.Combine(Configuration.ToolkitPath, "debugger/" + config["core"]));

            return includePaths;
        }

        protected override string GetOptimizationSetting()
        {
            return "-O0";  // In debug, disable optimization. Most debug info is lost if enabled.
        }
    }


    public class InoBuildTarget : DebugBuildTarget
    {
        private SketchFileParser mParser;

        public IList<string> GetAllIncludes()
        {
            AnalizeSketchFile();

            return mParser.IncludedFiles;
        }

        private void AnalizeSketchFile()
        {
            if (mParser != null) return;

            mParser = new SketchFileParser(SourceFile);
            mParser.Parse();
        }

        protected override void ProcessFile(List<BreakPointInfo> breakpoints)
        {
            AnalizeSketchFile();

            base.ProcessFile(breakpoints);
        }

        protected override IList<string> ProcessLine(int lineNumber, string line, List<BreakPointInfo> breakpoints)
        {
            var result = new List<string>();
            bool hasAddedContent = false;

            if (lineNumber == mParser.LastIncludeLineNumber + 1)
            {
                result.Add("#include \"Arduino.h\"");
                foreach (var prototype in mParser.UniqueFunctionDeclarations) result.Add(prototype + ";");

                if (Debugger != null && !mParser.HasSetupFunction)
                {
                    result.Add("void setup()");
                    result.Add("{ ");
                    result.Add("    SOFTDEBUGGER_CONNECT");
                    result.Add("}");
                }

                hasAddedContent = true;
            }

            if (lineNumber == 1)
            {
                if (Debugger != null) result.Add("#include \"soft_debugger.h\"");

                hasAddedContent = true;
            }

            if (lineNumber == mParser.SetupFunctionFirstLine + 1)
            {
                if (Debugger != null && mParser.HasSetupFunction)
                { 
                    // Inserting this as the first line in the setup() function effectively prevents user code from 
                    // being executed first. If there was a watchdog reset (soft reset), the watchdog 
                    // registers should be set first-thing (otherwise the watchdog will keep reseting forever).

                    result.Add("SOFTDEBUGGER_CONNECT");
                    hasAddedContent = true;
                }
            }

            if (hasAddedContent)
            {
                result.Add(string.Format("#line 1 \"{0}\"", EscapePath(SourceFile)));
            }

            result.AddRange(ProcessLineForBreakpoints(lineNumber, line, breakpoints));

            return result;
        }

        protected override IList<string> GetIncludePaths(ConfigSection config)
        {
            var includes = base.GetIncludePaths(config);
            var includePaths = Compiler.GetLibraryPaths(GetAllIncludes());

            foreach (var libPath in includePaths)
            {
                includes.Add(libPath);
            }

            AdditionalIncludePaths.AddRange(includePaths);

            return includes;
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
}
