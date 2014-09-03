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

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for OutputTextBox.xaml
    /// </summary>
    public partial class OutputTextBox : UserControl
    {
        public OutputTextBox()
        {
            InitializeComponent();

            Logger.RegisterListener((i, msg) =>
            {
                Dispatcher.Invoke(() => ContentTextBox.AppendText(msg + "\n"));
            });
        }

        public void ClearText()
        {
            ContentTextBox.Clear();
        }
    }
}
