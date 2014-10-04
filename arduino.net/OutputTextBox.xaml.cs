using System;
using System.Collections.Generic;
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
                        //mContentTextBox.ScrollToEnd();
                    });
            });
        }

        public void AppendText(string text, bool autoScroll)
        {
            mContentTextBox.AppendText(text);

            //if (autoScroll) mContentTextBox.Selection.
        }

        public void ClearText()
        {
            mContentTextBox.Clear();
        }


        private void InitializeTextBox()
        {
            mContentTextBox = new FastColoredTextBox()
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            WFHost.Child = mContentTextBox;
        }
    }
}
