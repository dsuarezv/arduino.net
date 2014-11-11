using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;


namespace arduino.net
{
    public class BreakPointManager: IPersistenceListener
    {
        private List<BreakPointInfo> mBreakPoints = new List<BreakPointInfo>();
        private byte mNewBreakPointIndex = 10;

        public event BreakPointDelegate BreakPointAdded;
        public event BreakPointDelegate BreakPointRemoved;
        public event BreakpointMovedDelegate BreakPointMoved;


        internal List<BreakPointInfo> BreakPoints
        {
            get { return mBreakPoints; }
        }

        public int Count
        {
            get { return mBreakPoints.Count; }
        }

        public BreakPointInfo this[int breakpointId]
        {
            get 
            {
                foreach (var br in mBreakPoints)
                { 
                    if (br.Id == breakpointId) return br;
                }

                return null;
            }
        }

        public BreakPointManager()
        {
            SessionSettings.RegisterPersistenceListener(this);
        }

        internal void Add(BreakPointInfo bi)
        {
            mBreakPoints.Add(bi);

            IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
            TouchBreakpointSource(bi);

            if (BreakPointAdded != null) BreakPointAdded(this, bi);

            SessionSettings.Save();
        }

        public BreakPointInfo Add(string sourceFile, int lineNumber)
        {
            var result = new BreakPointInfo() { LineNumber = lineNumber, SourceFileName = sourceFile, Id = mNewBreakPointIndex++ };

            Add(result);

            return result;
        }

        public void Remove(BreakPointInfo bi)
        {
            if (!mBreakPoints.Remove(bi)) return;

            IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
            TouchBreakpointSource(bi);

            if (BreakPointRemoved != null) BreakPointRemoved(this, bi);

            SessionSettings.Save();
        }

        public void Clear()
        {
            mBreakPoints.Clear();
        }

        public List<BreakPointInfo> GetBreakpointsForFile(string fileName)
        {
            List<BreakPointInfo> result = new List<BreakPointInfo>();

            foreach (var br in mBreakPoints)
            {
                if (br.SourceFileName == fileName) result.Add(br);
            }

            return result;
        }

        public void ShiftBreakpointsForFile(string fileName, int fromLine, int shift)
        {
            foreach (var br in GetBreakpointsForFile(fileName))
            { 
                if (br.LineNumber >= fromLine)
                {
                    int oldLine = br.LineNumber;
                    br.LineNumber += shift;
                    br.LastEditDate = DateTime.Now;

                    IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);

                    if (BreakPointMoved != null) BreakPointMoved(this, br, oldLine);
                }
            }
        }

        object IPersistenceListener.GetObjectToPersist()
        {
            return mBreakPoints;
        }

        void IPersistenceListener.RestorePersistedObject(object obj)
        {
            mBreakPoints.Clear();

            var breakpoints = obj as List<BreakPointInfo>;
            if (breakpoints == null) return;

            foreach (var bi in breakpoints) Add(bi);

            mNewBreakPointIndex = (byte)(GetMaxId(breakpoints) + 1);
        }

        private void TouchBreakpointSource(BreakPointInfo bi)
        {
            File.SetLastWriteTime(bi.SourceFileName, DateTime.Now);
        }

        private int GetMaxId(List<BreakPointInfo> brs)
        {
            int max = -1;

            foreach (var br in brs) 
                if (br.Id > max) 
                    max = br.Id;

            return max;
        }
    }
}
