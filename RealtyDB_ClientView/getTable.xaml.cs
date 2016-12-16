using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

using RealtyDB.ClientSide;
using WPF_UC_Library;
using RealtyDB.Common.Commands;
using System.Windows.Threading;
using RealtyDB_ClientView.Business_logic;
using System.Reflection;
using RealtyDB_ClientView.User_Controls;
using RealtyDB_ClientView.Custom_Controls;
using System.Windows.Controls.Primitives;

namespace RealtyDB_ClientView
{
    /// <summary>
    /// Interaction logic for getTable.xaml
    /// </summary>
    public partial class getTable : Page
    {
        private alpha_DG_ViewModel dgvm;
        private bool isEditing = false;
        private string tableName;
        private misc.TableGroup rdtt;
        private misc.ApplicationType theApplicationType;
        private ClosableRefreshableTabItem closingTarget;
        public getTable(string tableName, ClosableRefreshableTabItem CRTI, misc.TableGroup RDTT)
        {
            InitializeComponent();
            this.tableName = tableName;
            this.closingTarget = CRTI;
            this.rdtt = RDTT;
            switch (ClientKeeper.NPC.GetPrivileges())
            {
                case UserPrivileges.Admin:
                    break;
                case UserPrivileges.Guest:
                    DG.CanUserAddRows = false;
                    DG.CanUserDeleteRows = false;
                    DG.IsReadOnly = true;
                    break;
            }
        }

        private void set_constraints(out bool set_el_constraint, ref int el, out bool set_pk_constraint, out bool set_do_constraint, string x)
        {
            set_do_constraint = set_el_constraint = set_pk_constraint = false;
            if (x.Contains("_DO"))
                set_do_constraint = true;

            if (dgvm.DS.Tables[0].PrimaryKey[0] == dgvm.DS.Tables[0].Columns[x])
                set_pk_constraint = true;

            if (x.Contains("_EL="))
            {
                set_el_constraint = true;
                el = Convert.ToInt32(x.Substring(x.IndexOf("_EL=") + 4));
            }
        }

        private void DefineColumnType(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            dynamic tables = "";
            for (int i = 1; i < dgvm.DS.Tables.Count; i++)
                tables += dgvm.DS.Tables[i].TableName + ";";
            tables = tables.Split(new char[] { ';' });

            if (!e.PropertyName.Contains("_SUB"))
                #region datetime columns
                if (dgvm.DS.Tables[0].Columns[e.PropertyName].DataType == typeof(DateTime))
                {
                    DataGridTemplateColumn DGTC = new DataGridTemplateColumn();
                    DGTC.Header = e.PropertyName.Substring(0,e.PropertyName.IndexOf('>'));
                    DGTC.HeaderStyle = (Style)this.Resources["DataGridDatesFilterHeader"];

                    //they say creating datatemplates like this is moveton
                    DataTemplate dateTemplate = new DataTemplate(typeof(DateTime));
                    DataTemplate textTemplate = new DataTemplate(typeof(string));

                    FrameworkElementFactory spFactory = new FrameworkElementFactory(typeof(StackPanel));
                    spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                    FrameworkElementFactory spFactory_Text = new FrameworkElementFactory(typeof(StackPanel));
                    spFactory_Text.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(DatePicker));
                    FrameworkElementFactory fef_text = new FrameworkElementFactory(typeof(TextBlock));

                    Binding B = new Binding(e.PropertyName.Substring(0, e.PropertyName.IndexOf('>')))
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                        NotifyOnTargetUpdated = true,
                        NotifyOnSourceUpdated = true,
                        StringFormat = "d",
                        ConverterCulture = CultureInfo.CurrentCulture
                    };
                    fef.SetBinding(DatePicker.SelectedDateProperty, B);
                    fef_text.SetBinding(TextBlock.TextProperty, B);

                    spFactory.AppendChild(fef);
                    spFactory_Text.AppendChild(fef_text);
                    //set the visual tree of the data template
                    dateTemplate.VisualTree = spFactory;
                    textTemplate.VisualTree = spFactory_Text;

                    DGTC.CellTemplate = textTemplate; //datePickerTemplate;// textTemplate;
                    DGTC.CellEditingTemplate = dateTemplate;//datePickerTemplate;
                    DGTC.SortMemberPath = e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'));
                    e.Column = DGTC;
                }
                #endregion
                else if (e.PropertyName.Contains("_FK"))//((string[])tables).Contains(e.PropertyName))
                  #region combobox column
                {
                    DataGridComboBoxColumn DGCbB = new DataGridComboBoxColumn();
                    DGCbB.Header = new ComboboxyDGCHeader(misc.EnumerationSource.DataTable, e.PropertyName.Substring(0, e.PropertyName.IndexOf('>')));//e.PropertyName;
                    DGCbB.ItemsSource = dgvm.DS.Tables[e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'))].DefaultView;

                    DGCbB.SelectedValuePath = e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'));//e.PropertyName;//dgvm.DS.Tables[e.PropertyName].Columns[e.PropertyName].ColumnName;
                    DGCbB.DisplayMemberPath = e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'));//e.PropertyName;//dgvm.DS.Tables[e.PropertyName].Columns[e.PropertyName].ColumnName;

                    DGCbB.SelectedValueBinding = new Binding(dgvm.DS.Tables[e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'))].Columns[1].ColumnName)
                    { UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                    DGCbB.Header = new ComboboxyDGCHeader(misc.EnumerationSource.DataTable, e.PropertyName.Substring(0, e.PropertyName.IndexOf('>')));
                    DGCbB.HeaderStyle = (Style)this.Resources["DGCCBFH"];
                    DGCbB.EditingElementStyle = (Style)this.Resources["SingleSelectComboboxStyle"];
                    e.Column = DGCbB;
                }
                #endregion
                else if (e.PropertyName.Contains("_ENUM="))
                #region enum columns
                {
                    int x;
                    string enumName = e.PropertyName.Substring(x = e.PropertyName.IndexOf("_ENUM=") + 6, e.PropertyName.IndexOf(";") - x);
                    Type T = Assembly.Load(new AssemblyName("RealtyDB.Common")).GetType("RealtyDB.Common.Classes." + enumName, true, true);

                    DataGridTemplateColumn DGTC = new DataGridTemplateColumn();
                    FrameworkElementFactory spFactory_edit = new FrameworkElementFactory(typeof(StackPanel));
                    FrameworkElementFactory spFactory_text = new FrameworkElementFactory(typeof(StackPanel));

                    FrameworkElementFactory listboxInPopup = new FrameworkElementFactory(typeof(ListboxInPopup));
                    FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(ExtendedTextBlock));
                    listboxInPopup.SetValue(ListboxInPopup.SelectableValuesTypeProperty, T);
                    listboxInPopup.SetValue(ListboxInPopup.UseAsFlaggedEnumProperty, theApplicationType == misc.ApplicationType.Спрос);

                    listboxInPopup.SetBinding(ListboxInPopup.ActualEnumValueProperty, new Binding(e.PropertyName)
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        TargetNullValue = 0
                    });
                    textBlock.SetBinding(ExtendedTextBlock.TextAndTooltipProperty, new Binding(e.PropertyName)
                    {
                        Converter = new EnumTextblockConverter(T),
                        TargetNullValue = "Выебрите Х из списка",
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        NotifyOnSourceUpdated = true,
                        NotifyOnTargetUpdated = true
                    });

                    spFactory_text.AppendChild(textBlock);
                    spFactory_edit.AppendChild(listboxInPopup);
                    DataTemplate textDataTemplate = new DataTemplate();
                    textDataTemplate.VisualTree = spFactory_text;
                    DataTemplate cbbDataTemplate = new DataTemplate();
                    cbbDataTemplate.VisualTree = spFactory_edit;
                    DGTC.CellTemplate = textDataTemplate;
                    DGTC.CellEditingTemplate = cbbDataTemplate;
                    DGTC.Header = new ComboboxyDGCHeader(misc.EnumerationSource.Enumeration, T.Name);
                    DGTC.HeaderStyle = (Style)this.Resources["DGCCBFH"];
                    e.Column = DGTC;
                    
                    //DGCbB.ItemsSource = Enum.GetValues(Assembly.Load(new AssemblyName("RealtyDB.Common")).GetType("RealtyDB.Common.Classes." + enumName, true, true));

                    //DGCbB.HeaderStyle = (Style)this.Resources["DGCCBFH"];

                    //else DGCbB.EditingElementStyle = (Style)this.Resources["SingleSelectComboboxStyle"];

                    dgvm.DS.Tables[0].Columns[e.PropertyName].ColumnName = e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'));
                }
                #endregion
                #region text columns
                else
                {
                    string x = e.PropertyName.Substring(0, e.PropertyName.IndexOf('>'));
                    bool set_do_constraint, set_el_constraint, set_pk_constraint;
                    int el_constraint = 0;

                    set_constraints(out set_el_constraint, ref el_constraint, out set_pk_constraint, out set_do_constraint, e.PropertyName);

                    dgvm.DS.Tables[0].Columns[e.PropertyName].ColumnName = x;

                    DataGridTextColumn DGTC = new DataGridTextColumn();
                    DGTC.Header = x;

                    Binding B = new Binding(x);
                    if (set_pk_constraint)
                        B.Mode = BindingMode.OneWay;
                    else B.Mode = BindingMode.TwoWay;
                    B.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
                    DGTC.Binding = B;

                    if (set_do_constraint)
                        B.ValidationRules.Add(new StringDigitsOnlyValidationRule(x));

                    if (set_el_constraint)
                        B.ValidationRules.Add(new EstimatedLengthConstraintValidationRule(el_constraint, x));

                    e.Column = DGTC;
                }
            #endregion

            #region obsolete
            //else for (int i = 0; i < dgvm.DS.Tables.Count; i++)
            //        #region combobox column
            //        if (e.PropertyName == dgvm.DS.Tables[i].TableName)
            //        {
            //            DataGridComboBoxColumn DGCbB = new DataGridComboBoxColumn();
            //            DGCbB.Header = e.PropertyName;
            //            DGCbB.ItemsSource = dgvm.DS.Tables[i].DefaultView;
            //            DGCbB.SelectedValuePath = dgvm.DS.Tables[i].Columns[e.PropertyName].ColumnName;
            //            DGCbB.DisplayMemberPath = dgvm.DS.Tables[i].Columns[e.PropertyName].ColumnName;

            //            DGCbB.SelectedValueBinding = new Binding(dgvm.DS.Tables[i].Columns[1].ColumnName)
            //            { UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
            //            DGCbB.HeaderStyle = (Style)this.Resources["DGCCBFH"];
            //            DGCbB.EditingElementStyle = (Style)this.Resources["RadiobuttonComboboxStyle"];
            //            e.Column = DGCbB;
            //            break;
            //        }
            //        #endregion
            //        #region text columns
            //        else
            //        {
            //            DataGridTextColumn DGTC = new DataGridTextColumn();
            //            DGTC.Header = e.PropertyName;

            //            Binding B = new Binding(e.PropertyName);
            //            B.Mode = BindingMode.TwoWay;
            //            B.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
            //            DGTC.Binding = B;

            //            if (e.PropertyName.ToLower() == "инн" || e.PropertyName.ToLower() == "снилс" || e.PropertyName.ToLower() == "паспорт")
            //                B.ValidationRules.Add(new StringDigitsOnlyValidationRule(e.PropertyName.ToLower() == "инн" ? documentType.ИНН : e.PropertyName.ToLower() == "паспорт" ? documentType.Паспорт : documentType.СНИЛС));

            //            e.Column = DGTC;
            //        }
            //#endregion
            #endregion
        }

        private void DG_DropFocus(object sender, EventArgs e) => DG.SelectedItem = null;

        private void DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!isEditing)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (Keyboard.IsKeyDown(Key.Z))
                    {
                        dgvm.SuppressEvents = true;
                        dgvm.InvokeUndo();
                        dgvm.SuppressEvents = false;
                    }
                    else if (Keyboard.IsKeyDown(Key.F))
                    {
                        dgvm.SuppressEvents = true;
                        dgvm.InvokeRedo();
                        dgvm.SuppressEvents = false;
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Tab)
                DG.CommitEdit(DataGridEditingUnit.Cell, true);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            dgvm.SuppressEvents = true;
            dgvm.InvokeUndo();
            dgvm.SuppressEvents = false;
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            dgvm.SuppressEvents = true;
            dgvm.InvokeRedo();
            dgvm.SuppressEvents = false;
        }

        private void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isEditing)
            {
                isEditing = false;
                DG.CommitEdit(DataGridEditingUnit.Row, true);
            }
        }
        private void DG_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) => isEditing = true;


        //submit logic
        private void Submit_Click(object sender, RoutedEventArgs e) => dgvm.Submit();


        //tab items button events
        public void RefreshClick(object s, EventArgs e)
        { this.dgvm?.Refresh(); }

        public void CloseClick(object s, EventArgs e)
        {
            exitAction eA;
            eA = dgvm.RequestClosure();
            if (eA != exitAction.cancel)
            {
                if (eA == exitAction.save)
                    dgvm.Submit();
                ItemsControl tabControl = closingTarget.Parent as ItemsControl;
                tabControl.Items.Remove(closingTarget);
            }
    }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Page_Loaded;

            DependencyObject x = VisualTreeHelper.GetParent(this);
            do x = VisualTreeHelper.GetParent(x);
            while (x.GetType() != typeof(ClientPresentation));

            switch(this.rdtt)
            {
                case misc.TableGroup.None:
                    dgvm = new DG_ViewModel(this.DG, tableName, x as ClientPresentation, loadingIcon);
                    break;
                case misc.TableGroup.Realty_Offer:
                    this.theApplicationType = misc.ApplicationType.Предложение;
                    dgvm = new Realty_DG_ViewModel(this.DG, tableName, x as ClientPresentation, loadingIcon, misc.ApplicationType.Предложение);
                    break;
                case misc.TableGroup.Realty_Demand:
                    this.theApplicationType = misc.ApplicationType.Спрос;
                    dgvm = new Realty_DG_ViewModel(this.DG, tableName, x as ClientPresentation, loadingIcon, misc.ApplicationType.Спрос);
                    break;
                case misc.TableGroup.Employee:
                    dgvm = new DG_ViewModel(this.DG, tableName, x as ClientPresentation, loadingIcon);
                    break;
                case misc.TableGroup.Client:
                    break;
                case misc.TableGroup.Service:
                    break;
                default: dgvm = new DG_ViewModel(this.DG, tableName, x as ClientPresentation, loadingIcon);break;
            }
            this.DataContext = dgvm;
        }
    }
}