using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace arduino.net
{
    public class Configuration
    {
        public const string AppName = "arduino.net";


        private static ConfigSection mBaseConfig;
        private static ConfigSection mBoards;
        private static ConfigSection mProgrammers;
        private static string mToolkitPath;


        public static string CurrentBoard
        {
            get { return mBaseConfig.GetSection("target")["board"]; }
            set { mBaseConfig.GetSection("target")["board"] = value; }
        }

        public static string CurrentProgrammer
        {
            get { return mBaseConfig.GetSection("target")["programmer"]; }
            set { mBaseConfig.GetSection("target")["programmer"] = value; }
        }

        public static string CurrentComPort
        {
            get { return mBaseConfig.GetSection("target")["serialport"]; }
            set { mBaseConfig.GetSection("target")["serialport"] = value; }
        }

        public static string EditorFontName = "Consolas";
        public static float EditorFontSize = 11f;
        public static bool EditorAutoIndent = true;

        public static IList<string> LibraryPaths = new string[] 
        {
            "",
            ""
        };


        public static string ToolkitPath
        {
            get { return mToolkitPath; }
        }

        public static string ToolsPath
        { 
            get { return Path.Combine(mToolkitPath, "hardware/tools/avr/bin/"); }
        }

        public static ConfigSection Boards
        {
            get { return mBoards; }
        }

        public static ConfigSection Programmers
        {
            get { return mProgrammers; }
        }

        public static void Initialize(string toolkitPath)
        {
            mToolkitPath = toolkitPath;

            var configPath = Path.Combine(toolkitPath, "hardware/arduino");

            mBoards = ConfigSection.LoadFromFile(Path.Combine(configPath, "boards.txt"));
            mProgrammers = ConfigSection.LoadFromFile(Path.Combine(configPath, "programmers.txt"));
            mBaseConfig = ConfigSection.LoadFromFile(GetPreferencesFile());
        }

        public static void Save()
        {
            mBaseConfig.SaveToFile(GetPreferencesFile());
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
    }
}

