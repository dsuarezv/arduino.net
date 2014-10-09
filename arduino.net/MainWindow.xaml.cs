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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration.Initialize(@"..\");

                //var sketch = @"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino";
                var sketch = @"C:\Users\dave\Documents\develop\ardupilot\ArduCopter\ArduCopter.pde";
                

                IdeManager.CurrentProject = new Project(sketch);
                IdeManager.Debugger = new Debugger("COM3");
                IdeManager.Compiler = new Compiler(IdeManager.CurrentProject, IdeManager.Debugger);
                IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
                IdeManager.Debugger.TargetConnected += Debugger_TargetConnected;
                IdeManager.Debugger.StatusChanged += Debugger_StatusChanged;
                IdeManager.GoToLineRequested += IdeManager_GoToLineRequested;
                
                ThreadPool.QueueUserWorkItem(new WaitCallback(Debugger_SerialCharWorker));

                SessionSettings.Initialize(IdeManager.CurrentProject.GetSettingsFileName());

                StatusControl.SetState(0, "");

                ProjectPad1.TargetTabControl = OpenFilesTab;

                OpenAllProjectFiles();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private async void OpenAllProjectFiles()
        {
            foreach (var f in IdeManager.CurrentProject.GetFileList()) await OpenFile(f);
            
            await OpenFile(IdeManager.CurrentProject.GetSketchFileName());
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


        // __ Action buttons __________________________________________________


        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await LaunchBuild();            
        }

        private async void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await LaunchDeploy();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            switch (IdeManager.Debugger.Status)
            {
                case DebuggerStatus.Break:
                    ClearEditorActiveLine();
                    IdeManager.Debugger.TargetContinue();
                    break;

                case DebuggerStatus.Running:
                    break;

                case DebuggerStatus.Stopped:
                    // Check for changes and build/deploy/run

                    //var success = await LaunchDeploy();

                    if (IsDebugBuild())
                    {
                        IdeManager.Debugger.Attach();
                        IdeManager.Debugger.TargetContinue();
                    }
                    break;
            }
        }


        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IdeManager.Debugger.Detach();
            ClearEditorActiveLine();
            StatusControl.SetState(0, "Debugger dettached.");
        }

        private void ClearEditorActiveLine()
        {
            var dbg = IdeManager.Debugger;

            if (dbg.LastBreakpoint != null)
            {
                var editor = GetEditor(dbg.LastBreakpoint.SourceFileName);
                if (editor != null) editor.ClearActiveLine();
            }
        }

        private bool IsDebugBuild()
        {
            var c = DebuggerCheckbox.IsChecked;
            return (c.HasValue ? c.Value : false);
        }


        // __ Actions _________________________________________________________


        private async Task<bool> LaunchBuild()
        {
            bool debug = IsDebugBuild();

            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Compiling...");

            SaveAllDocuments();

            var compiler = IdeManager.Compiler;
            bool result = await compiler.BuildAsync(Configuration.CurrentBoard, debug);
            var elfFile = compiler.GetElfFile();

            if (result)
            {
                if (debug)
                { 
                    OpenContent("Sketch dissasembly",
                        ObjectDumper.GetSingleString(
                            ObjectDumper.GetDisassemblyWithSource(elfFile)));

                    OpenContent("Symbol table",
                        ObjectDumper.GetSingleString(
                            ObjectDumper.GetNmSymbolTable(elfFile)));
                }

                StatusControl.SetState(0, "Build succeeded");
            }
            else
            {
                StatusControl.SetState(1, "Build failed");
            }

            return result;
        }

        private async Task<bool> LaunchDeploy()
        {
            bool buildSuccess = await LaunchBuild();

            if (!buildSuccess) return false;

            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Deploying...");
            bool success = await IdeManager.Compiler.DeployAsync(Configuration.CurrentBoard, Configuration.CurrentProgrammer, IsDebugBuild());

            if (success)
            {
                StatusControl.SetState(0, "Deploy succeeded");
            }
            else
            {
                StatusControl.SetState(1, "Deploy failed");
            }

            return success;
        }

        private void UpdateDwarf()
        {
            if (IdeManager.Dwarf != null) return;

            IdeManager.Compiler.BuildDwarf();
        }


        // __ Debugger events _________________________________________________


        private void DebuggerCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = true;
        }

        private void DebuggerCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
        }

        private void Debugger_StatusChanged(object sender, DebuggerStatus newState)
        {
            Dispatcher.Invoke( () =>
            {
                // Update UI, what buttons are enabled and disabled.
                switch (newState)
                { 
                    case DebuggerStatus.Stopped:
                        SetAllDocumentsReadOnly(false);
                        BuildButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        RunButton.IsEnabled = true;
                        DeployButton.IsEnabled = true;
                        DebuggerCheckbox.IsEnabled = true;
                        break;
                    case DebuggerStatus.Running:
                        SetAllDocumentsReadOnly(true);
                        BuildButton.IsEnabled = false;
                        StopButton.IsEnabled = true;
                        RunButton.IsEnabled = false;
                        DeployButton.IsEnabled = false;
                        DebuggerCheckbox.IsEnabled = false;
                        break;
                    case DebuggerStatus.Break:
                        SetAllDocumentsReadOnly(true);
                        BuildButton.IsEnabled = false;
                        RunButton.IsEnabled = true;
                        StopButton.IsEnabled = true;
                        DeployButton.IsEnabled = false;
                        DebuggerCheckbox.IsEnabled = false;
                        break;

                }
            });
        }

        private void Debugger_TargetConnected(object sender)
        {

        }

        private void Debugger_BreakPointHit(object sender, BreakPointInfo bi)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    RegistersPad.UpdateRegisters(IdeManager.Debugger.RegManager);

            //    if (bi == null)
            //    {
            //        StatusControl.SetState(1, "Unknown breakpoint hit. Target is stopped. Hit 'debug' to continue.");
            //    }
            //    else
            //    {
            //        StatusControl.SetState(0, "Stopped at breakpoint. Hit 'debug' to continue.");

            //        var editor = OpenFileAtLine(bi.SourceFileName, bi.LineNumber);
            //        if (editor != null) editor.SetActiveLine(bi.LineNumber);
            //    }
            //});

            UpdateDwarf();
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


        // __ Document management _____________________________________________


        private async Task OpenFile(string fileName)
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

            await editor.OpenFile(fileName);
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

            TabItem t = new TabItem() { Header = System.IO.Path.GetFileName(fileName), Tag = fileName, Content = codeEditor, Visibility = Visibility.Collapsed };

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

        private void RunOnAllEditors(Action<CodeTextBox> editorAction)
        {
            foreach (TabItem ti in OpenFilesTab.Items)
            {
                var editor = ti.Content as CodeTextBox;
                if (editor == null) continue;

                editorAction(editor);
            }
        }

        private void SaveAllDocuments()
        {
            RunOnAllEditors((e) => e.SaveFile());
        }

        private void SetAllDocumentsReadOnly(bool readOnly)
        {
            RunOnAllEditors((e) => e.SetReadOnly(readOnly));
        }

        private void IdeManager_GoToLineRequested(string fileName, int lineNumber)
        {
            OpenFileAtLine(fileName, lineNumber);
        }

        private CodeTextBox GetEditor(string fileName)
        {
            var tab = GetTabForFileName(fileName);
            if (tab == null) return null;

            tab.IsSelected = true;

            var editor = tab.Content as CodeTextBox;
            if (editor == null) return null;

            return editor;
        }

        private CodeTextBox OpenFileAtLine(string fileName, int lineNumber)
        {
            var editor = GetEditor(fileName);
            if (editor == null) return null;

            editor.SetCursorAt(lineNumber, 0);
            editor.FocusEditor();

            return editor;
        }
    }
}
