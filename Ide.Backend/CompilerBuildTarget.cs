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
        public Command BuildCommand;
        public bool TargetUpToDate = false;
        public bool FinishedSuccessfully = true;

        protected string EffectiveSourceFile;

        public BuildTarget()
        { 
            
        }

        public BuildTarget(string targetFile, string sourceFile)
        {
            TargetFile = targetFile;
            SourceFile = sourceFile;
        }

        public abstract void SetupCommand(string boardName);

        public virtual void SetupSources(string tempDir)
        {
            CopySourceToTemp(tempDir);
        }

        public virtual void Build(string tempDir)
        {
            if (BuildCommand == null) return;

            if (File.Exists(TargetFile))
            {
                if (File.GetLastWriteTime(SourceFile) > File.GetLastWriteTime(TargetFile))
                {
                    FinishedSuccessfully = (CmdRunner.Run(BuildCommand) == 0);
                }
                else
                { 
                    TargetUpToDate = true;
                }
            }
            else
            {
                FinishedSuccessfully = (CmdRunner.Run(BuildCommand) == 0);
            }
        }

        protected void CopySourceToTemp(string tempDir, string destFileName = null)
        {
            EffectiveSourceFile = SourceFile;

            if (!CopyToTmp) return;

            if (destFileName == null) destFileName = Path.GetFileName(SourceFile);
            var destFullPath = Path.Combine(tempDir, destFileName);

            File.Copy(SourceFile, destFullPath, true);

            EffectiveSourceFile = destFullPath;
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

        public static bool IsCFile(string file)
        {
            return (Path.GetExtension(file).ToLower() == ".c");
        }
    }


    public class ArBuildTarget : BuildTarget
    {
        public override void SetupCommand(string boardName)
        {
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

    public class InoBuildTarget : CppBuildTarget
    {
        public override void SetupSources(string tempDir)
        {
            base.SetupSources(tempDir);
        }

        public override void SetupCommand(string boardName)
        {


            base.SetupCommand(boardName);
        }
    }
}
