using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class CapturePointManager
    {
        private int mNewCapturePointIndex = 100;
        private Debugger mDebugger;
        private ObservableCollection<CapturePointInfo> mCapturePoints = new ObservableCollection<CapturePointInfo>();

        public ObservableCollection<CapturePointInfo> CapturePoints
        {
            get { return mCapturePoints; }
        }

        public void RecordCapture(int captureId, int value)
        {
            foreach (var cp in mCapturePoints)
            { 
                if (cp.Id == captureId)
                {
                    cp.AddValue(value);
                    return;
                }
            }

            // Not found, create new anonymous control point

            var newCp = new CapturePointInfo("", captureId);
            newCp.AddValue(value);

            mCapturePoints.Add(newCp);
        }

        private int GetNextCaptureId()
        {
            return mNewCapturePointIndex++;
        }

        private int GetMaxId(IList<CapturePointInfo> cps)
        {
            int max = -1;

            foreach (var cp in cps) 
                if (cp.Id > max) 
                    max = cp.Id;

            return max;
        }
    }
}
