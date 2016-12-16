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

namespace RealtyDB_ClientView
{
    /// <summary>
    /// Interaction logic for CloseTabButton.xaml
    /// </summary>
    public partial class CloseTabButton : UserControl
    {
        public event EventHandler Click;

        public CloseTabButton()
        {InitializeComponent();}

        private void OnClick(object sender, RoutedEventArgs e)=> Click?.Invoke(sender, e);

    }
}
