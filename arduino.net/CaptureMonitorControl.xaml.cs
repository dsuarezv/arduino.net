using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for CaptureMonitorControl.xaml
    /// </summary>
    public partial class CaptureMonitorControl : UserControl
    {
        //propdp
        public CapturePointInfo Target
        {
            get { return (CapturePointInfo)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); SetupTarget(value); }
        }


        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(CapturePointInfo), typeof(CaptureMonitorControl));


        public CaptureMonitorControl()
        {
            InitializeComponent();
        }

        private void SetupTarget(CapturePointInfo capture)
        { 
            if (capture == null) return;

            var serie = new LineSeries();
            serie.ItemsSource = capture.Values;

            var model = new PlotModel() { Title = capture.SymbolToTrace };
            model.Series.Add(serie);

            //MainPlot.Model = model;
        }
    }
}
