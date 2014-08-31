using System;
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
    /// Interaction logic for StatusHeaderControl.xaml
    /// </summary>
    public partial class StatusHeaderControl : UserControl
    {
        private int mState = 0;

        public int State
        {
            get { return mState; }
            set
            {
                mState = value;
                InternalBar.Margin = new Thickness(-this.ActualWidth * mState, 0, 0, 0);
            }
        }

        public StatusHeaderControl()
        {
            InitializeComponent();

            SizeChanged += StatusHeaderControl_SizeChanged;
        }

        void StatusHeaderControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InternalBar.Width = this.ActualWidth * 2.1;
        }

        
    }
}
