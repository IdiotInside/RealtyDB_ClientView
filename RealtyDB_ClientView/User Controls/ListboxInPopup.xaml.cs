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

namespace RealtyDB_ClientView.User_Controls
{
    /// <summary>
    /// Interaction logic for ListboxInPopup.xaml
    /// </summary>
    public partial class ListboxInPopup : UserControl
    {
        public bool AEV_ChangedByCheckbox { get; set; } = false;
        public int SkipCheckingCounter { get; set; } = 0;
        public ListboxInPopup()
        {InitializeComponent();}



        #region properties

        public Type SelectableValuesType
        {
            get { return (Type)GetValue(SelectableValuesTypeProperty); }
            set { SetValue(SelectableValuesTypeProperty, value); }
        }

        public bool UseAsFlaggedEnum
        {
            get { return (bool)GetValue(UseAsFlaggedEnumProperty); }
            set
            {
                SetValue(UseAsFlaggedEnumProperty, value);
                LB.SelectionMode = value ? SelectionMode.Multiple : SelectionMode.Single;
            }
        }
        public int ActualEnumValue
        {
            get { return (int)GetValue(ActualEnumValueProperty); }
            set { SetValue(ActualEnumValueProperty, value); }
        }
        #endregion


        public static readonly DependencyProperty SelectableValuesTypeProperty =
            DependencyProperty.Register("SelectableValuesType", typeof(Type), typeof(ListboxInPopup), new FrameworkPropertyMetadata(typeof(Enum), TypeChanged));//, CoerceTypeChanged), ValidateTypeChanged);

        public static readonly DependencyProperty UseAsFlaggedEnumProperty =
            DependencyProperty.Register("UseAsFlaggedEnum", typeof(bool), typeof(ListboxInPopup), new PropertyMetadata(false, OnConsiderAsFlaggedChanged));


        // Using a DependencyProperty as the backing store for ActualEnumValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualEnumValueProperty =
            DependencyProperty.Register("ActualEnumValue", typeof(int), typeof(ListboxInPopup), new PropertyMetadata(0, OnActualEnumValueChanged));



        #region coerces, callbacks, validations
        private static void TypeChanged(DependencyObject DO, DependencyPropertyChangedEventArgs e)
        {
            if (((Type)e.NewValue).IsEnum && !((Type)e.NewValue).IsEquivalentTo(((Type)e.OldValue)))
            {
                ListboxInPopup LiP = DO as ListboxInPopup;
                //LiP.UseAsFlaggedEnum = ((Type)e.NewValue).GetCustomAttributes(typeof(FlagsAttribute), false).Any();
                LiP.LB.ItemsSource = Enum.GetValues((Type)e.NewValue);
            }
        }

        private static void OnConsiderAsFlaggedChanged(DependencyObject DO, DependencyPropertyChangedEventArgs e)
        { (DO as ListboxInPopup).LB.SelectionMode = (bool)e.NewValue ? SelectionMode.Multiple : SelectionMode.Single; }
        private static void OnActualEnumValueChanged(DependencyObject DO, DependencyPropertyChangedEventArgs e)
        {
            #region info string
            //string t = $"e.NewValue = {e.NewValue.ToString()} of type {e.NewValue.GetType().Name}\n" +
            //    $"e.OldValue = {e.OldValue.ToString()} of type {e.OldValue.GetType().Name}\nLiP.Act...val={LiP.ActualEnumValue.ToString()}\n"
            //    +$"typeof lbi={LiP.LB.Items[0]?.GetType().Name}";
            //Task.Run(() => { MessageBox.Show(t); });
            #endregion
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //Enum E = (Enum)Activator.CreateInstance(LiP.SelectableValuesType);
            ListboxInPopup LiP = DO as ListboxInPopup;
            if (!LiP.AEV_ChangedByCheckbox)
            {
                Task.Run(() => { MessageBox.Show("HALT, SIR"); });
                Enum E = (Enum)Enum.Parse(LiP.SelectableValuesType, e.NewValue.ToString());
                foreach (Enum LBI in LiP.LB.Items)//короче, ричи. у него в айтемсах там все блядь enumTest
                    if (E.HasFlag(LBI))
                    {
                        LiP.SkipCheckingCounter++;
                        if (LiP.UseAsFlaggedEnum)
                            LiP.LB.SelectedItems.Add(LBI);
                        else
                        {
                            LiP.LB.SelectedItem = LBI;
                            break;
                        }
                    }
            }
            else LiP.AEV_ChangedByCheckbox = false;
        }


        private static object CoerceTypeChanged(DependencyObject DO, object x)// => (((Type)x).IsEnum || (x.Equals(typeof(Enum)))) ? x : null;
        {
            //Task T = Task.Run(() => { MessageBox.Show("IN COERCION" + " x.tostring=" + x.ToString()+" x.type= "+ ((Type)x).Name); });
            return x;// (((Type)x).IsEnum||(x.Equals(typeof(Enum)))) ? x : null;
        }

        private static bool ValidateTypeChanged(object x) => ((Type)x).IsEnum || ((Type)x).IsEquivalentTo(typeof(Enum));
        #endregion

        public void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OUT HERE CB");
            CheckBox CB = sender as CheckBox;
            if (true/*SkipCheckingCounter == 0*/)
            {
                MessageBox.Show("IN HERE CB");
                AEV_ChangedByCheckbox = true;
                if (CB.IsChecked.Value)
                    ActualEnumValue = ActualEnumValue | (int)Enum.Parse(SelectableValuesType, CB.Content.ToString());
                else ActualEnumValue = ActualEnumValue & (~(int)Enum.Parse(SelectableValuesType, CB.Content.ToString()));
            }
            //else SkipCheckingCounter--;
            e.Handled = true;
        }
        public void RadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OUT HERE RB");
            RadioButton RB = sender as RadioButton;
            if (true/*SkipCheckingCounter == 0*/)
            {
                MessageBox.Show("IN HERE RB");
                AEV_ChangedByCheckbox = true;
                ActualEnumValue = (int)Enum.Parse(SelectableValuesType, RB.Content.ToString());
            }
            //else SkipCheckingCounter--;
            e.Handled = true;
        }
    }
}
