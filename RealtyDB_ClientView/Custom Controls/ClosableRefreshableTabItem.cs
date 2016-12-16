using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows;
namespace RealtyDB_ClientView
{
    public class ClosableRefreshableTabItem:TabItem
    {
        public RefreshTabButton RTB;
        public CloseTabButton CTB;

        public ClosableRefreshableTabItem():base()
        {
        }

        public void SetHeader(UIElement header)
        {
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(header);

            CTB = new CloseTabButton();
            RTB = new RefreshTabButton();
            //closeButton.Click += (sender, e) =>

            dockPanel.Children.Add(RTB);
            dockPanel.Children.Add(CTB);
            // Set the header
            Header = dockPanel;
        }
    }
}
