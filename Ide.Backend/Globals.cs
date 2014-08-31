using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace arduino.net
{
    public class Configuration
    {
        private static ConfigurationFile mBoards;
        private static ConfigurationFile mProgrammers;

        private static string mToolkitPath;

        public static void Initialize(string toolkitPath)
        {
            mToolkitPath = toolkitPath;

            try
            {
                var configPath = Path.Combine(toolkitPath, "hardware/arduino");

                mBoards = new ConfigurationFile(Path.Combine(configPath, "boards.txt")).Load();
                mProgrammers = new ConfigurationFile(Path.Combine(configPath, "programmers.txt")).Load();
            }
            catch
            { }
        }

        

        public static string GetBoardsKey(string key)
        {
            return GetKey(mBoards, key);
        }


        private static string GetKey(ConfigurationFile file, string key)
        {
            if (file == null) return null;

            var section = file.GetSection("");

            if (section == null) return null;

            return section.GetString(key);            
        }
    }

    public class ConfigurationFile
    {
        private string mFileName;
        private Dictionary<string, ConfigurationSection> mSections = new Dictionary<string, ConfigurationSection>();

        public ConfigurationSection this[string sectionName]
        {
            get { return mSections[sectionName]; }
        }

        public ConfigurationFile(string name)
        {
            mFileName = name;
        }

        public ConfigurationFile Load()
        {
            string line;
            ConfigurationSection section = AddSection("");

            using (var reader = new StreamReader(mFileName))
            { 
                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLine(ref section, line);
                }
            }

            return this;
        }

        private ConfigurationSection AddSection(string name)
        {
            ConfigurationSection result = new ConfigurationSection(name);
            mSections[name] = result;

            return result;
        }

        private void ProcessLine(ref ConfigurationSection section, string line)
        {
            if (line != null && line != "")
            {
                switch (line[0])
                {
                    case '[': ProcessSection(ref section, line); break;
                    case ';':
                    case '#':
                        break; // comment, will be overwritten, but ignore here
                    default: section.ParseEntry(line); break;
                }

            }
        }

        private void ProcessSection(ref ConfigurationSection section, string line)
        {
            line = line.TrimStart('[', ' ');
            line = line.TrimEnd(']', ' ');

            section = AddSection(line);
        }

        public void Save()
        {
            try
            {
                using (var writer = new StreamWriter(mFileName, false))
                { 
                    foreach (ConfigurationSection section in mSections.Values)
                    {
                        section.Write(writer);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Failed to store: " + ex.Message);
            }
        }

        public string GetEntryValue(string sectionName, string key)
        {
            ConfigurationSection section = mSections[sectionName] as ConfigurationSection;

            if (section != null)
            {
                return section[key.ToLower()] as string;
            }

            return "";
        }

        public void SetEntryValue(string sectionName, string key, string val)
        {
            ConfigurationSection section = mSections[sectionName] as ConfigurationSection;

            if (section == null)
            {
                section = new ConfigurationSection(sectionName);

                mSections[sectionName] = section;
            }

            section.Entries[key.ToLower()] = val;
        }

        public ConfigurationSection GetSection(string sectionName)
        {
            ConfigurationSection result = mSections[sectionName] as ConfigurationSection;

            if (result != null) return result;

            return AddSection(sectionName);
        }
    }

    public class ConfigurationSection
    {
        private Dictionary<string, string> mEntries = new Dictionary<string, string>();
        private string mSectionName;

        public Dictionary<string, string> Entries
        {
            get { return mEntries; }
        }

        public string this[string key]
        {
            get { return mEntries[key]; }
        }

        public string SectionName
        {
            get
            {
                return mSectionName;
            }
        }

        public ConfigurationSection(string sectionName)
        {
            mSectionName = sectionName;
        }

        public int GetInt(string name, int defaultValue)
        {
            string res = mEntries[name] as string;

            if (res != null)
            {
                return int.Parse(res);
            }

            return defaultValue;
        }

        public float GetFloat(string name, float defaultValue)
        {
            string res = mEntries[name] as string;

            if (res != null)
            {
                return float.Parse(res);
            }

            return defaultValue;
        }

        public void SetInt(string name, int val)
        {
            mEntries[name] = val.ToString();
        }

        public void SetFloat(string name, float val)
        {
            mEntries[name] = val.ToString();
        }

        public long GetLong(string name, long defaultValue)
        {
            string res = mEntries[name] as string;

            if (res != null)
            {
                return long.Parse(res);
            }

            return defaultValue;
        }

        public void SetLong(string name, long val)
        {
            mEntries[name] = val.ToString();
        }

        public string GetString(string name)
        {
            return mEntries[name] as string;
        }

        public void SetString(string name, string val)
        {
            mEntries[name] = val;
        }

        public bool GetBool(string name, bool defaultValue)
        {
            string res = mEntries[name] as string;

            if (res != null)
            {
                res = res.ToLower();

                return (res == "true" || res == "1");
            }

            return defaultValue;
        }

        public void SetBool(string name, bool val)
        {
            mEntries[name] = val ? "true" : "false";
        }

        public int[] GetInts(string name, int[] defaultValue)
        {
            string res = mEntries[name] as string;

            if (res == null || res == string.Empty) return defaultValue;

            int[] result = DeserializeInts(res);

            return result == null ? defaultValue : result;
        }
        public void SetInts(string name, int[] values)
        {
            if (values == null) mEntries[name] = null;

            mEntries[name] = SerializeWithCommas(values);
        }

        internal void ParseEntry(string entryLine)
        {
            if (entryLine != null && entryLine != "")
            {
                int split = entryLine.IndexOf('=');

                if (split > 0)
                {
                    string key = entryLine.Substring(0, split).Trim();

                    string val = "";

                    if (split < entryLine.Length - 1)
                    {
                        val = entryLine.Substring(split + 1).Trim();
                    }

                    mEntries[key] = val;
                }
            }
        }

        internal void Write(TextWriter writer)
        {
            if (mSectionName != "")
            {
                writer.WriteLine("[{0}]", mSectionName);
            }

            foreach (var entry in mEntries)
            {
                writer.WriteLine("{0}={1}", ((string)entry.Key).ToLower(), entry.Value);
            }

            writer.WriteLine();
        }

        public static string SerializeWithCommas(params object[] args)
        {
            bool isFirst = true;

            StringBuilder sb = new StringBuilder();

            foreach (object obj in args)
            {
                if (isFirst)
                {
                    sb.Append(obj.ToString());
                    isFirst = false;
                }
                else
                {
                    sb.Append("," + obj.ToString());
                }
            }

            return sb.ToString();
        }

        public static string SerializeGuidsWithCommas(params Guid[] args)
        {
            bool isFirst = true;

            StringBuilder sb = new StringBuilder();

            foreach (Guid guid in args)
            {
                long high = 0;
                long low = 0;
                GuidToLongs(guid, out high, out low);

                if (isFirst)
                {
                    sb.Append(high.ToString() + "&" + low.ToString());
                    isFirst = false;
                }
                else
                {
                    sb.Append("," + high.ToString() + "&" + low.ToString());
                }
            }

            return sb.ToString();
        }

        public static int[] DeserializeInts(string target)
        {
            string[] strings = target.Split(',');

            if (strings == null || strings.Length == 0) return null;

            int[] result = new int[strings.Length];

            for (int i = 0; i < strings.Length; ++i)
            {
                try
                {
                    result[i] = int.Parse(strings[i]);
                }
                catch
                {
                    // FIXME: localize
                    throw new Exception(string.Format("Failed to parse int value '{0}' in string '{1}'.", result[i], target));
                }
            }

            return result;
        }

        public static Guid[] DeserializeGuids(string target)
        {
            string[] strings = target.Split(',');

            if (strings == null || strings.Length == 0) return null;

            Guid[] result = new Guid[strings.Length];

            for (int i = 0; i < strings.Length; ++i)
            {
                try
                {
                    string[] longs = strings[i].Split('&');
                    result[i] = GuidFromLongs(
                        long.Parse(longs[0]),
                        long.Parse(longs[1]));
                }
                catch
                {
                    // FIXME: localize
                    throw new Exception(string.Format("Failed to parse int value '{0}' in string '{1}'.", result[i], target));
                }
            }

            return result;
        }

        private static void GuidToLongs(Guid guid, out long high, out long low)
        {
            byte[] bytes = guid.ToByteArray();
            high = BitConverter.ToInt64(bytes, 8);
            low = BitConverter.ToInt64(bytes, 0);
        }

        private static Guid GuidFromLongs(long high, long low)
        {
            byte[] guiddata = new byte[16];
            byte[] byteshigh = BitConverter.GetBytes(high);
            byte[] byteslow = BitConverter.GetBytes(low);

            Array.Copy(byteshigh, 0, guiddata, 8, byteshigh.Length);
            Array.Copy(byteslow, 0, guiddata, 0, byteshigh.Length);

            return new Guid(guiddata);
        }

    }

}