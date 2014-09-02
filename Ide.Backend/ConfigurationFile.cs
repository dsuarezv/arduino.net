using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class ConfigurationFile
    {
        private Dictionary<string, string> mEntries = new Dictionary<string, string>();
        private Dictionary<string, ConfigurationFile> mSections = new Dictionary<string, ConfigurationFile>();

        public ConfigurationFile this[string sectionName]
        {
            get { return mSections[sectionName]; }
        }

        public string Get(string key)
        {
            if (!mEntries.ContainsKey(key)) return null;

            return mEntries[key];
        }

        public static ConfigurationFile LoadFromFile(string fileName)
        {
            ConfigurationFile result = new ConfigurationFile();

            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLine(result, line);
                }
            }

            return result;
        }

        private static void ProcessLine(ConfigurationFile file, string line)
        {
            if (line == null || line == "" || IsComment(line)) return;

            var entry = GetKeyValue(line);
            if (entry == null) return;

            var sections = entry[0].Split('.');

            ConfigurationFile currentFile = file;

            for (int i = 0; i < sections.Length; ++i)
            {
                string val = sections[i];

                if (i == sections.Length - 1)
                {
                    currentFile.mEntries.Add(val, entry[1]);
                }
                else
                {
                    if (currentFile.mSections.ContainsKey(val))
                    {
                        currentFile = currentFile.mSections[val];
                    }
                    else
                    {
                        var newFile = new ConfigurationFile();
                        currentFile.mSections.Add(val, newFile);
                        currentFile = newFile;
                    }
                }
            }
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
