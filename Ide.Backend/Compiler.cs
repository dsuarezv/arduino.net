using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class Compiler
    {
        private Project mProject;
        private Debugger mDebugger;
        private string mBoardName;

        public Compiler(Project p, Debugger d)
        {
            mProject = p;
            mDebugger = d;
        }

        public void Build(string boardName, bool debug)
        {
            var tempDir = CreateTempDirectory();

            if (mBoardName != boardName) Clean();
            mBoardName = boardName;

            var projectCmds = CreateProjectCompileCommands(tempDir);
            var coreCmds = CreateCoreCompileCommands(tempDir);
            var coreLibCmds = CreateLibraryCommands(tempDir, coreCmds);
            var linkCmds = CreateLinkCommand(tempDir, projectCmds);
            var elfCmds = CreateImageCommands(tempDir);

            if (!RunCommands(projectCmds, tempDir)) return;
            if (!RunCommands(coreCmds, tempDir)) return;
            if (!RunCommands(coreLibCmds, tempDir)) return;
            if (!RunCommands(linkCmds, tempDir)) return;
            if (!RunCommands(elfCmds, tempDir)) return;
        }

        public void Clean()
        {

        }

        public void Deploy(string boardName, string programmerName)
        { 
            
        }
        

        // __ Command generation ______________________________________________


        private List<BuildTarget> CreateProjectCompileCommands(string tempDir)
        {
            var result = new List<BuildTarget>();

            foreach (var sourceFile in mProject.GetFileList())
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");

                if (IsCodeFile(sourceFile))
                {
                    result.Add(new CppBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, CopyToTmp = true });
                }
                else
                {
                    result.Add(new CopyBuildTarget() { SourceFile = sourceFile });
                }
            }

            return result;
        }

        private List<BuildTarget> CreateCoreCompileCommands(string tempDir)
        {
            var result = new List<BuildTarget>();


            foreach (var sourceFile in GetCoreFiles())
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");
                result.Add(new CppBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile });
            }

            return result;
        }

        private List<BuildTarget> CreateLibraryCommands(string tempDir, List<BuildTarget> coreTargets)
        {
            var result = new List<BuildTarget>();

            foreach (var c in coreTargets)
            {
                var sourceFile = c.TargetFile;
                var targetFile = GetCoreLibraryFile(tempDir);

                result.Add(new ArBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile });
            }

            return result;
        }

        private List<BuildTarget> CreateLinkCommand(string tempDir, List<BuildTarget> projectTargets)
        {
            var result = new List<BuildTarget>();
            var sourceFiles = new StringBuilder();

            foreach (var c in projectTargets)
            {
                sourceFiles.AppendFormat("{0} ", c.TargetFile);
            }

            sourceFiles.AppendFormat(GetCoreLibraryFile(tempDir));

            result.Add(new ElfBuildTarget() { SourceFile = sourceFiles.ToString(), TargetFile = GetElfFile(tempDir) });

            return result;
        }

        private List<BuildTarget> CreateImageCommands(string tempDir)
        {
            var result = new List<BuildTarget>();
            var elfFile = GetElfFile(tempDir);

            result.Add(new EepBuildTarget() { SourceFile = elfFile, TargetFile = GetEepromFile(tempDir) });
            result.Add(new HexBuildTarget() { SourceFile = elfFile, TargetFile = GetHexFile(tempDir) });

            return result;
        }


        // __ Command execution _______________________________________________


        private bool RunCommands(List<BuildTarget> commands, string tempDir)
        {
            foreach (var cmd in commands)
            {
                if (!RunBuildTarget(cmd, tempDir)) return false;
            }

            return true;
        }

        private bool RunBuildTarget(BuildTarget cmd, string tempDir)
        {
            cmd.SetupSources(tempDir);
            cmd.SetupCommand(mBoardName);

            Console.WriteLine(cmd);

            cmd.Build(tempDir);
            
            if (cmd.BuildCommand == null) return true;
            foreach (var s in cmd.BuildCommand.Output) Console.WriteLine("    " + s);

            return cmd.FinishedSuccessfully;
        }


        // __ Helpers _________________________________________________________


        private string CreateTempDirectory()
        {
            var path = GetTempDirectory();

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        private string GetTempDirectory()
        {
            string tempDir = Path.GetTempPath();
            string dirName = string.Format("build-{0}.tmp", mProject.SketchFile);

            return Path.Combine(tempDir, dirName);
        }

        private string GetElfFile(string tempDir)
        {
            return Path.Combine(tempDir, mProject.SketchFile + ".elf");
        }

        private string GetEepromFile(string tempDir)
        {
            return Path.Combine(tempDir, mProject.SketchFile + ".eep");
        }

        private string GetHexFile(string tempDir)
        {
            return Path.Combine(tempDir, mProject.SketchFile + ".hex");
        }

        private string GetCoreLibraryFile(string tempDir)
        {
            return Path.Combine(tempDir, "core.a");
        }


        private string[] GetCoreFiles()
        {
            return Directory.GetFiles(GetBoardCoreDirectory(), "*.c*", SearchOption.AllDirectories);
        }

        private bool IsCodeFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext.StartsWith(".c") || ext == ".ino";
        }

        private string GetBoardCoreDirectory()
        {
            var config = Configuration.Boards[mBoardName]["build"];

            return Path.Combine(Configuration.ToolkitPath, "hardware/arduino/cores/" + config.Get("core"));
        }
    }
}
