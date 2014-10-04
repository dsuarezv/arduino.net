using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
using FastColoredTextBoxNS;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for OutputTextBox.xaml
    /// </summary>
    public partial class OutputTextBox : UserControl
    {
        private FastColoredTextBox mContentTextBox;

        public OutputTextBox()
        {
            InitializeComponent();

            InitializeTextBox();

            Logger.RegisterListener((i, msg) =>
            {
                Dispatcher.Invoke(() => 
                    {
                        mContentTextBox.AppendText(msg + "\n");
                        mContentTextBox.GoEnd();
                    });
            });
        }

        public void AppendText(string text, bool autoScroll)
        {
            mContentTextBox.AppendText(text);

            if (autoScroll) mContentTextBox.GoEnd();
        }

        public void ClearText()
        {
            mContentTextBox.Clear();
        }


        private void InitializeTextBox()
        {
            mContentTextBox = new FastColoredTextBox()
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ShowLineNumbers = false,
                ReadOnly = true
            };

            mContentTextBox.MouseDown += mContentTextBox_MouseDown;
            mContentTextBox.TextChanged += mContentTextBox_TextChanged;

            WFHost.Child = mContentTextBox;
        }

        private void mContentTextBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                var lineNo = mContentTextBox.PointToPlace(e.Location).iLine;
                var line = mContentTextBox.GetLineText(lineNo);

                string fileName;
                int lineNumber;

                if (IsErrorLocation(line, out fileName, out lineNumber))
                {
                    IdeManager.GoToFileAndLine(fileName, lineNumber);
                }
            }
        }

        private void mContentTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            SyntaxHighlightApplier.Compiler(mContentTextBox, e);
        }


        // TODO: move this to the compiler or idemanager class


        private bool IsErrorLocation(string line, out string fileName, out int lineNumber)
        {
            lineNumber = 0;
            fileName = null;

            if (IsLineMatch(SyntaxHighlightApplier.ErrorRegex, line, out fileName, out lineNumber)) return true;
            if (IsLineMatch(SyntaxHighlightApplier.WarningRegex, line, out fileName, out lineNumber)) return true;

            return false;
        }

        private bool IsLineMatch(Regex regex, string line, out string fileName, out int lineNumber)
        {
            var m = regex.Match(line);
            if (m.Success)
            {
                lineNumber = m.Groups["line"].GetIntValue() - 1;
                fileName = m.Groups["file"].Value;
                return true;
            }

            lineNumber = 0;
            fileName = null;
            return false;
        }
    }
}
