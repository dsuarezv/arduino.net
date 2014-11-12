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
using System.Windows.Navigation;
using System.Drawing;

using FastColoredTextBoxNS;
using System.Drawing.Text;
using Microsoft.Win32;

namespace arduino.net
{
    public partial class CodeTextBox : UserControl
    {
        private Brush C5Brush;
        private Brush C6Brush;
        private Brush C7Brush;
        private bool mReadOnly = false;
        private bool mIsLoading = false;
        private string mFileName;
        private int mActiveLine = -1;
        private Dictionary<int, BreakPointInfo> mBreakpoints = new Dictionary<int, BreakPointInfo>();
        private FastColoredTextBox mMainTextBox;
        private Action<FastColoredTextBox, FastColoredTextBoxNS.TextChangedEventArgs> mSyntaxHighlighter;
        

        public string FullFileName
        {
            get { return mFileName; }
        }

        public string FileName
        {
            get 
            {
                if (mFileName == null) return "New file";

                return Path.GetFileName(mFileName); 
            }
        }


        public CodeTextBox()
        {
            InitializeComponent();
            InitializeTextBox();
        }

        private void InitializeTextBox()
        {
            var backColor = UiConfig.GetWinformsColor(UiConfig.Background0);

            mMainTextBox = new FastColoredTextBox()
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Font = FontManager.GetSourceCodeFont(),
                AutoIndent = Configuration.Instance.EditorAutoIndent,
                ReservedCountOfLineNumberChars = 5,
                BackColor = backColor,
                IndentBackColor = backColor,
                LineNumberColor = Color.FromArgb(180, 180, 180),
                HoveredWordRegex = @"[a-zA-Z0-9]"
            };

            mMainTextBox.PaintLine += MainTextBox_PaintLine;
            mMainTextBox.KeyDown += MainTextBox_KeyDown;
            mMainTextBox.KeyPress += MainTextBox_KeyPress;
            mMainTextBox.KeyUp += mMainTextBox_KeyUp;
            mMainTextBox.TextChanged += MainTextBox_TextChanged;

            mMainTextBox.ToolTipNeeded += MainTextBox_ToolTipNeeded;
            
            C5Brush = new SolidBrush(UiConfig.GetWinformsColor(UiConfig.Color5));
            C6Brush = new SolidBrush(UiConfig.GetWinformsColor(UiConfig.Color6));
            C7Brush = new SolidBrush(UiConfig.GetWinformsColor(UiConfig.Color7));

            WFHost.Child = mMainTextBox;

            if (IdeManager.Debugger == null) return;

            IdeManager.Debugger.BreakPoints.BreakPointAdded += Debugger_BreakPointAdded;
            IdeManager.Debugger.BreakPoints.BreakPointRemoved += Debugger_BreakPointRemoved;
            IdeManager.Debugger.BreakPoints.BreakPointMoved += Debugger_BreakPointMoved;
        }

        
        public void OpenFile(string fileName)
        {
            if (!CheckChanges()) return;

            mFileName = fileName;

            ApplySyntaxHighlight(Path.GetExtension(mFileName));

            mIsLoading = true;
            mMainTextBox.OpenFile(fileName, Encoding.UTF8);
            mMainTextBox.TextSource.LineInserted += TextSource_LineInserted;
            mMainTextBox.TextSource.LineRemoved += TextSource_LineRemoved;

            mIsLoading = false;
        }

        public void OpenContent(string content, string highlightExt)
        {
            ApplySyntaxHighlight(highlightExt);

            mIsLoading = true;
            mMainTextBox.Text = content;
            mMainTextBox.TextSource.LineInserted += TextSource_LineInserted;
            mMainTextBox.TextSource.LineRemoved += TextSource_LineRemoved;

            mIsLoading = false;

            if (highlightExt == null) return;
        }

        public void SaveFile()
        {
            if (mFileName == null) 
            {
                var s = new SaveFileDialog()
                {
                    AddExtension = true,
                    Title = "Save as...",
                    DefaultExt = ".ino",
                    Filter = "Sketch files|*.ino;*.pde|All files|*.*"
                };

                if (!(bool)s.ShowDialog()) return;

                mFileName = s.FileName;
            }

            SaveFileAs(mFileName);
        }

        public void SaveFileAs(string fileName)
        {
            if (!mMainTextBox.IsChanged) return;

            mMainTextBox.SaveToFile(fileName, Encoding.UTF8);

            IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
        }

        public bool CloseFile()
        {
            if (!CheckChanges()) return false;

            mMainTextBox.Dispose();
            mMainTextBox = null;
            mFileName = null;

            return true;
        }

        public void Find(string text)
        { 
        
        }

        public void FindRegEx(string text)
        { 
            
        }

        public void FindAndReplace(string findText, string replaceText)
        { 
            
        }

        public void FindAndReplaceRegEx(string findExpre, string replaceExpr)
        { 
            
        }

        public void FocusEditor()
        {
            mMainTextBox.Focus();
        }

        public void SetActiveLine(int lineNumber)
        {
            mActiveLine = lineNumber;
            
            if (mActiveLine > -1)
            {
                mMainTextBox.Navigate(mActiveLine - 1);
            }
            else
            {
                mMainTextBox.Invalidate();
            }
        }

        public void ClearActiveLine()
        {
            SetActiveLine(-1);
        }

        public void SetReadOnly(bool readOnly)
        {
            mReadOnly = readOnly;

            mMainTextBox.ReadOnly = readOnly;
        }


        private bool CheckChanges()
        {
            // returns true is operation can proceed (either changes were saved or discarded)
            // or false if not (pending changes dialog was cancelled)

            if (!mMainTextBox.IsChanged) return true;

            var fileName = (mFileName == null) ? "File" : Path.GetFileName(mFileName);

            var r = MessageBox.Show(fileName + " has changes, do you want to save them?", "Save changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (r == MessageBoxResult.Yes)
            {
                SaveFile();
                return true;
            }
            else if (r == MessageBoxResult.No)
            {
                return true;
            }
           
            return false;
        }

        private void MainTextBox_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
        {
            if (string.IsNullOrEmpty(e.HoveredWord)) return;

            if (IdeManager.Debugger.Status == DebuggerStatus.Break)
            {
                var si = IdeManager.WatchManager.GetInmmediateValue(e.HoveredWord, IdeManager.Dwarf);
                if (si == null) return;

                e.ToolTipText = si.GetAsStringWithChildren();
            }
        }

        private void TextSource_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            if (mIsLoading) return;

            DeleteBreakpointsInRange(e.Index, e.Count);

            IdeManager.Debugger.BreakPoints.ShiftBreakpointsForFile(mFileName, e.Index, -e.Count);
        }

        private void TextSource_LineInserted(object sender, LineInsertedEventArgs e)
        {
            if (mIsLoading) return;

            IdeManager.Debugger.BreakPoints.ShiftBreakpointsForFile(mFileName, e.Index, e.Count);
        }

        private void MainTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (mSyntaxHighlighter == null) return;

            mSyntaxHighlighter(mMainTextBox, e);
        }

        private void DeleteBreakpointsInRange(int start, int numLines)
        {
            int startLine = start;
            int endLine = start + numLines;

            // Cannot remove while enumerating, so keep a list of breakpoints in the deleted range to remove them later.

            List<BreakPointInfo> brToRemove = new List<BreakPointInfo>();

            foreach (var br in mBreakpoints.Values)
            {
                if (br.LineNumber >= start && br.LineNumber <= endLine) brToRemove.Add(br);
            }

            foreach (var br in brToRemove) mBreakpoints.Remove(br.LineNumber);
        }

        private void ApplySyntaxHighlight(string ext)
        {
            if (ext == null) return;

            switch (ext.Trim('.').ToLower())
            {
                case "s": mSyntaxHighlighter = SyntaxHighlightApplier.Cpp;  break;
                case "disassembly": mSyntaxHighlighter = SyntaxHighlightApplier.Disassembly; break;
                case "c":
                case "cpp":
                case "h":
                case "hpp":
                case "pde":
                case "ino": mSyntaxHighlighter = SyntaxHighlightApplier.Cpp; break;
            }
        }


        // __ Keyboard shortcuts ______________________________________________


        private bool mMofidifierKeyPressed = false;


        private void MainTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            mMofidifierKeyPressed = e.Control || e.Alt;

            if (e.Control)
            { 
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.S: 
                        if (CanEdit()) SaveFile(); 
                        break;
                }
            }

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.F9: // Toggle breakpoint

                    if (CanEdit())
                    { 
                        var lineNumber = mMainTextBox.Selection.End.iLine + 1;

                        if (mBreakpoints.ContainsKey(lineNumber))
                        {
                            IdeManager.Debugger.BreakPoints.Remove(mBreakpoints[lineNumber]);
                        }
                        else
                        {
                            IdeManager.Debugger.BreakPoints.Add(mFileName, lineNumber);
                        }
                    }
                    
                    break;
            }
        }

        void mMainTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            mMofidifierKeyPressed = e.Control || e.Alt;
        }        

        private void MainTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (mMofidifierKeyPressed) return;

            if (!CanEdit()) e.Handled = true;
        }

        private bool CanEdit()
        {
            if (!mReadOnly) return true;
            
            MessageBox.Show("You can't change things while the debugger is running. If you stop the debugger, then you can edit the code and modify breakpoints.", 
                "Hey...", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            return false;
            
        }


        // __ Custom drawing __________________________________________________
        
        
        private void MainTextBox_PaintLine(object sender, PaintLineEventArgs e)
        {
            var re = e.LineRect;
            var l = e.LineIndex + 1;

            foreach (var br in mBreakpoints)
            {
                if (br.Key == l)
                {
                    bool isUpToDate = br.Value.IsDeployedOnDevice(IdeManager.Compiler);

                    var brush = isUpToDate ? C5Brush : C6Brush;
                    
                    e.Graphics.FillEllipse(brush, new Rectangle(2, re.Top + re.Height / 2 - 8, 15, 15));

                    break;
                }
            }

            if (mActiveLine == l)
            {
                const int xStart = 50;
                e.Graphics.FillRectangle(C7Brush, new Rectangle(xStart, re.Top, re.Width - xStart, re.Height));
            }
        }


        // __ Breakpoint handling _____________________________________________


        private void Debugger_BreakPointRemoved(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints.Remove(breakpoint.LineNumber);

            mMainTextBox.Invalidate();
        }

        private void Debugger_BreakPointAdded(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints[breakpoint.LineNumber] = breakpoint;
            
            mMainTextBox.Invalidate();
        }

        private void Debugger_BreakPointMoved(object sender, BreakPointInfo breakpoint, int oldLineNumber)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints.Remove(oldLineNumber);
            mBreakpoints[breakpoint.LineNumber] = breakpoint;

            mMainTextBox.Invalidate();
        }
    }
}
