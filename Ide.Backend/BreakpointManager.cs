using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


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

            if (BreakPointAdded != null) BreakPointAdded(this, bi);

            SessionSettings.Save();
        }

        public BreakPointInfo Add(string sourceFile, int lineNumber)
        {
            var result = new BreakPointInfo() { LineNumber = lineNumber, SourceFileName = sourceFile, Id = mNewBreakPointIndex++ };

            Add(result);

            return result;
        }

        public void Remove(BreakPointInfo br)
        {
            if (!mBreakPoints.Remove(br)) return;

            if (BreakPointRemoved != null) BreakPointRemoved(this, br);

            SessionSettings.Save();
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
            var breakpoints = obj as List<BreakPointInfo>;
            if (breakpoints == null) return;

            foreach (var bi in breakpoints) Add(bi);

            mNewBreakPointIndex = (byte)(GetMaxId(breakpoints) + 1);
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
