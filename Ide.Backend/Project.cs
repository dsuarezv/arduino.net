using System;
using System.IO;
using System.Collections.Generic;


namespace arduino.net
{
    public class Project
    {
        private const string IdeSettingsExtension = ".ide_settings.user";
        private const string DefaultSketchTemplateFile = "templates/default.ino";
        private const string DefaultSketchExtension = ".ino";
        private const string DefaultSketchNameTemplate = "sketch_{0}{1}";
        private static readonly string[] ProjectExtensions = new string[] { "h", "hpp", "s", "c", "cpp", "ino", "pde" };


        private string mProjectPath;
        private string mSketchFile;

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

            if (!Directory.Exists(mProjectPath)) Directory.CreateDirectory(mProjectPath);
            if (!File.Exists(sketchFile)) CreateDefaultSketch(sketchFile);
        }

        public IList<string> GetFileList()
        {
            return GetCodeFilesOnPath(mProjectPath);
        }

        public IList<string> GetFileListWithFullPaths()
        {
            List<string> result = new List<string>();

            foreach (var f in GetFileList())
            {
                result.Add(Path.Combine(mProjectPath, f));
            }

            return result;
        }

        public static IList<string> GetCodeFilesOnPath(string path)
        {
            List<string> result = new List<string>();

            foreach (var ext in ProjectExtensions)
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
            return Path.Combine(sketchFolder, Path.GetFileNameWithoutExtension(sketchFolder) + DefaultSketchExtension);
        }

        public static string GetDefaultNewProjectName()
        {
            var d = DateTime.Now;

            return string.Format(DefaultSketchNameTemplate, d.ToString("MMM").Trim('.').ToLower(), d.Day);
        }

        public static string GetDefaultNewProjectFullName()
        {
            var projectName = GetDefaultNewProjectName();
            var sketchFolder = Path.Combine(Configuration.SketchBookPath, projectName);

            return GetNewProjectFile(sketchFolder);
        }

        private static void CreateDefaultSketch(string sketchFile)
        {
            File.Copy(DefaultSketchTemplateFile, sketchFile, false);
        }
    }
}
