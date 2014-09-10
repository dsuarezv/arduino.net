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
    /// <summary>
    /// Interaction logic for StatusHeaderControl.xaml
    /// </summary>
    public partial class StatusHeaderControl : UserControl
    {
        private int mState = 0;
        private ThicknessAnimation mBarAnimation;
        private Storyboard mBarStoryboard;
        

        public StatusHeaderControl()
        {
            InitializeComponent();

            SetupAnimations();

            SizeChanged += StatusHeaderControl_SizeChanged;
        }

        private void StatusHeaderControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InternalBar.Width = this.ActualWidth * 2.1;
            mBarAnimation.From = GetMarginForState(0);
            mBarAnimation.To = GetMarginForState(1);
        }

        private void SetupAnimations()
        {
            mBarAnimation = new ThicknessAnimation() 
            { 
                From = GetMarginForState(0), 
                To = GetMarginForState(1),
                Duration = TimeSpan.FromSeconds(0.15),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(mBarAnimation, InternalBar);
            Storyboard.SetTargetProperty(mBarAnimation, new PropertyPath(Border.MarginProperty));
            
            mBarStoryboard = new Storyboard();
            mBarStoryboard.Children.Add(mBarAnimation);
        }

        public void SetState(int state, string msg, params object[] args)
        {
            MessageLabel.Text = string.Format(msg, args);
            var key = (state == 0) ? "SuccessColor" : "FailColor";
            var color = this.FindResource(key);
            MessageLabel.Foreground = (SolidColorBrush)color;

            if (state == 0)
            {
                if (mState == 0) return;

                //mBarStoryboard.AutoReverse = true;
                //mBarStoryboard.Begin();
                
                //mBarStoryboard.Seek(mBarAnimation.Duration.TimeSpan);
                //mBarStoryboard.Resume();
                
                mState = 0;
            }
            else
            {
                if (mState == 1) return;

                //mBarStoryboard.AutoReverse = false;
                //mBarStoryboard.Begin();

                mState = 1;
            }

            InternalBar.Margin = GetMarginForState(state);
        }

        private Thickness GetMarginForState(int state)
        {
            return new Thickness(-this.ActualWidth * 1.1 * state, 0, 0, 0);
        }

        
    }
}
