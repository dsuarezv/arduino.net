using System;
using System.Collections.Generic;


namespace arduino.net
{
    public class BreakPointManager
    {
        private List<BreakPointInfo> mBreakPoints = new List<BreakPointInfo>();
        private byte mNewBreakPointIndex = 10;

        public event BreakPointDelegate BreakPointAdded;
        public event BreakPointDelegate BreakPointRemoved;


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



        public BreakPointInfo Add(string sourceFile, int lineNumber)
        {
            var result = new BreakPointInfo() { LineNumber = lineNumber, SourceFileName = sourceFile, Id = mNewBreakPointIndex++ };

            mBreakPoints.Add(result);

            if (BreakPointAdded != null) BreakPointAdded(this, result);

            return result;
        }

        public void Remove(BreakPointInfo br)
        {
            if (!mBreakPoints.Remove(br)) return;

            if (BreakPointRemoved != null) BreakPointRemoved(this, br);
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
    }
}
