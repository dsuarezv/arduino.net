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

        public void UpdateRegisters(RegisterManager regMan)
        {
            var sb = new StringBuilder();
            sb.Append("Registers:\n\n");

            var sorted = regMan.Registers.OrderBy(x =>
            {
                if (x.Key.StartsWith("r")) return string.Format("__{0}", x.Key);

                return x.Key;
            });

            foreach (var e in sorted)
            {
                sb.AppendFormat("  {0}:\t0x{1:X2}\t({1})\n", e.Key, e.Value);
            }

            RegistersTextbox.Text = sb.ToString();
        }
    }

}
