using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using arduino.net;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace Ide.Wpf.DefaultCaptures
{
    /// <summary>
    /// Interaction logic for ChartCaptureMonitor.xaml
    /// </summary>
    public partial class ChartCaptureMonitor : UserControl, ICaptureMonitor
    {
        private PlotModel mModel;


        public int MaxNumberOfDataPoints { get; set; }


        public ChartCaptureMonitor()
        {
            MaxNumberOfDataPoints = 10;

            InitializeComponent();
        }

        public string MonitorName
        {
            get { return "Chart"; }
        }

        public void Setup(CapturePointInfo capture)
        {
            SetupModel();

            capture.Values.CollectionChanged += Values_CollectionChanged;
        }

        private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;

            var s = mModel.Series[0] as LineSeries;

            foreach (CaptureData cd in e.NewItems)
            {
                s.Points.Add(new DataPoint(DateTimeAxis.ToDouble(cd.TimeStamp), cd.Value));
            }

            int beyondMax = s.Points.Count - MaxNumberOfDataPoints;
            if (beyondMax > 0) s.Points.RemoveRange(0, beyondMax);

            mModel.InvalidatePlot(true);
        }

        private void SetupModel()
        {
            mModel = new PlotModel();
            mModel.Axes.Add(new DateTimeAxis() { Position = AxisPosition.Bottom });
            mModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            mModel.Series.Add(new LineSeries());

            MainPlot.Model = mModel;
        }
    }
}
