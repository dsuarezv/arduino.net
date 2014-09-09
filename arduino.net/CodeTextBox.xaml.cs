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
using AurelienRibon.Ui.SyntaxHighlightBox;

namespace arduino.net
{
    public partial class CodeTextBox : UserControl
    {
        private string mFileName;
        private Dictionary<int, BreakPointInfo> mBreakpoints = new Dictionary<int, BreakPointInfo>();

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
        }

        private void MainTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (IdeManager.Debugger == null) return;

            IdeManager.Debugger.BreakPoints.BreakPointAdded += Debugger_BreakPointAdded;
            IdeManager.Debugger.BreakPoints.BreakPointRemoved += Debugger_BreakPointRemoved;
        }



        public void OpenFile(string fileName)
        {
            if (!CheckChanges()) return;

            using (var f = new StreamReader(fileName))
            {
                MainTextBox.Text = f.ReadToEnd();
            }

            mFileName = fileName;

            ApplySyntaxHighlight();
        }

        public void OpenContent(string content, string highlighter)
        {
            MainTextBox.Text = content;

            if (highlighter == null) return;

            MainTextBox.CurrentHighlighter = HighlighterManager.Instance.Highlighters[highlighter];
        }

        public void SaveFile()
        {
            if (mFileName == null) 
            {
                // Display a SaveAs dialog box and set the name to that.
                return;
            }

            using (var w = new StreamWriter(mFileName))
            {
                w.Write(MainTextBox.Text);
            }
        }

        public void SaveFileAs(string fileName)
        {
            using (var w = new StreamWriter(fileName))
            {
                w.Write(MainTextBox.Text);
            }
        }

        public void CloseFile()
        {
            if (!CheckChanges()) return;

            MainTextBox.Text = "";
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

        private bool CheckChanges()
        {
            // returns true is operation can proceed (either changes were saved or discarded)
            // or false if not (pending changes dialog was cancelled)
            return true;
        }

        private void ApplySyntaxHighlight()
        {
            string ext = Path.GetExtension(mFileName);
            string hl = null;

            switch (ext.Trim('.').ToLower())
            {
                case "s": hl = "assembler";  break;
                    
                case "c":
                case "cpp":
                case "h":
                case "hpp":
                case "ino": hl = "cpp"; break;

                default: break;
            }

            if (hl == null) return;

            MainTextBox.CurrentHighlighter = HighlighterManager.Instance.Highlighters[hl];
        }


        // __ Keyboard shortcuts ______________________________________________


        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            { 
                switch (e.Key)
                {
                    case Key.S: SaveFile(); break;
                    case Key.W: break;
                }
            }

            switch (e.Key)
            {
                case Key.F9:  // Toggle breakpoint
                    var lineNumber = MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex) + 1;
                    
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



        private void MainTextBox_BeforeDrawingLineNumber(int lineNumber, DrawingContext dc, Rect lineRect)
        {
            foreach (int i in mBreakpoints.Keys)
            { 
                if (i == lineNumber + 1)
                {
                    dc.DrawRectangle(Brushes.Red, null, lineRect);
                    return;
                }
            }
        }


        // __ Breakpoint handling _____________________________________________


        void Debugger_BreakPointRemoved(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints.Remove(breakpoint.LineNumber);

            MainTextBox.InvalidateVisual();
        }

        void Debugger_BreakPointAdded(object sender, BreakPointInfo breakpoint)
        {
            if (breakpoint.SourceFileName != mFileName) return;

            mBreakpoints[breakpoint.LineNumber] = breakpoint;
            
            MainTextBox.InvalidateVisual();
        }
    }
}
