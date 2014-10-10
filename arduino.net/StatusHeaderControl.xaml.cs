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
        private ActionStatus mState = 0;

        public StatusHeaderControl()
        {
            InitializeComponent();
        }


        public void SetState(ActionStatus status, string title, string msg, params object[] args)
        {
            mState = status;

            FirstLine.Text = title;
            SecondLine.Text = string.Format(msg, args);
            SecondLine.Foreground = GetBrushForState();
        }

        private Brush GetBrushForState()
        {
            var key = "C0-Text";

            switch (mState)
            {

                case ActionStatus.InProgress: key = "C6"; break;
                case ActionStatus.Fail: key = "C5"; break;
            }

            return Application.Current.TryFindResource(key) as Brush;
        }
    }

    public enum ActionStatus { OK, InProgress, Fail };
}
