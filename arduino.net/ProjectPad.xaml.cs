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

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for ProjectPad.xaml
    /// </summary>
    public partial class ProjectPad : UserControl
    {
        private TabControl mTabControl;


        public TabControl TargetTabControl
        {
            get { return MainTabSelector.TargetTabControl; }
            set 
            {
                mTabControl = value;
                MainTabSelector.TargetTabControl = value; 
            }
        }

        public ProjectPad()
        {
            InitializeComponent();

            IdeManager.GoToLineRequested += IdeManager_GoToLineRequested;
        }


        // __ Action buttons __________________________________________________


        private void NewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddLibrarybutton_Click(object sender, RoutedEventArgs e)
        {

        }


        // __ Project management ______________________________________________


        public void OpenProject(string sketchFile)
        {
            if (IdeManager.CurrentProject != null)
            {
                CloseAllProjectFiles();
            }

            Project p = new Project(sketchFile);

            IdeManager.CurrentProject = p;
            IdeManager.Compiler = new Compiler(p, IdeManager.Debugger);
            
            OpenAllProjectFiles();

            SessionSettings.Initialize(p.GetSettingsFileName());
        }

        public void CloseProject()
        {
            CloseAllProjectFiles();
        }


        private async void OpenAllProjectFiles()
        {
            foreach (var f in IdeManager.CurrentProject.GetFileList()) OpenFile(f);

            await OpenFile(IdeManager.CurrentProject.GetSketchFileName());
        }

        private void CloseAllProjectFiles()
        {
            foreach (var f in IdeManager.CurrentProject.GetFileList()) CloseFile(f);
        }


        // __ Document management _____________________________________________


        private void CloseFile(string fileName)
        {
            var ti = GetTabForFileName(fileName);
            if (ti == null) return;

            var editor = ti.Content as CodeTextBox;
            if (editor == null) return;

            editor.CloseFile();

            mTabControl.Items.Remove(ti);
        }


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

        public void OpenContent(string title, string content, string ext = null)
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

            editor.OpenContent(content, ext);
        }

        private CodeTextBox CreateEditorTabItem(string fileName)
        {
            var codeEditor = new CodeTextBox() { Padding = new Thickness(0, 5, 0, 5) };

            TabItem t = new TabItem() { Header = System.IO.Path.GetFileName(fileName), Tag = fileName, Content = codeEditor, Visibility = Visibility.Collapsed };

            mTabControl.Items.Add(t);

            t.IsSelected = true;

            return codeEditor;
        }

        private TabItem GetTabForFileName(string fileName)
        {
            foreach (TabItem ti in mTabControl.Items)
            {
                if (ti.Tag as string == fileName) return ti;
            }

            return null;
        }

        private void RunOnAllEditors(Action<CodeTextBox> editorAction)
        {
            foreach (TabItem ti in mTabControl.Items)
            {
                var editor = ti.Content as CodeTextBox;
                if (editor == null) continue;

                editorAction(editor);
            }
        }

        public void ClearAllActiveLines()
        {
            RunOnAllEditors((e) => e.ClearActiveLine());
        }

        public void SaveAllDocuments()
        {
            RunOnAllEditors((e) => e.SaveFile());
        }

        public void SetAllDocumentsReadOnly(bool readOnly)
        {
            RunOnAllEditors((e) => e.SetReadOnly(readOnly));
        }

        private void IdeManager_GoToLineRequested(string fileName, int lineNumber)
        {
            OpenFileAtLine(fileName, lineNumber);
        }

        public CodeTextBox GetEditor(string fileName)
        {
            var tab = GetTabForFileName(fileName);
            if (tab == null) return null;

            tab.IsSelected = true;

            var editor = tab.Content as CodeTextBox;
            if (editor == null) return null;

            return editor;
        }

        public CodeTextBox OpenFileAtLine(string fileName, int lineNumber)
        {
            var editor = GetEditor(fileName);
            if (editor == null) return null;

            editor.SetCursorAt(lineNumber - 1, 0);
            editor.FocusEditor();
            editor.SetActiveLine(lineNumber);

            return editor;
        }
    }
}
