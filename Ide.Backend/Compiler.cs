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
        private List<BuildTarget> mBuildCommands = new List<BuildTarget>();
        private List<BuildTarget> mCoreBuildCommands = new List<BuildTarget>();
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

            CreateCompileCommands(tempDir);

            RunBuildCommands(tempDir);
        }

        public void Clean()
        {

        }

        public void Deploy(string boardName, string programmerName)
        { 
            
        }


        private void RunBuildCommands(string tempDir)
        { 
            foreach (var cmd in mBuildCommands) RunBuildTarget(cmd, tempDir);
            foreach (var cmd in mCoreBuildCommands) RunBuildTarget(cmd, tempDir);
        }

        private void RunBuildTarget(BuildTarget cmd, string tempDir)
        {
            cmd.SetupSources(tempDir);
            cmd.SetupCommand(mBoardName);
            cmd.Build(tempDir);
            
            Console.WriteLine(cmd);
            if (cmd.BuildCommand == null) return;
            foreach (var s in cmd.BuildCommand.Output) Console.WriteLine("    " + s);
        }


        private void CreateCompileCommands(string tempDir)
        { 
            var inputFiles = new List<string>();

            // project files
            foreach (var sourceFile in mProject.GetFileList())
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");

                if (IsCodeFile(sourceFile))
                {
                    mBuildCommands.Add(new CppBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile, CopyToTmp = true });
                }
                else
                {
                    mBuildCommands.Add(new CopyBuildTarget() { SourceFile = sourceFile });
                }
            }

            // core files
            foreach (var sourceFile in GetCoreFiles())
            {
                var targetFile = Path.Combine(tempDir, Path.GetFileName(sourceFile) + ".o");
                mCoreBuildCommands.Add(new CppBuildTarget() { SourceFile = sourceFile, TargetFile = targetFile });
            }
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
    }
}
