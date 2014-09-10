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
    /// Interaction logic for RegistersPad.xaml
    /// </summary>
    public partial class RegistersPad : UserControl
    {
        private const int NumberOfRegisters = 36;

        public RegistersPad()
        {
            InitializeComponent();
        }

        public void UpdateRegisters(byte[] registers)
        {
            var sb = new StringBuilder();
            sb.Append("Registers:\n\n");

            for (int i = 0; i < registers.Length; ++i)
            {
                sb.AppendFormat("  r{0}: {1}\n", i, registers[i]);
            }

            RegistersTextbox.Text = sb.ToString();
        }


    }
}
