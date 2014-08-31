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
        private List<Command> mCompilationCommands = new List<Command>();

        public Compiler(Project p, Debugger d)
        {
            mProject = p;
            mDebugger = d;
        }

        public void Build(string boardName, bool debug)
        {
            CreateTempDirectory();

            // Create list of files to compile, including system. 

            // if debug is enabled, go through the list of breakpoints and 
            // create modified files for them.

            // Modify main sketch file

            // Copy all project and core files to temp. We need to copy so that the right arduino_pins.h is used by project and core.

            

            
        }

        public void Clean()
        {

        }

        public void Deploy(string boardName, string programmerName)
        { 
            
        }
        

        private void ClearCommands()
        {
            mCompilationCommands.Clear();
        }

        private void RunQueuedCommands()
        { 
            
        }

        private void CreateCompilationCommands()
        { 
            // Compile files only if source is newer than existing .o file on destination
        }

        private void CreateLibraryCommands()
        { 
        
        }

        private void CreateLinkCommand()
        { 
        
        }

        private void CreateBinaryCommands()
        { 
            
        }

        private void CreateFinalSketchFile(bool debug)
        { 
            
            // Insert function headers and #include <Arduino.h>
            
            // If debug is defined:
            // - insert #include <soft_debugger.h> and 
            // - insert breakpoint and tracepoint calls in the needed places. 
            // - Add compilation of soft_debugger.s to the output
        }

        private void CopyProjectFilesToTemp(bool debug)
        { 
            
            
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

            return Path.Combine(tempDir, mProject.SketchFile.GetHashCode().ToString());
        }
    }



    public class BuildTarget
    {
        public string TargetFile;
        public string SourceFile;
        public bool NeedsCopy = false;

        public Command BuildCommand;

        public BuildTarget(string targetFile, string sourceFile)
        {
            TargetFile = targetFile;
            SourceFile = sourceFile;
        }

        public void CreateCommand()
        { 
            
        }
    }
    
}
