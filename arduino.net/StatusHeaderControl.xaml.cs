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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace arduino.net
{
    public partial class StatusHeaderControl : UserControl
    {
        private int mState = 0;

        public StatusHeaderControl()
        {
            InitializeComponent();
        }


        public void SetState(int state, string msg, params object[] args)
        {
            mState = state;
            MessageLabel.Text = string.Format(msg, args);
            MessageLabel.Foreground = GetBrushForState();
        }

        private Brush GetBrushForState()
        { 
            switch (mState)
            {
                case 0: return Brushes.Black;
                default: return Brushes.Red;
            }
        }
    }
}
