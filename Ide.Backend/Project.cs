using System;
using System.IO;
using System.Collections.Generic;


namespace arduino.net
{
    public class Project
    {
        private const string IdeSettingsExtension = ".ide_settings.user";
        private const string DefaultSketchTemplateFile = "templates/default.ino";

        private string mProjectPath;
        private string mSketchFile;

        private string mSdkPath;

        public string SketchFile
        {
            get { return mSketchFile; }
        }

        public string ProjectPath
        {
            get { return mProjectPath; }
        }

        public Project(string sketchFile, string sdkPath = null)
        {
            mProjectPath = Path.GetDirectoryName(sketchFile);
            mSketchFile = Path.GetFileName(sketchFile);

            if (sdkPath == null) mSdkPath = Configuration.ToolkitPath;

            if (!Directory.Exists(mProjectPath)) Directory.CreateDirectory(mProjectPath);
            if (!File.Exists(sketchFile)) CreateDefaultSketch(sketchFile);
        }

        public List<string> GetFileList()
        {
            return GetCodeFilesOnPath(mProjectPath);
        }

        public static List<string> GetCodeFilesOnPath(string path)
        {
            List<string> result = new List<string>();

            var extensions = new string[] { "h", "hpp", "s", "c", "cpp", "ino", "pde" };

            foreach (var ext in extensions)
            {
                result.AddRange(Directory.GetFileSystemEntries(path, "*." + ext, SearchOption.TopDirectoryOnly));
            }

            return result;            
        }

        public string GetSketchFileName()
        {
            return Path.Combine(mProjectPath, mSketchFile);
        }
        
        public string GetSettingsFileName()
        {
            var fileName = Path.GetFileNameWithoutExtension(mSketchFile) + IdeSettingsExtension;
            return Path.Combine(mProjectPath, fileName);
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


        public static string GetNewProjectFile(string sketchFolder)
        {
            return Path.Combine(sketchFolder, Path.GetFileNameWithoutExtension(sketchFolder) + ".ino");
        }

        public static string GetDefaultNewProjectName()
        {
            var d = DateTime.Now;

            return string.Format("sketch_{0}{1}", d.ToString("MMM").ToLower(), d.Day);
        }

        private static void CreateDefaultSketch(string sketchFile)
        {
            File.Copy(DefaultSketchTemplateFile, sketchFile, false);
        }
    }
}
