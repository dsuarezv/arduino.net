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
                Configuration.Initialize(@"C:\Program Files (x86)\Arduino");

                var sketch = @"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino";

                IdeManager.CurrentProject = new Project(sketch);
                IdeManager.Debugger = new Debugger("COM3");
                IdeManager.Compiler = new Compiler(IdeManager.CurrentProject, IdeManager.Debugger);
                IdeManager.Debugger.BreakPointHit += Debugger_BreakPointHit;
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

        private async void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await LaunchDeploy();

            if (success)
            {
                StatusControl.SetState(0, "Deploy succeeded");
            }
            else
            {
                StatusControl.SetState(1, "Deploy failed");
            }
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            IdeManager.Debugger.TargetContinue();
            StatusControl.SetState(0, "Running...");
        }


        private void OpenFile(string fileName)
        {
            var ti = GetTabForFileName(fileName);

            if (ti != null)
            {
                ti.IsSelected = true;
                return;
            }

            var editor = CreateEditorTabItem(fileName);
            editor.OpenFile(fileName);
        }

        private void OpenContent(string title, string content)
        {
            var ti = GetTabForFileName(title);

            if (ti != null)
            {
                ti.IsSelected = true;
                return;
            }

            var editor = CreateEditorTabItem(title);
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
            OpenContent("Dissasembly",
                ObjectDumper.GetSingleString(
                    ObjectDumper.GetDisassembly(
                        compiler.GetElfFile(compiler.GetTempDirectory()))));

            return result;
        }

        private async Task<bool> LaunchDeploy()
        {
            OutputTextBox1.ClearText();
            StatusControl.SetState(1, "Deploying...");
            bool result = await IdeManager.Compiler.DeployAsync("atmega328", "usbasp", true);
            return result;
        }

        private void Debugger_BreakPointHit(object sender, BreakPointInfo breakpoint)
        {
            Dispatcher.Invoke(() =>
            {
                StatusControl.SetState(1, "Breakpoint hit on line {0} ({1})", breakpoint.LineNumber, breakpoint.SourceFileName);
            });
        }

        private void Debugger_SerialCharReceived(object sender, byte b)
        {
            
        }
    }
}
