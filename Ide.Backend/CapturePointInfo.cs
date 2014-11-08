using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    [Serializable]
    public class CapturePointInfo : INotifyPropertyChanged 
    {
        [NonSerialized]
        private List<CaptureData> mValues = new List<CaptureData>();
        private int mBulkAddIndex = -1;
        private string mSymbolToTrace;
        private string mName;
        private int mId;
        private CaptureData mLastValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public event NewDataCaptureDelegate NewValuesAdded;


        public int Id
        {
            get { return mId; }
            private set { mId = value; }
        }

        public List<CaptureData> Values
        {
            get { return mValues; }
        }

        public string SymbolToTrace 
        { 
            get { return mSymbolToTrace; }
        }

        public string Name { 
            get { return mName; }
            set { mName = value; OnPropertyChanged("Name"); }
        }

        public CaptureData LastValue
        {
            get { return mLastValue; }
            set { mLastValue = value; OnPropertyChanged("LastValue"); }
        }        


        public CapturePointInfo(string symbolToTrace, int id)
        {
            mSymbolToTrace = symbolToTrace;
            mId = id;
        }

        public void AddValue(CaptureData c)
        {
            Values.Add(c);
            LastValue = c;

            if (mBulkAddIndex > -1 || NewValuesAdded == null) return;
            NewValuesAdded(this, Values.Count - 1, 1);
        }

        
        internal void BeginBulkAdd()
        {
            mBulkAddIndex = mValues.Count - 1;
        }

        internal void EndBulkAdd()
        {
            if (mBulkAddIndex >= 0 && NewValuesAdded != null)
            {
                int numNewItems = mValues.Count - 1 - mBulkAddIndex;

                NewValuesAdded(this, mBulkAddIndex, numNewItems);
            }

            mBulkAddIndex = -1;
        }
        

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public delegate void NewDataCaptureDelegate(object sender, int beginIndex, int numItems);


    public class CaptureData
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
