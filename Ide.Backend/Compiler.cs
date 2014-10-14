using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduino.net
{
    public class Compiler
    {
        private Project mProject;
        private Debugger mDebugger;
        private string mBoardName;
        private DateTime mLastSuccessfulCompilationDate = DateTime.MinValue;
        private DateTime mLastSuccessfulDeploymentDate = DateTime.MinValue;


        public DateTime LastSuccessfulCompilationDate
        {
            get { return mLastSuccessfulCompilationDate; }
        }

        public DateTime LastSuccessfulDeploymentDate
        {
            get { return mLastSuccessfulDeploymentDate; }
        }



        public Compiler(Project p, Debugger d)
        {
            mProject = p;
            mDebugger = d;
        }



        public Task<bool> BuildAsync(string boardName, bool debug)
        {
            return Task.Run<bool>(() => Build(boardName, debug));
        }

        public bool Build(string boardName, bool debug)
        {
            var tempDir = CreateTempDirectory();

            SetupBoardName(boardName);

            var debuggerCmds = CreateDebuggerCompileCommands(tempDir, debug);
            var projectCmds = CreateProjectCompileCommands(tempDir, debug);
            var coreCmds = CreateCoreCompileCommands(tempDir);
            var coreLibCmds = CreateLibraryCommands(tempDir, coreCmds);
            var linkCmds = CreateLinkCommand(tempDir, projectCmds, debuggerCmds);
            var elfCmds = CreateImageCommands(tempDir);

            if (!RunCommands(debuggerCmds, tempDir)) return false;
            if (!RunCommands(projectCmds, tempDir)) return false;
            if (!RunCommands(coreCmds, tempDir)) return false;
            if (!RunCommands(coreLibCmds, tempDir)) return false;
            if (!RunCommands(linkCmds, tempDir)) return false;
            if (!RunCommands(elfCmds, tempDir)) return false;

            mLastSuccessfulCompilationDate = DateTime.Now;

            return true;
        }

        public void Clean()
        {
            foreach (var f in Directory.GetFiles(GetTempDirectory()))
            {
                try
                {
                    File.Delete(f);
                }
                catch { }
            }
        }

        public Task<bool> DeployAsync(string boardName, string programmerName, bool debug)
        {
            return Task.Run<bool>(() => Deploy(boardName, programmerName, debug));
        }

        public bool Deploy(string boardName, string programmerName, bool debug)
        {
            var tempDir = CreateTempDirectory();
            var hexFile = GetHexFile(tempDir);

            SetupBoardName(boardName);

            if (!File.Exists(hexFile))
            {
                if (!Build(boardName, debug)) return false;
            }

            IdeManager.Debugger.Detach();

            var deployCmds = CreateDeployCommands(tempDir, programmerName);

            if (!RunCommands(deployCmds, tempDir)) return false;

            // Successful deploy. Post actions.

            mLastSuccessfulDeploymentDate = DateTime.Now;
            SessionSettings.Save();
            BuildDwarf();

            return true;
        }
        
        public void BuildDwarf()
        {
            IdeManager.Dwarf = new DwarfTree(new DwarfTextParser(GetElfFile()));
        }


        private void SetupBoardName(string boardName)
        {
            if (mBoardName != boardName) Clean();
            mBoardName = boardName;
        }


        // __ Command generation ______________________________________________

        
        private enum FileType { Sketch, Code, Include, Assembler, Other };


        private List<BuildTarget> CreateProjectCompileCommands(string tempDir, bool debug)
        {
            return GetCommandsForFiles(tempDir, debug, mProject.GetFileList());
        }
        
        private List<BuildTarget> CreateDebuggerCompileCommands(string tempDir, bool debug)
        {
            if (!debug) return new List<BuildTarget>();
            
            var config = Configuration.Boards.GetSection(mBoardName).GetSection("build");
            var sourceDir = Path.Combine(Configuration.ToolkitPath, "debugger/" + config["core"]);

            var fileList = Project.GetCodeFilesOnPath(sourceDir);
            return GetCommandsForFiles(tempDir, debug, fileList, false);
        }

        private List<BuildTarget> GetCommandsForFiles(string tempDir, bool debug, List<string> fileList, bool copyToTmp = true)
        {
            var result = new List<BuildTarget>();
            var debugger = debug ? mDebugger : null;

            foreach (var sourceFile in fileList)
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");

                switch (GetFileType(sourceFile))
                {
                    case FileType.Code: result.Add(new DebugBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, Debugger = debugger, CopyToTmp = copyToTmp }); break;
                    case FileType.Sketch: result.Add(new InoBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, Debugger = debugger, FileExtensionOnTmp = ".cpp", CopyToTmp = copyToTmp }); break;
                    case FileType.Assembler: result.Add(new AssemblerBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, CopyToTmp = false }); break;
                    default:
                        result.Add(new CopyBuildTarget() { SourceFile = sourceFile }); break;
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
            var cmd = new ArBuildTarget() { TargetFile = GetCoreLibraryFile(tempDir) };

            foreach (var c in coreTargets)
            {
                cmd.SourceFiles.Add(c.TargetFile);
            }

            result.Add(cmd);

            return result;
        }

        private List<BuildTarget> CreateLinkCommand(string tempDir, List<BuildTarget> projectTargets, List<BuildTarget> debuggerTargets)
        {
            var result = new List<BuildTarget>();
            var sourceFiles = new StringBuilder();

            foreach (var c in projectTargets) if (c.TargetFile != null) sourceFiles.AppendFormat("{0} ", c.TargetFile);
            foreach (var c in debuggerTargets) if (c.TargetFile != null) sourceFiles.AppendFormat("{0} ", c.TargetFile);

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

        private List<BuildTarget> CreateDeployCommands(string tempDir, string programmerName)
        {
            var result = new List<BuildTarget>();
            
            result.Add(new DeployBuildTarget(programmerName) { SourceFile = GetHexFile(tempDir) });

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

            cmd.Build(tempDir);

            if (cmd.TargetIsUpToDate)
            {
                Logger.LogCompiler("  {0} is up to date.", Path.GetFileName(cmd.TargetFile));
            }
            else
            { 
                Logger.LogCompiler("Building {0}: {1}", Path.GetFileName(cmd.TargetFile), cmd);
            }
            
            if (cmd.BuildCommand == null) return true;
            foreach (var s in cmd.BuildCommand.Output) Logger.LogCompiler("    " + s);

            return cmd.FinishedSuccessfully;
        }


        // __ Helpers _________________________________________________________


        private string CreateTempDirectory()
        {
            var path = GetTempDirectory();

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        public string GetTempDirectory()
        {
            string tempDir = Path.GetTempPath();
            string dirName = string.Format("build-{0}.tmp", mProject.SketchFile);

            return Path.Combine(tempDir, dirName);
        }

        public string GetElfFile(string tempDir = null)
        {
            if (tempDir == null) tempDir = GetTempDirectory();

            return Path.Combine(tempDir, mProject.SketchFile + ".elf");
        }

        private string GetEepromFile(string tempDir = null)
        {
            if (tempDir == null) tempDir = GetTempDirectory();

            return Path.Combine(tempDir, mProject.SketchFile + ".eep");
        }

        private string GetHexFile(string tempDir = null)
        {
            if (tempDir == null) tempDir = GetTempDirectory();

            return Path.Combine(tempDir, mProject.SketchFile + ".hex");
        }

        private string GetCoreLibraryFile(string tempDir = null)
        {
            if (tempDir == null) tempDir = GetTempDirectory();

            return Path.Combine(tempDir, "core.a");
        }


        private string[] GetCoreFiles()
        {
            return Directory.GetFiles(GetBoardCoreDirectory(), "*.c*", SearchOption.AllDirectories);
        }

        private static FileType GetFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            if (ext.StartsWith(".c")) return FileType.Code;
            else if (ext == ".ino" || ext == ".pde") return FileType.Sketch;
            else if (ext.StartsWith(".h")) return FileType.Include;
            else if (ext == ".s") return FileType.Assembler;

            return FileType.Other;
        }

        private string GetBoardCoreDirectory()
        {
            var config = Configuration.Boards.GetSection(mBoardName).GetSection("build");

            return Path.Combine(Configuration.ToolkitPath, "hardware/arduino/cores/" + config["core"]);
        }
    }
}
