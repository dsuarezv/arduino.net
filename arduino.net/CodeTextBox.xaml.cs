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

        public CodeTextBox()
        {
            InitializeComponent();
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

        public void SaveFile()
        {
            if (mFileName == null) return;

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
                case "ino": break;

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
                    case Key.S: break;
                    case Key.W: break;
                    case Key.F5: break; // run without debug (compile first)
                }
            }

            switch (e.Key)
            {
                case Key.F9: break;  // Toggle breakpoint
                case Key.F5: break;  // Debug run (compile first)
            }
            
        }


    }
}
