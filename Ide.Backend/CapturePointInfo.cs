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
        private ObservableCollection<int> mValues = new ObservableCollection<int>();
        private string mSymbolToTrace;
        private string mName;
        private int mId;
        private int mLastValue;

        
        public event PropertyChangedEventHandler PropertyChanged;


        public int Id
        {
            get { return mId; }
            private set { mId = value; }
        }

        public ObservableCollection<int> Values 
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

        public int LastValue
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
            Values.Add(value);
            LastValue = value;
        }
        

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
