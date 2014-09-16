﻿using System;
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
                Configuration.Initialize(@"C:\Users\dave\Documents\develop\Arduino\ArduinoIDE.net\deploy");

                var sketch = @"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino";

                IdeManager.CurrentProject = new Project(sketch);
                IdeManager.Debugger = new Debugger("COM3");
                IdeManager.Compiler = new Compiler(IdeManager.CurrentProject, IdeManager.Debugger);
                IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
                IdeManager.Debugger.TargetConnected += Debugger_TargetConnected;
                IdeManager.Debugger.SerialCharReceived += Debugger_SerialCharReceived;

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
            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Deploying...");
            bool result = await IdeManager.Compiler.DeployAsync("atmega328", "usbasp", true);

            //if (success)
            //{
            //    StatusControl.SetState(0, "Deploy succeeded");
            //}
            //else
            //{
            //    StatusControl.SetState(1, "Deploy failed");
            //}

            InitDwarf();

            return result;
        }

        private void Debugger_BreakPointHit(object sender, BreakPointInfo breakpoint)
        {
            Dispatcher.Invoke(() =>
            {
                RegistersPad.UpdateRegisters(IdeManager.Debugger.Registers);

                if (breakpoint == null)
                {
                    StatusControl.SetState(1, "Unknown breakpoint hit. Target is stopped. Hit 'debug' to continue.");
                }
                else
                { 
                    StatusControl.SetState(1, "Breakpoint hit on line {0} ({1}). Hit 'debug' to continue.", breakpoint.LineNumber, breakpoint.SourceFileName);
                }
            });

            InitDwarf();

            UpdateWatches();
        }

        private void UpdateWatches()
        {
            Dispatcher.Invoke(() =>
            {
                var t = MemDumpPad1.ResultTextBlock;
                t.Text = "";
                t.Text += GetWatchValue("myfunc", "myGlobalVariable");
                t.Text += GetWatchValue("myfunc", "mylocal");
            });
        }

        private string GetWatchValue(string function, string symbol)
        {
            var di = IdeManager.Dwarf;
            var currentFunc = di.GetFunctionByName(function);
            var val = di.GetSymbolValue(symbol, currentFunc, IdeManager.Debugger);

            if (val == null) return symbol + ": <not found>\n";
            if (val.Length != 2) return symbol + ": <wrong size>\n";

            int value = (int)(val[1] << 8 | val[0]);
            var msg = string.Format("{0}: {1} (0x{1:X4})\n", symbol, value);
            return msg;
        }

        void Debugger_TargetConnected(object sender)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    OutputTextBox1.ClearText();
            //});
        }

        private void Debugger_SerialCharReceived(object sender, byte b)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    OutputTextBox1.ContentTextBox.AppendText(new string((char)b, 1));
            //    OutputTextBox1.ContentTextBox.ScrollToEnd();
            //});
            
        }
    }
}
