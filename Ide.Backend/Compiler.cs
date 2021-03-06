﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduino.net
{
    public class Compiler
    {
        private const string SketchFileExtensionOnTmp = ".cpp";

        private bool mIsOperationRunning = false;
        private BuildStage mBuildStage = BuildStage.NeedsBuild;
        private ObservableCollection<CompilerMsg> mCompilerErrors = new ObservableCollection<CompilerMsg>();
        private List<string> mIncludePaths = new List<string>();
        private Project mProject;
        private Debugger mDebugger;
        private string mBoardName;
        private DateTime mLastSuccessfulCompilationDate = DateTime.MinValue;
        private DateTime mLastSuccessfulDeploymentDate = DateTime.MinValue;


        public bool IsOperationRunning
        {
            get { return mIsOperationRunning; }
        }

        public bool IsDirty
        {
            get { return mBuildStage != BuildStage.ReadyToRun; }
        }

        public DateTime LastSuccessfulCompilationDate
        {
            get { return mLastSuccessfulCompilationDate; }
        }

        public DateTime LastSuccessfulDeploymentDate
        {
            get { return mLastSuccessfulDeploymentDate; }
        }

        public Project Project
        {
            get { return mProject; }
            set { mProject = value; MarkAsDirty(BuildStage.NeedsBuild); }
        }

        public ObservableCollection<CompilerMsg> CompilerMessages
        {
            get { return mCompilerErrors; }
        }
        

        public Compiler(Project p, Debugger d)
        {
            mProject = p;
            mDebugger = d;
        }


        public void MarkAsDirty(BuildStage stage)
        {
            mBuildStage = stage;
        }
        
        public Task<bool> BuildAsync(string boardName, bool debug)
        {
            return Task.Run<bool>(() => Build(boardName, debug));
        }

        public bool Build(string boardName, bool debug)
        {
            if (mIsOperationRunning) return false;

            try
            {
                mIsOperationRunning = true;

                if (IsBuildUpToDate())
                {
                    Logger.LogCompiler("Build is up-to-date.");
                    return true;
                }

                var tempDir = CreateTempDirectory();

                SetupBoardName(boardName);

                mCompilerErrors.Clear();
                mIncludePaths.Clear();

                var debuggerCmds = CreateDebuggerCompileCommands(tempDir, debug);
                var projectCmds = CreateProjectCompileCommands(tempDir, debug);
                var librariesCmds = CreateLibraryCompileCommands(tempDir, projectCmds, debug);
                var coreCmds = CreateCoreCompileCommands(tempDir);
                var coreLibCmds = CreateCoreLibraryCommands(tempDir, coreCmds);
                var linkCmds = CreateLinkCommand(tempDir, projectCmds, debuggerCmds, librariesCmds);
                var elfCmds = CreateImageCommands(tempDir);

                if (!RunCommands(projectCmds, tempDir)) return false;
                if (!RunCommands(debuggerCmds, tempDir)) return false;
                if (!RunCommands(librariesCmds, tempDir)) return false;
                if (!RunCommands(coreCmds, tempDir)) return false;
                if (!RunCommands(coreLibCmds, tempDir)) return false;
                if (!RunCommands(linkCmds, tempDir)) return false;
                if (!RunCommands(elfCmds, tempDir)) return false;
                if (!VerifySize(tempDir, true)) return false;

                mLastSuccessfulCompilationDate = DateTime.Now;
                mBuildStage = BuildStage.NeedsDeploy;

                BuildDwarf();

                return true;
            }
            finally
            {
                mIsOperationRunning = false;
            }
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
            if (mIsOperationRunning) return false;

            try
            {
                mIsOperationRunning = true;

                if (IsDeployUpToDate()) 
                {
                    Logger.LogCompiler("Deploy: No changes since last deployment.");
                    return true;
                }

                if (!VerifySize(null, false)) throw new Exception("Sketch size is larger than supported Arduino memory. Remove some code to get it below the maximum and try again.");

                var tempDir = CreateTempDirectory();
                var hexFile = GetHexFile(tempDir);

                SetupBoardName(boardName);

                if (!File.Exists(hexFile))
                {
                    if (!Build(boardName, debug)) return false;
                }

                IdeManager.Debugger.Stop();

                var deployCmds = CreateDeployCommands(tempDir, programmerName);

                if (!RunCommands(deployCmds, tempDir)) return false;

                // Successful deploy. Post actions.

                mLastSuccessfulDeploymentDate = DateTime.Now;
                mBuildStage = BuildStage.ReadyToRun;
                SessionSettings.Save();
                BuildDwarf();

                return true;
            }
            finally
            {
                mIsOperationRunning = false;
            }
        }
        
        public void BuildDwarf()
        {
            IdeManager.Dwarf = new DwarfTree(new DwarfTextParser(GetElfFile()));
        }


        private bool IsBuildUpToDate()
        {
            return (mBuildStage != BuildStage.NeedsBuild);
        }

        private bool IsDeployUpToDate()
        {
            return (mBuildStage == BuildStage.ReadyToRun);
        }

        private void SetupBoardName(string boardName)
        {
            if (mBoardName != boardName) Clean();
            mBoardName = boardName;
        }


        // __ Size verification _______________________________________________


        private bool VerifySize(string tempDir, bool printOutput)
        {
            int maxSize = Int32.Parse(Configuration.Instance.Boards.GetSection(mBoardName).GetSection("upload")["maximum_size"]);
            int elfSize = ObjectDumper.GetSize(GetElfFile(tempDir));
            bool result = maxSize > elfSize;
            
            if (printOutput)
            { 
                if (result)
                {
                    Logger.LogCompiler("Binary sketch size: {0} bytes (of a {1} byte maximum)", elfSize, maxSize);
                }
                else
                {
                    Logger.LogCompiler("WARNING: Binary sketch size of {0} bytes IS LARGER THAN the {1} byte maximum.", elfSize, maxSize);
                }

            }                
                
            return result;
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
            
            var config = Configuration.Instance.Boards.GetSection(mBoardName).GetSection("build");
            var sourceDir = Path.Combine(Configuration.Instance.ToolkitPath, "debugger/" + config["core"]);

            var fileList = Project.GetCodeFilesOnPath(sourceDir);
            return GetCommandsForFiles(tempDir, debug, fileList, false);
        }

        private List<BuildTarget> GetCommandsForFiles(string tempDir, bool debug, IList<string> fileList, bool copyToTmp = true)
        {
            var result = new List<BuildTarget>();
            var debugger = debug ? mDebugger : null;

            foreach (var sourceFile in fileList)
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");

                switch (GetFileType(sourceFile))
                {
                    case FileType.Code: result.Add(new DebugBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, Debugger = debugger, CopyToTmp = copyToTmp, AdditionalIncludePaths = mIncludePaths }); break;
                    case FileType.Sketch: result.Add(new InoBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, Debugger = debugger, FileExtensionOnTmp = SketchFileExtensionOnTmp, CopyToTmp = copyToTmp, AdditionalIncludePaths = mIncludePaths }); break;
                    case FileType.Assembler: result.Add(new AssemblerBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, CopyToTmp = false, AdditionalIncludePaths = mIncludePaths }); break;
                    default:
                        result.Add(new CopyBuildTarget() { SourceFile = sourceFile }); break;
                }
            }
            return result;
        }

        private List<BuildTarget> CreateLibraryCompileCommands(string tempDir, List<BuildTarget> projectCmds, bool debug)
        {
            var result = new List<string>();
            var libPaths = GetLibraryPaths(GetAllLibraryIncludes(projectCmds));

            foreach (var libPath in libPaths)
            {
                foreach (var cppFile in Directory.GetFiles(libPath, "*.c*", SearchOption.TopDirectoryOnly))
                {
                    result.Add(cppFile);
                }
            }

            return GetCommandsForFiles(tempDir, debug, result, false);
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

        private List<BuildTarget> CreateCoreLibraryCommands(string tempDir, List<BuildTarget> coreTargets)
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

        private List<BuildTarget> CreateLinkCommand(string tempDir, params List<BuildTarget>[] targets)
        {
            var result = new List<BuildTarget>();
            var sourceFiles = new StringBuilder();

            foreach (var subTargets in targets)
            { 
                foreach (var c in subTargets) if (c.TargetFile != null) sourceFiles.AppendFormat("{0} ", c.TargetFile);
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

        private List<BuildTarget> CreateDeployCommands(string tempDir, string programmerName)
        {
            var result = new List<BuildTarget>();

            var deployCmd = (programmerName == null || programmerName == Configuration.NullProgrammerName) ?
                new BootLoaderDeployBuildTarget(mDebugger) :
                new DeployBuildTarget(programmerName);

            deployCmd.SourceFile = GetHexFile(tempDir);

            result.Add(deployCmd);

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
                if (Configuration.Instance.VerboseBuildOutput)
                { 
                    Logger.LogCompiler("  {0} is up to date.", Path.GetFileName(cmd.TargetFile));
                }
            }
            else
            {
                if (Configuration.Instance.VerboseBuildOutput)
                { 
                    Logger.LogCompiler("Building {0}: {1}", Path.GetFileName(cmd.TargetFile), cmd);
                }
            }
            
            if (cmd.BuildCommand == null) return true;
            foreach (var s in cmd.BuildCommand.Output) ProcessOutputLine(s);

            return cmd.FinishedSuccessfully;
        }

        private void ProcessOutputLine(string line)
        {
            bool verbose = Configuration.Instance.VerboseBuildOutput;

            if (verbose) Logger.LogCompilerDirect("    " + line);

            var msg = CompilerMsg.GetMsgForLine(line);
            if (msg == null) return;

            mCompilerErrors.Add(msg);

            if (!verbose && msg.Type == "Error") Logger.LogCompilerDirect(msg.ToString());
        }


        // __ Library support _________________________________________________


        private IList<string> GetAllLibraryIncludes(IList<BuildTarget> targets)
        {
            List<string> result = new List<string>();

            foreach (BuildTarget target in targets)
            {
                var ino = target as InoBuildTarget;
                if (ino == null) continue;

                result.AddRange(ino.GetAllIncludes());
            }

            return result;
        }

        public static IList<string> GetLibraryPaths(IList<string> includedFiles)
        {
            List<string> result = new List<string>();

            foreach (var inc in includedFiles)
            {
                var libName = Path.GetFileNameWithoutExtension(inc);

                foreach (var path in Configuration.Instance.LibraryPaths)
                {
                    var libPath = Path.GetFullPath(Path.Combine(path, libName));

                    if (Directory.Exists(libPath))
                    {
                        result.Add(libPath);
                        result.AddRange(GetLibrarySubPaths(libPath));
                    }
                }
            }

            return result;
        }

        private static IList<string> GetLibrarySubPaths(string path)
        {
            List<string> result = new List<string>();

            var subdirs = Directory.GetDirectories(path);

            foreach (var dir in subdirs)
            { 
                var dirName = Path.GetFileName(dir).ToLower();
                if (dirName == "examples") continue;

                result.Add(dir);
                result.AddRange(GetLibrarySubPaths(dir));
            }

            return result;
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

        public string GetSketchTransformedFile(string tempDir = null)
        {
            if (tempDir == null) tempDir = GetTempDirectory();

            var sketchFile = mProject.GetSketchFileName();

            return InoBuildTarget.GetEffectiveSourceFile(sketchFile, tempDir, SketchFileExtensionOnTmp);
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
            var config = Configuration.Instance.Boards.GetSection(mBoardName).GetSection("build");

            return Path.Combine(Configuration.Instance.ToolkitPath, "hardware/arduino/cores/" + config["core"]);
        }
    }

    public enum BuildStage { NeedsBuild, NeedsDeploy, ReadyToRun }
}
