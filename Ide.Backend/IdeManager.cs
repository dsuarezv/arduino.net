using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class IdeManager
    {
        public static event Action<string, int> GoToLineRequested;


        public static Project CurrentProject;
        public static Debugger Debugger;
        public static Compiler Compiler;
        public static DwarfTree Dwarf;
        public static WatchManager WatchManager;
        public static CapturePointManager CapturePointManager;


        public static void Reset()
        {
            if (Debugger != null) Debugger.Reset();
            if (WatchManager != null) WatchManager.Reset();
            if (CapturePointManager != null) CapturePointManager.Reset();
        }

        public static void GoToFileAndLine(string fileName, int lineNumber)
        {
            if (GoToLineRequested == null) return;

            GoToLineRequested(fileName, lineNumber);
        }
    }
}
