using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class ConfigSection
    {
        private static readonly ConfigSection Empty = new ConfigSection();

        private Dictionary<string, string> mEntries = new Dictionary<string, string>();
        private Dictionary<string, ConfigSection> mSubSections = new Dictionary<string, ConfigSection>();

        public string Name { get; set; }

        public string this[string entryName]
        { 
            get
            {
                if (!mEntries.ContainsKey(entryName)) return null;

                return mEntries[entryName];
            }
            set
            {
                mEntries[entryName] = value;
            }
        }

        public ICollection<string> GetAllSectionNames()
        {
            return mSubSections.Keys;
        }

        public ICollection<ConfigSection> GetAllSections()
        {
            return mSubSections.Values;
        }

        public ConfigSection GetSub(string sectionName)
        {
            if (sectionName == null) return Empty;
                
            if (!mSubSections.ContainsKey(sectionName))
                mSubSections[sectionName] = new ConfigSection() { Name = sectionName };

            return mSubSections[sectionName]; 
        }
        
        public void SaveToFile(string fileName)
        { 
            using (var w = new StreamWriter(fileName))
            {
                SaveToFile(w, null);
            }
        }

        public void SaveToFile(TextWriter writer, string parentName)
        { 
            var name = (parentName == null) ? Name : parentName + "." + Name;

            foreach (var e in mEntries)
            {
                var entryName = (name == null) ? e.Key : name + "." + e.Key;

                SaveLine(writer, entryName, e.Value);
            }

            foreach (var s in mSubSections.Values)
            {
                s.SaveToFile(writer, name);
            }

            writer.WriteLine();
        }

        public static ConfigSection LoadFromFile(string fileName)
        {
            ConfigSection result = new ConfigSection();

            if (File.Exists(fileName))
            { 
                using (var reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ProcessLine(result, line);
                    }
                }
            }

            return result;
        }

        private static void ProcessLine(ConfigSection file, string line)
        {
            if (line == null || line == "" || IsComment(line)) return;

            var entry = GetKeyValue(line);
            if (entry == null) return;

            var sections = entry[0].Split('.');

            ConfigSection currentFile = file;

            for (int i = 0; i < sections.Length; ++i)
            {
                string val = sections[i];

                if (i == sections.Length - 1)
                {
                    currentFile.mEntries.Add(val, entry[1]);
                }
                else
                {
                    if (currentFile.mSubSections.ContainsKey(val))
                    {
                        currentFile = currentFile.mSubSections[val];
                    }
                    else
                    {
                        var newFile = new ConfigSection() { Name = val };
                        currentFile.mSubSections.Add(val, newFile);
                        currentFile = newFile;
                    }
                }
            }
        }

        private static void SaveLine(TextWriter writer, string key, string val)
        {
            writer.WriteLine(string.Format("{0}={1}", key, val));
        }

        private static bool IsComment(string line)
        {
            var c = line.Trim()[0];
            return (c == ';' || c == '#');
        }

        private static string[] GetKeyValue(string line)
        {
            int split = line.IndexOf('=');
            if (split == -1) return null;

            string key = line.Substring(0, split).Trim();
            string val = "";

            if (split < line.Length - 1)
            {
                val = line.Substring(split + 1).Trim();
            }

            return new string[] { key, val };
        }
    }
}
