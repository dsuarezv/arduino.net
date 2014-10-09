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
    /// Interaction logic for BasePad.xaml
    /// </summary>
    public partial class BasePad : UserControl
    {
        public string Title
        {
            get { return (String)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public bool Iscollapsed
        {
            get { return (bool)this.GetValue(IsCollapsedProperty); }
            set { this.SetValue(IsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BasePad));
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(BasePad));


        public BasePad()
        {
            InitializeComponent();
        }

        private void ExpandCollapseChevron_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }
    }
}
