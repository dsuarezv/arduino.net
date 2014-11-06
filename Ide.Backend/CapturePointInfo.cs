using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    [Serializable]
    public class CapturePointInfo : INotifyPropertyChanged 
    {
        private ObservableCollection<CaptureData> mValues = new ObservableCollection<CaptureData>();
        private string mSymbolToTrace;
        private string mName;
        private int mId;
        private CaptureData mLastValue;

        
        public event PropertyChangedEventHandler PropertyChanged;


        public int Id
        {
            get { return mId; }
            private set { mId = value; }
        }

        public ObservableCollection<CaptureData> Values 
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

        public void AddValue(int value)
        {
            var c = new CaptureData() { Value = value, TimeStamp = DateTime.Now };
            Values.Add(c);
            LastValue = c;
        }
        

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CaptureData
    {
        public int Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
