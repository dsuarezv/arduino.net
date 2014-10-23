using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
                ReadOnly = true,
                BackColor = UiConfig.GetWinformsColor(UiConfig.Background0),
                Font = FontManager.GetSourceCodeFont(),
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

                var msg = CompilerMsg.GetMsgForLine(line);
                if (msg == null) return;
    
                IdeManager.GoToFileAndLine(msg.FileName, msg.LineNumber + 1);
            }
        }

        private void mContentTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            SyntaxHighlightApplier.CppCompiler(mContentTextBox, e);
        }
    }
}
