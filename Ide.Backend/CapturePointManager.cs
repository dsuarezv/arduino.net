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
        private ObservableCollection<CapturePointInfo> mCapturePoints = new ObservableCollection<CapturePointInfo>();

        public ObservableCollection<CapturePointInfo> CapturePoints
        {
            get { return mCapturePoints; }
        }

        public void Reset()
        {
            mCapturePoints.Clear();
        }

        public void RecordCaptures(IList<CaptureData> data)
        {
            // Dispatch a list of captures to their control points. 

            int lastCapturePointId = -1;
            CapturePointInfo point = null;

            foreach (var cd in data)
            {
                if (cd.Id != lastCapturePointId)
                { 
                    point = GetCapturePoint(cd.Id);
                    lastCapturePointId = cd.Id;
                }
                
                point.BeginBulkAdd();
                point.AddValue(cd);
            }

            foreach (var cp in mCapturePoints) cp.EndBulkAdd();
        }

        public void RecordCapture(CaptureData data)
        {
            GetCapturePoint(data.Id).AddValue(data);
        }

        private CapturePointInfo GetCapturePoint(int id)
        {
            foreach (var cp in mCapturePoints)
            {
                if (cp.Id == id) return cp;
            }

            // Not found, create new anonymous control point
            var newCp = new CapturePointInfo("Anonymous capture", id);
            mCapturePoints.Add(newCp);

            return newCp;
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
