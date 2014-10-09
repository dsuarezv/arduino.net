using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace arduino.net
{
    public class SessionSettings
    {
        private static string mFileName;
        private static List<IPersistenceListener> mListeners = new List<IPersistenceListener>();

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

                foreach (var l in mListeners)
                {
                    f.Serialize(writer, l.GetObjectToPersist());
                }
            }
        }

        public static void Load()
        {
            if (mFileName == null) throw new Exception("Persistence manager not initialized.");

            if (!File.Exists(mFileName)) return;

            try
            { 
                using (var reader = new FileStream(mFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var f = new BinaryFormatter();

                    foreach (var l in mListeners)
                    {
                        l.RestorePersistedObject(f.Deserialize(reader));
                    }
                }
            }
            catch 
            { 
                // ignore. We simply do not restore any options.
            }
        }

        public static void RegisterPersistenceListener(IPersistenceListener listener)
        {
            mListeners.Add(listener);
        }
    }

    public interface IPersistenceListener
    {
        object GetObjectToPersist();
        void RestorePersistedObject(object obj);
    }
}
