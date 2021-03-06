﻿using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using WPFFolderBrowser;


namespace arduino.net
{
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
                Configuration.Instance.PropertyValueRequired += Configuration_PropertyValueRequired;

                CaptureMonitorFactory.RegisterCaptureAssembly(@"Ide.Wpf.DefaultCaptures.dll");

                ProjectPad1.TargetTabControl = OpenFilesTab;

                IdeManager.CapturePointManager = new CapturePointManager();
                IdeManager.Debugger = new Debugger();
                IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
                IdeManager.Debugger.TargetConnected += Debugger_TargetConnected;
                IdeManager.Debugger.StatusChanged += Debugger_StatusChanged;
                IdeManager.WatchManager = new WatchManager(IdeManager.Debugger);

                LoadLastProject();
                
                Task.Factory.StartNew(Debugger_SerialCharWorker);
                Task.Factory.StartNew(Debugger_CapturesWorker);
                
                UpdateBoardUi();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }


        private void LoadLastProject()
        {
            var lastProject = Configuration.Instance.LastProject;

            if (lastProject == null)
                CreateEmptyProject();
            else
                OpenProject(lastProject);
        }

        private string Configuration_PropertyValueRequired(string propertyName)
        {
            string result = null;

            switch (propertyName)
            {
                case "SketchBookPath":
                    var d = new WPFFolderBrowserDialog()
                    {
                        Title = "Select sketchbook folder"
                    };

                    if ((bool)d.ShowDialog())
                    {
                        return d.FileName;
                    }

                    break;
                case "CurrentBoard": 
                    
                    break;
            }

            return result;
        }

        private void CreateEmptyProject()
        {
            // TODO: show initial project wizard that lets the user choose the new project directory.
            ProjectPad1.OpenProject(Project.GetDefaultNewProjectFullName());
        }

        private void OpenProject(string sketch)
        {
            ProjectPad1.OpenProject(sketch);

            StatusControl.SetState(ActionStatus.Info, "Project", "Project loaded successfully");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ProjectPad1.CloseProject()) e.Cancel = true;

            Configuration.Instance.Save();
            SessionSettings.Save();
        }        

        protected async override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool ctrl = ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
            bool shift = ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);

            switch (e.Key)
            {
                case Key.F5: RunButton_Click(null, null); break;
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

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (!RunButton.IsEnabled) return;

            switch (IdeManager.Debugger.Status)
            {
                case DebuggerStatus.Break:
                    ClearEditorActiveLine();
                    IdeManager.Debugger.Run();
                    break;

                case DebuggerStatus.Running:
                    break;

                case DebuggerStatus.Stopped:
                    if (Configuration.Instance.CheckRebuildBeforeRun)
                    {
                        if (IdeManager.Compiler.IsDirty)
                        {
                            var success = await LaunchDeploy();
                            if (!success) break;
                        }
                    }
                    else
                    {
                        UpdateDwarf();
                    }

                    if (IsDebugBuild())
                    {
                        IdeManager.Debugger.ComPort = Configuration.Instance.CurrentComPort;
                        IdeManager.Debugger.Run();
                    }
                    break;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IdeManager.Debugger.Stop();
            ClearEditorActiveLine();
        }

        private void SelectBoardButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBoard();
        }
        
        private void SelectSerialButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSerial();
        }

        private void SelectProgrammerButton_Click(object sender, RoutedEventArgs e)
        {
            SelectProgrammer();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new GlobalSettingsDialog();
            sw.ShowDialog();
        }

        private void ClearEditorActiveLine()
        {
            var dbg = IdeManager.Debugger;

            if (dbg.LastBreakpoint != null)
            {
                var editor = ProjectPad1.GetEditor(dbg.LastBreakpoint.SourceFileName);
                if (editor != null) editor.ClearActiveLine();
            }
        }

        private bool IsDebugBuild()
        {
            var c = DebuggerCheckbox.IsChecked;
            return (c ?? false);
        }


        // __ Board/Serial/Programmer ___________________________________________________


        private bool SelectSerial()
        {
            var c = Configuration.Instance.CurrentComPort;
            var ports = ComportAdapter.Get(IdeManager.Debugger.GetAvailableComPorts());
            var selected = GetConfigValue("Select serial port", ports, ref c, "img/comports");
            if (selected != null)
            {
                Configuration.Instance.CurrentComPort = c;
                IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsDeploy);
                UpdateBoardUi();
            }

            return selected != null;
        }

        private bool SelectProgrammer()
        {
            var c = Configuration.Instance.CurrentProgrammer;
            var selected = GetConfigValue("Select programmer", Configuration.Instance.Programmers, ref c, "img/programmers");
            if (selected != null) 
            {
                Configuration.Instance.CurrentProgrammer = c;
                IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsDeploy);
                UpdateBoardUi();
            }

            return selected != null;
        }

        private bool SelectBoard()
        {
            var c = Configuration.Instance.CurrentBoard;
            var selected = GetConfigValue("Select board", Configuration.Instance.Boards, ref c, "img/boards");
            if (selected != null) 
            {
                Configuration.Instance.CurrentBoard = c;
                IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
                UpdateBoardUi();
            }

            return selected != null;
        }

        private ConfigSection GetConfigValue(string title, ConfigSection masterSection, ref string current, string imagesDirectory)
        {
            var result = SelectionWindow.Show(this, title, masterSection.GetAllSections(), masterSection.GetSection(current), imagesDirectory);
            if (result == null) return null;

            current = result.Name;
            return result;
        }

        private void UpdateBoardUi()
        {
            var board = Configuration.Instance.Boards.GetSection(Configuration.Instance.CurrentBoard)["name"];
            var progr = Configuration.Instance.Programmers.GetSection(Configuration.Instance.CurrentProgrammer)["name"];
            var comport = Configuration.Instance.CurrentComPort;

            SelectBoardButton.Content = string.Format("Board: {0}", board ?? "None. Click to select");
            SelectProgrammerButton.Content = string.Format("Programmer: {0}", progr ?? "None (bootloader on serial port)");
            SelectSerialButton.Content = string.Format("Serial: {0}", comport ?? "None. Click to select");
        }

        private bool IsBoardConfigured()
        {
            if (Configuration.Instance.CurrentBoard != null) return true;

            MessageBox.Show("You have to configure the type of Arduino board you are using.", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return false;
        }

        private bool IsDeploymentConfigured()
        {
            if (Configuration.Instance.CurrentComPort != null || Configuration.Instance.CurrentProgrammer != null) return true;

            MessageBox.Show("You have to configure a deployment option, either bootloader through a serial port, or a programmer.", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return false;
        }

        private bool IsSerialConfigured()
        {
            if (Configuration.Instance.CurrentComPort != null) return true;

            MessageBox.Show("You have to configure the serial port that connects to your Arduino.", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return false;
        }


        // __ Actions _________________________________________________________


        private async Task<bool> LaunchBuild()
        {
            if (!IsBoardConfigured()) return false;

            ProjectPad1.ClearAllActiveLines();

            bool debug = IsDebugBuild();

            OutputTextBox1.ClearText();
            StatusControl.SetState(ActionStatus.InProgress, "Compiler", "Compiling...");

            ProjectPad1.SaveAllDocuments();

            var compiler = IdeManager.Compiler;
            bool result = await compiler.BuildAsync(Configuration.Instance.CurrentBoard, debug);

            if (result)
            {
                if (Configuration.Instance.ShowDisassembly) OpenDisassemblyAfterBuild();

                StatusControl.SetState(ActionStatus.OK, "Compiler", "Build succeeded");
            }
            else
            {
                StatusControl.SetState(ActionStatus.Fail, "Compiler", "Build failed");
            }

            return result;
        }

        private void OpenDisassemblyAfterBuild()
        {
            if (!IsDebugBuild()) return;

            var elfFile = IdeManager.Compiler.GetElfFile();

            ProjectPad1.OpenContent("Sketch dissasembly",
                ObjectDumper.GetSingleString(
                    ObjectDumper.GetDisassemblyWithSource(elfFile)), ".disassembly");

            var transformedFile = IdeManager.Compiler.GetSketchTransformedFile();
            if (!File.Exists(transformedFile)) return;

            using (var f = File.OpenText(transformedFile))
            {
                ProjectPad1.OpenContent("Transformed Sketch", f.ReadToEnd(), ".cpp");
            }

            //ProjectPad1.OpenContent("Symbol table",
            //    ObjectDumper.GetSingleString(
            //        ObjectDumper.GetNmSymbolTable(elfFile)), ".symboltable");
        }

        private async Task<bool> LaunchDeploy()
        {
            if (!IsDeploymentConfigured()) return false;

            bool buildSuccess = await LaunchBuild();

            if (!buildSuccess) return false;

            StatusControl.SetState(ActionStatus.InProgress, "Deploy", "Deploying...");
            bool success = await IdeManager.Compiler.DeployAsync(Configuration.Instance.CurrentBoard, Configuration.Instance.CurrentProgrammer, IsDebugBuild());

            if (success)
            {
                StatusControl.SetState(ActionStatus.OK, "Deploy", "Deploy succeeded");
            }
            else
            {
                StatusControl.SetState(ActionStatus.Fail, "Deploy", "Deploy failed");
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
            if (!IsSerialConfigured()) 
            {
                DebuggerCheckbox.IsChecked = false;
                return;
            }

            RunButton.IsEnabled = true;
            StatusControl.SetState(ActionStatus.Info, "Debugger", "Debugger enabled. Set breakpoints in the code with F9 and hit 'Run' when ready.");
            DebuggerCheckbox_CheckedChanged();
        }

        private void DebuggerCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            StatusControl.SetState(ActionStatus.Info, "Debugger", "Debugger Disabled.");
            DebuggerCheckbox_CheckedChanged();
        }

        private void DebuggerCheckbox_CheckedChanged()
        {
            IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
            
            if (Configuration.Instance.CheckRebuildBeforeRun) IdeManager.Compiler.Clean();
        }

        private void Debugger_StatusChanged(object sender, DebuggerStatus newState)
        {
            Dispatcher.Invoke( () =>
            {
                switch (newState)
                { 
                    case DebuggerStatus.Stopped:
                        ProjectPad1.SetAllDocumentsReadOnly(false);
                        BuildButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        RunButton.IsEnabled = true;
                        DeployButton.IsEnabled = true;
                        DebuggerCheckbox.IsEnabled = true;
                        StatusControl.SetState(ActionStatus.Info, "Debugger", "Debugger dettached from Arduino.");
                        break;
                    case DebuggerStatus.Running:
                        ProjectPad1.SetAllDocumentsReadOnly(true);
                        BuildButton.IsEnabled = false;
                        StopButton.IsEnabled = true;
                        RunButton.IsEnabled = false;
                        DeployButton.IsEnabled = false;
                        DebuggerCheckbox.IsEnabled = false;
                        StatusControl.SetState(ActionStatus.Info, "Debugger", "Arduino running...");
                        break;
                    case DebuggerStatus.Break:
                        ProjectPad1.SetAllDocumentsReadOnly(true);
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
            Dispatcher.Invoke(() =>
            {
                if (bi == null)
                {
                    StatusControl.SetState(ActionStatus.Fail, "Debugger", "Unknown breakpoint hit (any orange breakpoints?). Target is stopped.\nStop the debugger and hit 'Deploy' to get your breakpoints in sync.");
                }
                else
                {
                    StatusControl.SetState(ActionStatus.Info, "Debugger", "Stopped at breakpoint. Hit 'Run' to continue.");

                    var editor = ProjectPad1.OpenFileAtLine(bi.SourceFileName, bi.LineNumber);
                }
            });

            UpdateDwarf();

            Dispatcher.Invoke(() =>
            {
                WatchesPad1.UpdateWatches();
            });
        }

        
        private void Debugger_CapturesWorker()
        {
            var queue = IdeManager.Debugger.ReceivedCapturesQueue;
            var buffer = new List<CaptureData>();

            try
            { 
                while(true)
                {
                    buffer.Clear();

                    while (!queue.IsEmpty)
                    {
                        CaptureData data;
                        if (!queue.TryDequeue(out data)) break;

                        buffer.Add(data);
                    }

                    if (buffer.Count > 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            EnsureCapturesPadIsvisible();

                            IdeManager.CapturePointManager.RecordCaptures(buffer);
                        });
                    }

                    Thread.Sleep(100);
                }
            }
            catch (OperationCanceledException)
            { }
        }

        private void Debugger_SerialCharWorker()
        {
            const int BufLen = 100;
            var queue = IdeManager.Debugger.ReceivedCharsQueue;
            var buffer = new char[BufLen];

            while (true)
            {
                int bufIdx = 0;

                while (!queue.IsEmpty && bufIdx < BufLen)
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

        private void EnsureCapturesPadIsvisible()
        {
            CapturesPadColumn.Width = new GridLength(CapturesPad.Width);
        }
    }
}
