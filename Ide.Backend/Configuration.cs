using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;


namespace arduino.net
{
    public class Configuration
    {
        

        public const string AppName = "arduino.net";
        public const string NullProgrammerName = "none";
        public readonly string DefaultToolkitPath = Path.Combine(GetExecutablePath(), "../");


        private ConfigSection mBaseConfig;
        private ConfigSection mBoards;
        private ConfigSection mProgrammers;
        private static Configuration mInstance;

        
        public event Func<string, string> PropertyValueRequired;


        public static Configuration Instance
        {
            get
            {
                if (mInstance == null) mInstance = new Configuration();

                return mInstance;
            }
        }


        public bool VerboseBuildOutput { get; set; }

        public bool VerboseDeployOutput { get; set; }
        
        public bool VerifyCodeAfterUpload { get; set; }

        public bool CheckRebuildBeforeRun { get; set; }

        public bool ShowDisassembly { get; set; }

        public string ToolkitPath
        {
            get { return CheckProperty("ToolkitPath", "editor", "toolkitpath"); }
            set { mBaseConfig.GetSection("editor")["toolkitpath"] = value; }
        }

        public string SketchBookPath
        {
            get { return CheckProperty("SketchBookPath", "editor", "sketchbookfolder"); }
            set { mBaseConfig.GetSection("editor")["sketchbookfolder"] = value; }
        }

        public string LastProject
        {
            get { return GetProperty("editor", "lastproject"); }
            set { mBaseConfig.GetSection("editor")["lastproject"] = value; }
        }        

        public string CurrentBoard
        {
            get { return CheckProperty("CurrentBoard", "target", "board"); }
            set { mBaseConfig.GetSection("target")["board"] = value; }
        }

        public string CurrentProgrammer
        {
            get { return CheckProperty("CurrentProgrammer", "target", "programmer"); }
            set { mBaseConfig.GetSection("target")["programmer"] = value; }
        }

        public string CurrentComPort
        {
            get { return CheckProperty("CurrentComPort", "target", "serialport"); }
            set { mBaseConfig.GetSection("target")["serialport"] = value; }
        }

        public bool IsWindows
        {
            get { return true; }  // TODO: Implement for other platforms
        }


        public string EditorFontName = "Consolas";
        public float EditorFontSize = 11f;
        public bool EditorAutoIndent = true;

        public IList<string> LibraryPaths = new List<string>();

        public string ToolsPath
        { 
            get { return Path.Combine(ToolkitPath, "hardware/tools/avr/bin/"); }
        }

        public ConfigSection Boards
        {
            get { return mBoards; }
        }

        public ConfigSection Programmers
        {
            get { return mProgrammers; }
        }


        public Configuration()
        {
            mBaseConfig = ConfigSection.LoadFromFile(GetPreferencesFile());

            if (ToolkitPath == null) ToolkitPath = DefaultToolkitPath;

            LibraryPaths.Add(Path.Combine(ToolkitPath, "libraries"));
            LibraryPaths.Add(Path.Combine(SketchBookPath, "libraries"));

            var configPath = Path.Combine(ToolkitPath, "hardware/arduino");

            mBoards = ConfigSection.LoadFromFile(Path.Combine(configPath, "boards.txt"));
            mProgrammers = ConfigSection.LoadFromFile(Path.Combine(configPath, "programmers.txt"));

            AddNullProgrammer();
        }


        public void Save()
        {
            mBaseConfig.SaveToFile(GetPreferencesFile());
        }

        private void AddNullProgrammer()
        {
            var s = mProgrammers.GetSection(NullProgrammerName);
            s["name"] = "Use built-in bootloader";
        }

        private string GetPreferencesFile()
        {
            return Path.Combine(GetPreferencesDirectory(), "preferences.txt");
        }

        private string GetPreferencesDirectory()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        private static string GetExecutablePath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private string GetProperty(string section, string entry)
        {
            var val = mBaseConfig.GetSection(section)[entry];
            return val;
        }

        private string CheckProperty(string name, string section, string entry)
        {
            var val = mBaseConfig.GetSection(section)[entry];

            if (val == null)
            {
                if (PropertyValueRequired != null) 
                {
                    val = PropertyValueRequired(name);
                    if (val != null) mBaseConfig.GetSection(section)[entry] = val;
                }
            }

            return val;
        }
    }
}

