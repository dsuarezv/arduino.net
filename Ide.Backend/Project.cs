using System;
using System.IO;


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


    }
}
