using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:RealtyDB_ClientView.Custom_Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:RealtyDB_ClientView.Custom_Controls;assembly=RealtyDB_ClientView.Custom_Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:ExtendedTabControl/>
    ///
    /// </summary>
        
       
        [TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
        ///в классе решается(?) проблема "выгрузки" страницы и повторной загрузки при смене вкладки
        public class ExtendedTabControl : TabControl
        {
            static ExtendedTabControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedTabControl), new FrameworkPropertyMetadata(typeof(ExtendedTabControl)));
            }

            private Panel ItemsHolderPanel = null;

            public ExtendedTabControl()
                : base()
            {
                // This is necessary so that we get the initial databound selected item
                ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            }

            /// <summary>
            /// If containers are done, generate the selected item
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
            {
                if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    UpdateSelectedItem();
                }
            }

            /// <summary>
            /// Get the ItemsHolder and generate any children
            /// </summary>
            public override void OnApplyTemplate()
            {
                base.OnApplyTemplate();
                ItemsHolderPanel = GetTemplateChild("PART_ItemsHolder") as Panel;
                UpdateSelectedItem();
            }

            /// <summary>
            /// When the items change we remove any generated panel children and add any new ones as necessary
            /// </summary>
            /// <param name="e"></param>
            protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnItemsChanged(e);

                if (ItemsHolderPanel == null)
                    return;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        ItemsHolderPanel.Children.Clear();
                        break;

                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                ContentPresenter cp = FindChildContentPresenter(item);
                                if (cp != null)
                                    ItemsHolderPanel.Children.Remove(cp);
                            }
                        }

                        // Don't do anything with new items because we don't want to
                        // create visuals that aren't being shown

                        UpdateSelectedItem();
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException("Replace not implemented yet");
                }
            }

            protected override void OnSelectionChanged(SelectionChangedEventArgs e)
            {
                base.OnSelectionChanged(e);
                UpdateSelectedItem();
            }

            private void UpdateSelectedItem()
            {
                if (ItemsHolderPanel == null)
                    return;

                // Generate a ContentPresenter if necessary
                TabItem item = GetSelectedTabItem();
                if (item != null)
                    CreateChildContentPresenter(item);

                // show the right child
                foreach (ContentPresenter child in ItemsHolderPanel.Children)
                    child.Visibility = ((child.Tag as TabItem).IsSelected) ? Visibility.Visible : Visibility.Collapsed;
            }

            private ContentPresenter CreateChildContentPresenter(object item)
            {
                if (item == null)
                    return null;

                ContentPresenter cp = FindChildContentPresenter(item);

                if (cp != null)
                    return cp;

                // the actual child to be added.  cp.Tag is a reference to the TabItem
                cp = new ContentPresenter();
                cp.Content = (item is TabItem) ? (item as TabItem).Content : item;
                cp.ContentTemplate = this.SelectedContentTemplate;
                cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
                cp.ContentStringFormat = this.SelectedContentStringFormat;
                cp.Visibility = Visibility.Collapsed;
                cp.Tag = (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem(item));
                ItemsHolderPanel.Children.Add(cp);
                return cp;
            }

            private ContentPresenter FindChildContentPresenter(object data)
            {
                if (data is TabItem)
                    data = (data as TabItem).Content;

                if (data == null)
                    return null;

                if (ItemsHolderPanel == null)
                    return null;

                foreach (ContentPresenter cp in ItemsHolderPanel.Children)
                {
                    if (cp.Content == data)
                        return cp;
                }

                return null;
            }

            protected TabItem GetSelectedTabItem()
            {
                object selectedItem = base.SelectedItem;
                if (selectedItem == null)
                    return null;

                TabItem item = selectedItem as TabItem;
                if (item == null)
                    item = base.ItemContainerGenerator.ContainerFromIndex(base.SelectedIndex) as TabItem;

                return item;
            }
        }
    }
