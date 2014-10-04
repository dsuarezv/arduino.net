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
    /// Interaction logic for WatchesPad.xaml
    /// </summary>
    public partial class WatchesPad : UserControl
    {


        public WatchesPad()
        {
            InitializeComponent();

            IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Debugger_BreakPointHit(object sender, BreakPointInfo breakpoint)
        {
            UpdateWatches();
        }


        private void UpdateWatches()
        {
            Dispatcher.Invoke(() =>
            {
                //var t = MemDumpPad1.ResultTextBlock;
                //t.Text = "";
                //t.Text += GetWatchValue("myGlobalVariable");
                //t.Text += GetWatchValue("mylocal");
                //t.Text += GetWatchValue("result");
            });
        }

        private string GetWatchValue(string symbolName)
        {
            var pc = IdeManager.Debugger.Registers.Registers["PC"];
            var function = IdeManager.Dwarf.GetFunctionAt(pc);
            if (function == null) return symbolName + ": <current context not found>\n";

            return GetWatchValue(function, symbolName);
        }

        private string GetWatchValue(string functionName, string symbolName)
        {
            var function = IdeManager.Dwarf.GetFunctionByName(functionName);
            if (function == null) return symbolName + ": <context not found>\n";

            return GetWatchValue(function, symbolName);
        }

        private string GetWatchValue(DwarfSubprogram function, string symbolName)
        {
            var symbol = IdeManager.Dwarf.GetSymbol(symbolName, function);
            if (symbol == null) return symbolName + ": <not in current context>\n";

            var val = symbol.GetValue(IdeManager.Debugger);
            if (val == null) return symbolName + ": <symbol has no location debug information>\n";

            return string.Format("{0}: {1}\n", symbolName, symbol.GetValueRepresentation(IdeManager.Debugger, val));
        }
    }
}
