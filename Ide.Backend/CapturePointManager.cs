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

        public void RecordCapture(CaptureData data)
        {
            foreach (var cp in mCapturePoints)
            { 
                if (cp.Id == data.Id)
                {
                    cp.AddValue(data);
                    return;
                }
            }

            // Not found, create new anonymous control point

            var newCp = new CapturePointInfo("Anonymous capture", data.Id);
            newCp.AddValue(data);

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
