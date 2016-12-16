using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Controls;
namespace RealtyDB_ClientView
{
    class ClosableTabItem:TabItem
    {
        public void SetHeader(UIElement header)
        {
            // Container for header controls
            DockPanel dockPanel = new DockPanel();

            dockPanel.Children.Add(header);

            // Close button to remove the tab
            CloseTabButton closeButton = new CloseTabButton();
            closeButton.Click += (sender, e) =>
                {
                    ItemsControl tabControl = Parent as ItemsControl;
                    tabControl.Items.Remove(this);
                };
            dockPanel.Children.Add(closeButton);
            // Set the header
            Header = dockPanel;
        }
    }
}
