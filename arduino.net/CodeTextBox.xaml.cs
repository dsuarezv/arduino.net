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

using FastColoredTextBoxNS;

namespace arduino.net
{
    public partial class CodeTextBox : UserControl
    {
        private string mFileName;
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
            mMainTextBox = new FastColoredTextBox()
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 11f),
                AutoIndent = true,
                ReservedCountOfLineNumberChars = 5,
            };

            mMainTextBox.PaintLine += mMainTextBox_PaintLine;
            mMainTextBox.KeyDown += mMainTextBox_KeyDown;
            mMainTextBox.TextChanged += mMainTextBox_TextChanged;
            mMainTextBox.ToolTipNeeded += mMainTextBox_ToolTipNeeded;


            
            WFHost.Child = mMainTextBox;

            if (IdeManager.Debugger == null) return;

            IdeManager.Debugger.BreakPoints.BreakPointAdded += Debugger_BreakPointAdded;
            IdeManager.Debugger.BreakPoints.BreakPointRemoved += Debugger_BreakPointRemoved;
        }

        
        
        public void OpenFile(string fileName)
        {
            if (!CheckChanges()) return;

            mFileName = fileName;

            ApplySyntaxHighlight(Path.GetExtension(mFileName));

            using (var f = new StreamReader(fileName))
            {
                mMainTextBox.Text = f.ReadToEnd();
            }

        }

        public void OpenContent(string content, string highlightExt)
        {
            ApplySyntaxHighlight(highlightExt);

            mMainTextBox.Text = content;

            if (highlightExt == null) return;

            
        }

        public void SaveFile()
        {
            if (mFileName == null) 
            {
                // Display a SaveAs dialog box and set the name to that.
                return;
            }

            SaveFileAs(mFileName);
        }

        public void SaveFileAs(string fileName)
        {
            using (var w = new StreamWriter(fileName))
            {
                w.Write(mMainTextBox.Text);
            }
        }

        public void CloseFile()
        {
            if (!CheckChanges()) return;

            mMainTextBox.Text = "";
            mFileName = null;
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

        public void SetCursorAt(int lineNumber, int charNumber)
        {
            var s = mMainTextBox.Selection;

            s.Start = new Place(charNumber, lineNumber);
            s.End = s.Start;
        }

        public void FocusEditor()
        {
            //WFHost.Focus();
            mMainTextBox.Focus();
        }


        private bool CheckChanges()
        {
            // returns true is operation can proceed (either changes were saved or discarded)
            // or false if not (pending changes dialog was cancelled)
            return true;
        }

        void mMainTextBox_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
        {
            if (string.IsNullOrEmpty(e.HoveredWord)) return;

            if (IdeManager.Debugger.Status == DebuggerState.Break)
            {
                var val = Watch.GetWatchValue(e.HoveredWord);

                e.ToolTipText = val;
            }
        }

        private void mMainTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (mSyntaxHighlighter == null) return;

            mSyntaxHighlighter(mMainTextBox, e);
        }

        private void ApplySyntaxHighlight(string ext)
        {
            if (ext == null) return;

            switch (ext.Trim('.').ToLower())
            {
                case "s": mSyntaxHighlighter = SyntaxHighlightApplier.Cpp;  break;
                    
                case "c":
                case "cpp":
                case "h":
                case "hpp":
                case "ino": mSyntaxHighlighter = SyntaxHighlightApplier.Cpp; break;
            }
        }


        // __ Keyboard shortcuts ______________________________________________



        void mMainTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control)
            { 
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.S: SaveFile(); break;
                }
            }

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.F9: // Toggle breakpoint
                    var lineNumber = mMainTextBox.Selection.End.iLine + 1;

                    if (mBreakpoints.ContainsKey(lineNumber))
                    {
                        IdeManager.Debugger.BreakPoints.Remove(mBreakpoints[lineNumber]);
                    }
                    else
                    {
                        IdeManager.Debugger.BreakPoints.Add(mFileName, lineNumber);
                    }
                    break;
            }
        }

        
        // __ Breakpoint handling _____________________________________________


        void mMainTextBox_PaintLine(object sender, PaintLineEventArgs e)
        {
            foreach (var i in mBreakpoints)
            {
                if (i.Key == e.LineIndex + 1)
                {
                    bool isUpToDate = i.Value.IsDeployedOnDevice(IdeManager.Compiler);

                    var brush = isUpToDate ? System.Drawing.Brushes.Red : System.Drawing.Brushes.Orange;

                    e.Graphics.FillEllipse(brush, new System.Drawing.Rectangle(0, e.LineRect.Top, 15, 15));
                    return;
                }
            }

            // TODO: add some code to draw the current line where program is stopped at.
        }

        
        void Debugger_BreakPointRemoved(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints.Remove(breakpoint.LineNumber);

            mMainTextBox.Invalidate();
        }

        void Debugger_BreakPointAdded(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints[breakpoint.LineNumber] = breakpoint;
            
            mMainTextBox.Invalidate();
        }
    }
}
