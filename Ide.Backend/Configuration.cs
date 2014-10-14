using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace arduino.net
{
    public class Configuration
    {
        private static ConfigSection mBoards;
        private static ConfigSection mProgrammers;

        private static string mToolkitPath;


        public static string CurrentBoard = "atmega328";
        public static string CurrentProgrammer = "usbasp";
        public static string CurrentComPort;

        public static string EditorFontName = "Consolas";
        public static float EditorFontSize = 11f;
        public static bool EditorAutoIndent = true;


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
        }
    }
}

