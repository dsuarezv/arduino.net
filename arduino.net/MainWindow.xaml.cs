using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration.Initialize(@"C:\Users\dave\Documents\develop\Arduino\ArduinoIDE.net\deploy");

                var sketch = @"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino";

                IdeManager.CurrentProject = new Project(sketch);
                IdeManager.Debugger = new Debugger("COM3");
                IdeManager.Compiler = new Compiler(IdeManager.CurrentProject, IdeManager.Debugger);
                IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
                IdeManager.Debugger.TargetConnected += Debugger_TargetConnected;
                IdeManager.GoToLineRequested += IdeManager_GoToLineRequested;
                //IdeManager.Debugger.SerialCharReceived += Debugger_SerialCharReceived;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Debugger_SerialCharWorker));


                foreach (var f in IdeManager.CurrentProject.GetFileList()) OpenFile(f);

                StatusControl.SetState(0, "");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        

        protected async override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool ctrl = ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
            bool shift = ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);

            switch (e.Key)
            {
                case Key.F5: break;
                case Key.B: if (ctrl && shift) await LaunchBuild(); break;
            }

            base.OnPreviewKeyDown(e);
        }

        private void DisplayException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            await LaunchBuild();            
        }

        private async void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await LaunchDeploy();
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            IdeManager.Debugger.TargetContinue();
            StatusControl.SetState(0, "Running...");
        }


        private void OpenFile(string fileName)
        {
            var ti = GetTabForFileName(fileName);

            CodeTextBox editor = null;

            if (ti != null)
            {
                ti.IsSelected = true;
                editor = ti.Content as CodeTextBox;
            }
            else
            {
                editor = CreateEditorTabItem(fileName);
            }

            editor.OpenFile(fileName);
        }

        private void OpenContent(string title, string content)
        {
            var ti = GetTabForFileName(title);

            CodeTextBox editor = null;

            if (ti != null)
            {
                ti.IsSelected = true;
                editor = ti.Content as CodeTextBox;
            }
            else
            {
                editor = CreateEditorTabItem(title);
            }
            
            editor.OpenContent(content, null);
        }

        private CodeTextBox CreateEditorTabItem(string fileName)
        {
            var codeEditor = new CodeTextBox() { Padding = new Thickness(0, 5, 0, 5) };

            TabItem t = new TabItem() { Header = System.IO.Path.GetFileName(fileName), Tag = fileName, Content = codeEditor };

            OpenFilesTab.Items.Add(t);

            t.IsSelected = true;

            return codeEditor;
        }


        private TabItem GetTabForFileName(string fileName)
        { 
            foreach (TabItem ti in OpenFilesTab.Items)
            {
                if (ti.Tag as string == fileName) return ti;
            }

            return null;
        }

        private void SaveAll()
        { 
            foreach (TabItem ti in OpenFilesTab.Items)
            {
                var editor = ti.Content as CodeTextBox;
                if (editor == null) continue;

                editor.SaveFile();
            }
        }


        // __ Actions _________________________________________________________


        private async Task<bool> LaunchBuild()
        {
            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Compiling...");

            SaveAll();

            bool result = await IdeManager.Compiler.BuildAsync("atmega328", true);
            
            var compiler = IdeManager.Compiler;
            var elfFile = compiler.GetElfFile(compiler.GetTempDirectory());
            
            OpenContent("Sketch dissasembly",
                ObjectDumper.GetSingleString(
                    ObjectDumper.GetDisassemblyWithSource(elfFile)));

            OpenContent("Symbol table",
                ObjectDumper.GetSingleString(
                    ObjectDumper.GetNmSymbolTable(elfFile)));

            InitDwarf();

            if (result)
            {
                StatusControl.SetState(0, "Build succeeded");
            }
            else
            {
                StatusControl.SetState(1, "Build failed");
            }

            return result;
        }

        private void InitDwarf()
        {
            if (IdeManager.Dwarf != null) return;
            var compiler = IdeManager.Compiler;
            var elfFile = compiler.GetElfFile(compiler.GetTempDirectory());
            IdeManager.Dwarf = new DwarfTree(new DwarfTextParser(elfFile));
        }

        private async Task<bool> LaunchDeploy()
        {
            bool buildSuccess = await LaunchBuild();

            if (!buildSuccess) return false;

            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Deploying...");
            bool success = await IdeManager.Compiler.DeployAsync("atmega328", "usbasp", true);

            if (success)
            {
                //    StatusControl.SetState(0, "Deploy succeeded");
                
            }
            //else
            //{
            //    StatusControl.SetState(1, "Deploy failed");
            //}

            InitDwarf();

            return success;
        }


        // __ Debugger events _________________________________________________


        private void Debugger_TargetConnected(object sender)
        {

        }

        private void Debugger_BreakPointHit(object sender, BreakPointInfo breakpoint)
        {
            Dispatcher.Invoke(() =>
            {
                RegistersPad.UpdateRegisters(IdeManager.Debugger.RegManager);

                if (breakpoint == null)
                {
                    StatusControl.SetState(1, "Unknown breakpoint hit. Target is stopped. Hit 'debug' to continue.");
                }
                else
                { 
                    StatusControl.SetState(1, "Breakpoint hit on line {0} ({1}). Hit 'debug' to continue.", 
                        breakpoint.LineNumber, System.IO.Path.GetFileName(breakpoint.SourceFileName));
                }
            });

            InitDwarf();
        }
        
        private void Debugger_SerialCharWorker(object state)
        {
            const int BufLen = 100;
            var queue = IdeManager.Debugger.ReceivedCharsQueue;
            var buffer = new char[BufLen];

            while (true)
            {
                int bufIdx = 0;

                while (queue.Count > 0 && bufIdx < BufLen)
                {
                    byte b;
                    if (!queue.TryDequeue(out b)) break;

                    buffer[bufIdx++] = (char)b;
                }

                if (bufIdx > 0)
                {
                    var s = new string(buffer, 0, bufIdx);

                    Dispatcher.Invoke(() =>
                    {
                        OutputTextBox1.AppendText(s, true);
                    });
                }

                Thread.Sleep(50);
            }

            
        }


        // __ Open file in given line _________________________________________


        private void IdeManager_GoToLineRequested(string fileName, int lineNumber)
        {
            OpenFileAtLine(fileName, lineNumber);
        }


        private void OpenFileAtLine(string fileName, int lineNumber)
        {
            var tab = GetTabForFileName(fileName);
            if (tab == null) return;

            tab.IsSelected = true;

            var editor = tab.Content as CodeTextBox;
            if (editor == null) return;

            editor.SetCursorAt(lineNumber, 0);
            editor.FocusEditor();
        }

    }
}
