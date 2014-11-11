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
        public static readonly string DefaultToolkitPath = Path.Combine(GetExecutablePath(), "../");


        private static ConfigSection mBaseConfig;
        private static ConfigSection mBoards;
        private static ConfigSection mProgrammers;


        public static event Func<string, string> PropertyValueRequired;


        public static string ToolkitPath
        {
            get { return CheckProperty("ToolkitPath", "editor", "toolkitpath"); }
            set { mBaseConfig.GetSection("editor")["toolkitpath"] = value; }
        }

        public static string SketchBookPath
        {
            get { return CheckProperty("SketchBookPath", "editor", "sketchbookfolder"); }
            set { mBaseConfig.GetSection("editor")["sketchbookfolder"] = value; }
        }


        public static string CurrentBoard
        {
            get { return CheckProperty("CurrentBoard", "target", "board"); }
            set { mBaseConfig.GetSection("target")["board"] = value; }
        }

        public static string CurrentProgrammer
        {
            get { return CheckProperty("CurrentProgrammer", "target", "programmer"); }
            set { mBaseConfig.GetSection("target")["programmer"] = value; }
        }

        public static string CurrentComPort
        {
            get { return CheckProperty("CurrentComPort", "target", "serialport"); }
            set { mBaseConfig.GetSection("target")["serialport"] = value; }
        }

        public static bool IsWindows
        {
            get { return true; }  // TODO: Implement for other platforms
        }

        public static string EditorFontName = "Consolas";
        public static float EditorFontSize = 11f;
        public static bool EditorAutoIndent = true;

        public static IList<string> LibraryPaths = new List<string>();

        public static string ToolsPath
        { 
            get { return Path.Combine(ToolkitPath, "hardware/tools/avr/bin/"); }
        }

        public static ConfigSection Boards
        {
            get { return mBoards; }
        }

        public static ConfigSection Programmers
        {
            get { return mProgrammers; }
        }

        public static void Initialize()
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

        public static void Save()
        {
            mBaseConfig.SaveToFile(GetPreferencesFile());
        }

        private static void AddNullProgrammer()
        {
            var s = mProgrammers.GetSection(NullProgrammerName);
            s["name"] = "Use built-in bootloader";
        }

        private static string GetPreferencesFile()
        {
            return Path.Combine(GetPreferencesDirectory(), "preferences.txt");
        }

        private static string GetPreferencesDirectory()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        private static string GetExecutablePath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string CheckProperty(string name, string section, string entry)
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

