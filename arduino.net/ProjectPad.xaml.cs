using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

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
            var dialog = new SaveFileDialog()
            {
                Title = "Choose new sketch folder...",
                Filter = "All files (*.*)|*.*",
                FileName = Project.GetDefaultNewProjectFullName()
            };

            if (!(bool)dialog.ShowDialog()) return;

            if (!CloseAllProjectFiles()) return;

            // Create project

            var projectFile = Project.GetNewProjectFile(dialog.FileName);

            OpenProject(projectFile);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Arduino sketch...",
                Multiselect = false,
                Filter = "Arduino sketches (*.ino, *.pde)|*.ino;*.pde"
            };

            if (!(bool)dialog.ShowDialog()) return;

            OpenProject(dialog.FileName);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
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
                if (!CloseAllProjectFiles()) return;
            }

            Project p = new Project(sketchFile);

            IdeManager.CurrentProject = p;
            IdeManager.Compiler = new Compiler(p, IdeManager.Debugger);
            
            OpenAllProjectFiles();

            SessionSettings.Initialize(p.GetSettingsFileName());
        }

        public bool CloseProject()
        {
            return CloseAllProjectFiles();
        }


        private void OpenAllProjectFiles()
        {
            foreach (var f in IdeManager.CurrentProject.GetFileList()) OpenFile(f);

            OpenFile(IdeManager.CurrentProject.GetSketchFileName());
        }

        private bool CloseAllProjectFiles()
        {
            bool result = true;

            foreach (var f in IdeManager.CurrentProject.GetFileList()) 
            {
                if (!CloseFile(f))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }


        // __ Document management _____________________________________________


        private void OpenFile(string fileName, bool reloadIfAlreadyOpen = false)
        {
            var ti = GetTabForFileName(fileName);

            CodeTextBox editor = null;

            if (ti != null)
            {
                ti.IsSelected = true;
                editor = ti.Content as CodeTextBox;
                if (!reloadIfAlreadyOpen) return;
            }
            else
            {
                editor = CreateEditorTabItem(fileName);
            }

            editor.OpenFile(fileName);
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

        private bool CloseFile(string fileName)
        {
            var ti = GetTabForFileName(fileName);
            if (ti == null) return true;

            var editor = ti.Content as CodeTextBox;
            if (editor == null) return true;

            var result = editor.CloseFile();
            if (result)
            {
                mTabControl.Items.Remove(ti);
            }

            return result;
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

            editor.FocusEditor();
            editor.SetActiveLine(lineNumber);

            return editor;
        }
    }
}
