using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            viewModel.AddCharacterCommand.Executed += new Apex.MVVM.CommandEventHandler(AddCharacterCommand_Executed);
        }

        /// <summary>
        /// Handles the Executed event of the AddCharacterCommand command.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        void AddCharacterCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  Focus the name box so that we can type a new name in straight away.
            nameBox.Focus();
        }
    }
}
