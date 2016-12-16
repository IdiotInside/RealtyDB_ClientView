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

namespace RealtyDB_ClientView.Custom_Controls
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
    ///     <MyNamespace:ExtendedTextBlock/>
    ///
    /// </summary>
    public class ExtendedTextBlock : TextBlock
    {

        public string TextAndTooltip
        {
            get { return (string)GetValue(TextAndTooltipProperty); }
            set { SetValue(TextAndTooltipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAndTooltip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAndTooltipProperty =
            DependencyProperty.Register("TextAndTooltip", typeof(string), typeof(ExtendedTextBlock), new PropertyMetadata("", OnStringChanged));
        private static void OnStringChanged(DependencyObject DO, DependencyPropertyChangedEventArgs e)
        {
            (DO as ExtendedTextBlock).Text = e.NewValue.ToString();
            (DO as ExtendedTextBlock).ToolTip = e.NewValue;
        }
    }
}
