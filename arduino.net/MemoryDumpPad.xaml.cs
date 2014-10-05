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

            if (debugger.Status != DebuggerState.Break) return;

            int? address = ParseHexOrDecimal(AddressTextBox.Text);
            int? size = ParseHexOrDecimal(SizeTextBox.Text);

            if (address == null || size == null) return;

            var result = debugger.GetTargetMemDump((short)address, (byte)size);

            PrintBuffer((short)address, result);
        }

        private void PrintBuffer(short startAddress, byte[] bytes)
        {
            const int BytesPerLine = 1;

            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; ++i)
            {
                var address = startAddress + i;

                if (address % BytesPerLine == 0 && i != 0)
                {
                    sb.AppendFormat("\n0x{0:X4}:\t", address);
                }

                if (i == 0)
                {
                    sb.AppendFormat("0x{0:X4}:\t{1}", address, new string(' ', address % BytesPerLine * 3));
                }

                sb.AppendFormat("{0:X2} ", bytes[i]);
            }

            ResultTextBlock.Text = sb.ToString();
        }

        public static int? ParseHexOrDecimal(string number)
        { 
            // Parses a number in hex (0x000) or decimal (000)

            number = number.Trim().ToLower();

            int result;

            if (number.StartsWith("0x"))
            {
                if (!int.TryParse(number.Substring(2), NumberStyles.HexNumber, null, out result)) return null;
            }
            else
            {
                if (!int.TryParse(number, out result)) return null;
            }

            return result;

        }
    }
}
