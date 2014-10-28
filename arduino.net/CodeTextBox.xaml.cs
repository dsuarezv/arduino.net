﻿using System;
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
                AutoIndent = Configuration.EditorAutoIndent,
                ReservedCountOfLineNumberChars = 5,
                BackColor = backColor,
                IndentBackColor = backColor,
                LineNumberColor = Color.FromArgb(180, 180, 180),
                HoveredWordRegex = @"[a-zA-Z0-9]"
            };

            mMainTextBox.PaintLine += mMainTextBox_PaintLine;
            mMainTextBox.KeyDown += mMainTextBox_KeyDown;
            mMainTextBox.KeyPress += mMainTextBox_KeyPress;
            mMainTextBox.TextChanged += mMainTextBox_TextChanged;
            mMainTextBox.TextSource.LineInserted += TextSource_LineInserted;
            mMainTextBox.TextSource.LineRemoved += TextSource_LineRemoved;

            mMainTextBox.ToolTipNeeded += mMainTextBox_ToolTipNeeded;

            
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
            mIsLoading = false;
        }

        public void OpenContent(string content, string highlightExt)
        {
            ApplySyntaxHighlight(highlightExt);

            mIsLoading = true;
            mMainTextBox.Text = content;
            mIsLoading = false;

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
            if (!mMainTextBox.IsChanged) return;

            mMainTextBox.SaveToFile(fileName, Encoding.UTF8);

            IdeManager.Compiler.MarkAsDirty(BuildStage.NeedsBuild);
        }

        public bool CloseFile()
        {
            if (!CheckChanges()) return false;

            mMainTextBox.Text = "";
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

        public void SetCursorAt(int lineNumber, int charNumber)
        {
            var s = mMainTextBox.Selection;

            s.Start = new Place(charNumber, lineNumber);
            s.End = s.Start;
        }

        public void FocusEditor()
        {
            mMainTextBox.Focus();
        }

        public void SetActiveLine(int lineNumber)
        {
            mActiveLine = lineNumber;

            mMainTextBox.Invalidate();
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
            return true;
        }

        private void mMainTextBox_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
        {
            if (string.IsNullOrEmpty(e.HoveredWord)) return;

            if (IdeManager.Debugger.Status == DebuggerStatus.Break)
            {
                var val = Watch.GetWatchValue(e.HoveredWord);

                e.ToolTipText = val;
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

        private void mMainTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (mSyntaxHighlighter == null) return;

            mSyntaxHighlighter(mMainTextBox, e);
        }

        private void DeleteBreakpointsInRange(int start, int numLines)
        {
            // Check if there is any breakpoint in the deleted range. Delete that breakpoint.

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



        private void mMainTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
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

        private void mMainTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!CanEdit()) e.Handled = true;
        }

        private bool CanEdit()
        {
            if (!mReadOnly) return true;
            
            MessageBox.Show("You can't change things while the debugger is running. If you stop the debugger, then you can edit the code and modify breakpoints.", 
                "Hey...", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            return false;
            
        }

        
        // __ Breakpoint handling _____________________________________________


        private void mMainTextBox_PaintLine(object sender, PaintLineEventArgs e)
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
