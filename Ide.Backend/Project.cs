using System;
using System.IO;
using System.Collections.Generic;


namespace arduino.net
{
    public class Project
    {
        private string mProjectPath;
        private string mSketchFile;

        private string mSdkPath;

        public Project(string sketchFile, string sdkPath = null)
        {
            mProjectPath = Path.GetDirectoryName(sketchFile);
            mSketchFile = Path.GetFileName(sketchFile);

            if (sdkPath == null) mSdkPath = Configuration.ToolkitPath;
        }

        public List<string> GetFileList()
        {
            List<string> result = new List<string>();

            var extensions = new string[] { "c", "cpp", "h", "hpp", "s", "ino" };

            foreach (var ext in extensions)
            {
                result.AddRange(Directory.GetFileSystemEntries(mProjectPath, "*." + ext));
            }

            return result;
        }

        public string GetFileContent(string fileName)
        { 
            using (var r = new StreamReader(Path.Combine(mProjectPath, fileName)))
            {
                return r.ReadToEnd();
            }
        }

        public void SaveFileContent(string fileName, string content)
        {
            using (var w = new StreamWriter(Path.Combine(mProjectPath, fileName)))
            {
                w.Write(content);
            }
        }

        public void AddNewFile(string fileName)
        {
            var f = File.CreateText(Path.Combine(mProjectPath, fileName));
            f.Close();
        }

        public void RemoveFile(string fileName)
        {
            File.Delete(Path.Combine(mProjectPath, fileName));
        }

        public void RenameFile(string fileName, string newFileName)
        {
            File.Move(Path.Combine(mProjectPath, fileName), Path.Combine(mProjectPath, newFileName));
        }
    }
}
