using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    public class CaptureMonitorFactory
    {
        private static Dictionary<string, Type> mMonitors = new Dictionary<string, Type>();

        
        public static IEnumerable<string> AvailableMonitorTypes
        {
            get { return mMonitors.Keys; }
        }


        public static void RegisterCaptureAssembly(string assemblyName)
        {
            try
            { 
                var a = Assembly.LoadFile(assemblyName);

                foreach (var c in a.GetTypes())
                {
                    RegisterCaptureMonitorControl(c);
                }
            }
            catch { }
        }

        public static void RegisterCaptureMonitorControl(Type captureMonitorClass)
        {
            if (!IsValidCapturerType(captureMonitorClass)) return;

            var instance = CreateInstance(captureMonitorClass);
            if (instance == null) return;

            mMonitors.Add(instance.MonitorName, captureMonitorClass);
        }

        public static ICaptureMonitor CreateInstance(string captureMonitorName)
        {
            if (!mMonitors.ContainsKey(captureMonitorName)) return null;

            return CreateInstance(mMonitors[captureMonitorName]);
        }

        private static ICaptureMonitor CreateInstance(Type capturerType)
        {
            var cons = capturerType.GetConstructor(new Type[] { });
            if (cons == null) return null;

            return cons.Invoke(new object[] { }) as ICaptureMonitor;
        }

        private static bool IsValidCapturerType(Type type)
        {
            return IsUiControl(type) && IsCaptureMonitor(type);
        }

        private static bool IsCaptureMonitor(Type type)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i == typeof(ICaptureMonitor)) return true;
            }

            return false;
        }

        private static bool IsUiControl(Type type)
        {
            return type.IsSubclassOf(typeof(UIElement));
        }
    }
}
