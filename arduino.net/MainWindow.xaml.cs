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
using System.Windows.Shell;
using AurelienRibon.Ui.SyntaxHighlightBox;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Remove the window border.
            //WindowChrome.SetWindowChrome(this, new WindowChrome());

            // Register syntax highlight XMLs in this assembly
            HighlighterManager.Instance.RegisterDefinitions(this.GetType().Assembly);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration.Initialize(@"C:\Program Files (x86)\Arduino");

                var sketch = @"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino";

                IdeManager.CurrentProject = new Project(sketch);
                IdeManager.Debugger = new Debugger("COM3");
                IdeManager.Compiler = new Compiler(IdeManager.CurrentProject, IdeManager.Debugger);

                SampleCodeBox.OpenFile(sketch);

                IdeManager.Debugger.AddBreakpoint(sketch, 38);

                StatusControl.SetState(0, "");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool ctrl = (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down) || (Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down);
            bool shift = (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down) || (Keyboard.GetKeyStates(Key.RightShift) == KeyStates.Down);


            switch (e.Key)
            {
                case Key.F5: break;
                case Key.B: if (ctrl && shift) LaunchBuild(); break;
            }

            base.OnPreviewKeyDown(e);
        }

        private void DisplayException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await LaunchBuild();
            
            if (success)
            { 
                StatusControl.SetState(0, "Build succeeded");
            }
            else
            {
                StatusControl.SetState(1, "Build failed");
            }
        }

        private void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            StatusControl.SetState(1, "Deploy failed");
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {

        }


        // __ Actions _________________________________________________________


        private async Task<bool> LaunchBuild()
        {
            OutputTextBox1.ClearText();
            bool result = await IdeManager.Compiler.BuildAsync("atmega328", true);
            return result;
        }
    }
}
