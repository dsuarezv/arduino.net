using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace arduino.net
{
    public class PersistenceManager
    {
        private static string mFileName;

        public static void Initialize(string fileName)
        {
            mFileName = fileName;

            Load();
        }

        public static void Save()
        {
            if (mFileName == null) throw new Exception("Persistence manager not initialized.");

            using (var writer = new FileStream(mFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var f = new BinaryFormatter();
                f.Serialize(writer, IdeManager.Debugger.BreakPoints.BreakPoints);
            }
        }

        public static void Load()
        {
            if (mFileName == null) throw new Exception("Persistence manager not initialized.");

            try
            { 
                using (var reader = new FileStream(mFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var f = new BinaryFormatter();
                    var breakpoints = f.Deserialize(reader) as List<BreakPointInfo>;

                    foreach (var bi in breakpoints)
                    {
                        IdeManager.Debugger.BreakPoints.Add(bi);
                    }
                }
            }
            catch 
            { 
                // ignore. We simply do not restore any options.
            }
        }
    }
}
