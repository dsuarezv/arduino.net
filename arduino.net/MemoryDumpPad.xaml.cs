using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class MemoryDumpPad : UserControl
    {
        public MemoryDumpPad()
        {
            InitializeComponent();
        }

        private void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            var debugger = IdeManager.Debugger;

            if (debugger.IsTargetRunning) return;

            Int16 address;
            Byte size;
            
            if (!Int16.TryParse(AddressTextBox.Text, out address)) return;
            if (!Byte.TryParse(SizeTextBox.Text, out size)) return;

            var result = debugger.GetTargetMemDump(address, size);

            PrintBuffer(result);
        }

        private void PrintBuffer(byte[] bytes)
        {
            var sb = new StringBuilder();



            ResultTextBlock.Text = sb.ToString();
        }
    }
}
