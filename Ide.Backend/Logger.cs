using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class Logger
    {
        private static Action<int, string> mListener;

        public static void RegisterListener(Action<int, string> listener)
        {
            mListener = listener;
        }

        public static void LogDirect(int logTarget, string msg)
        {
            if (mListener != null)
            {
                mListener(logTarget, msg);
            }
            else
            {
                Console.WriteLine(msg);
            }            
        }

        public static void Log(int logTarget, string msg, params object[] args)
        {
            string text = string.Format(msg, args);

            if (mListener != null)
            {
                mListener(logTarget, string.Format(msg, args));
            }
            else
            {
                Console.WriteLine(text);
            }
        }

        public static void LogCompilerDirect(string msg)
        {
            LogDirect(1, msg);
        }

        public static void LogCompiler(string msg, params object[] args)
        {
            Log(1, msg, args);
        }

        public static void LogDebugger(string msg, params object[] args)
        {
            Log(2, msg, args);
        }
    }
}
