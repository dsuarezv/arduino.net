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

namespace arduino.net.CaptureMonitors
{
    /// <summary>
    /// Interaction logic for BasicCaptureMonitor.xaml
    /// </summary>
    public partial class BasicCaptureMonitor : UserControl, ICaptureMonitor
    {
        public BasicCaptureMonitor()
        {
            InitializeComponent();
        }

        public string MonitorName
        {
            get { return "Basic"; }
        }

        public void Setup(CapturePointInfo capture)
        {
            DataContext = capture;
        }
    }
}
