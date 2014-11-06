using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    public class CapturePointInfo : DependencyObject
    {
        public int Id { get; private set; }

        public ObservableCollection<int> Values { get; private set; }

        public string SymbolToTrace { get; private set; }

        public string Name { get; set; }

        public int LastValue
        {
            get { return (int)GetValue(LastValueProperty); }
            set { SetValue(LastValueProperty, value); }
        }


        public static readonly DependencyProperty LastValueProperty = DependencyProperty.Register("LastValue", typeof(int), typeof(CapturePointInfo));


        public CapturePointInfo(string symbolToTrace, int id)
        {
            Values = new ObservableCollection<int>();
            SymbolToTrace = symbolToTrace;
            Id = id;
        }

        public void AddValue(int value)
        {
            Values.Add(value);
            LastValue = value;
        }
    }
}
