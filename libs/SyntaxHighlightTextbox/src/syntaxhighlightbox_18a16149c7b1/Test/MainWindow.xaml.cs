using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AurelienRibon.Ui.SyntaxHighlightBox;

namespace Test {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			box.CurrentHighlighter = HighlighterManager.Instance.Highlighters["assembler"];
		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var f = new StreamReader(@"C:\Users\dave\Documents\develop\Arduino\Debugger\soft_debugger.s"))
            {
                box.Text = f.ReadToEnd();
            }
        }
	}
}
