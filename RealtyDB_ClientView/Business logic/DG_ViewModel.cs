using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Threading;
using WPF_UC_Library;
using System.Reflection;
using System.Linq;
using RealtyDB.Common.Commands;
using RealtyDB.Common.Classes;
using System.Threading;
using RealtyDB_ClientView.Custom_Controls;

namespace RealtyDB_ClientView
{
    public abstract class alpha_DG_ViewModel:INotifyPropertyChanged
    {
        #region variables
        private DispatcherTimer statusStringTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 3) };
        protected DataGrid DG;
        private UndoRedoManager URM;
        private Window ownerWindow;
        private Dictionary<string, string> Filters = new Dictionary<string, string>();
        private List<DependencyObject> FilterControls = new List<DependencyObject>();
        private bool droppingAllFilters = false;
        private bool filtersActivated;
        private bool hasChanges;
        private bool canRedo;
        private Command dropFiltersCommand;
        private DataSet ds;
        private BindingListCollectionView blcv;
        private string tableName = "";
        private Type tableItemType;
        private string statusString = "";
        private LoadingIcon loadingIcon;
        #endregion

        #region properties
        public BindingListCollectionView BLCV
        {
            get { return blcv; }
            set { this.blcv = value; OnPropertyChanged(nameof(BLCV)); }
        }
        public Command DropFiltersCommand
        {
            get { return this.dropFiltersCommand; }
            set { this.dropFiltersCommand = value; }
        }
        public bool SuppressEvents { get; set; } = false;
        public bool FiltersActivated
        {
            get { return this.filtersActivated; }
            set
            {
                if (filtersActivated != value)
                {
                    filtersActivated = value;
                    OnPropertyChanged(nameof(FiltersActivated));
                }
            }
        }
        public bool HasChanges
        {
            get { return this.hasChanges; }
            set
            {
                if (this.hasChanges != value)
                {
                    this.hasChanges = value;
                    OnPropertyChanged(nameof(HasChanges));
                }
            }
        }
        public bool CanRedo
        {
            get { return this.canRedo; }
            set
            {
                if (this.canRedo != value)
                {
                    this.canRedo = value;
                    OnPropertyChanged(nameof(CanRedo));
                }
            }
        }

        public string StatusString
        {
            get { return this.statusString; }
            set
            {
                if (this.statusString != value)
                {
                    this.statusString = value;
                    OnPropertyChanged(nameof(StatusString));
                    if (statusStringTimer.IsEnabled)
                        statusStringTimer.Stop();
                    statusStringTimer.Start();
                }
            }
        }
        public DataSet DS
        {
            get { return this.ds; }
            set { this.ds = value; }
        }
        #endregion

        public alpha_DG_ViewModel(DataGrid dg, string tN, Window oW, LoadingIcon lI)
        {
            DropFiltersCommand = new Command(DropAllFilters);
            this.DG = dg;
            this.loadingIcon = lI;
            this.tableName = tN;
            this.ownerWindow = oW;
            statusStringTimer.Tick += StatusStringTimer_Tick;
            this.CanRedo = false;
            this.HasChanges = false;
            tableItemType = Assembly.Load(new AssemblyName("RealtyDB.Common")).GetType("RealtyDB.Common.Classes." + tableName, true, true);
            //InitializeDataset();
            InitializeDatasetAsync();//какая версия вызовется?
        }

        #region notifypropertychanged implementation
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region invoking undo-redo manager methods
        public void InvokeUndo(int x = 1) => HasChanges = URM.Undo(x);

        public void InvokeRedo(int x = 1) => CanRedo = URM.Redo(x);
        #endregion

        #region datatable events

        private void DG_ViewModel_ColumnChanging(object sender, DataColumnChangeEventArgs e)
        {
            //MessageBox.Show("suppress events is " + SuppressEvents.ToString());
            DataTable temp_DT = sender as DataTable;
            if (!SuppressEvents)
            {
                int row_index = temp_DT.Rows.IndexOf(e.Row);
                if (row_index != -1)
                    if (e.ProposedValue != temp_DT.Rows[row_index][e.Column])
                    {
                        int xC = row_index;
                        int yC = temp_DT.Columns.IndexOf(e.Column);
                        URM.UpdateAction(temp_DT.Rows[row_index][e.Column], e.ProposedValue, new Tuple<long, int>((long)e.Row[temp_DT.PrimaryKey[0]], yC));
                        HasChanges = true;
                    }
            }
        }

        private void DG_ViewModel_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            if (!SuppressEvents)
            {
                DataTable temp_DT = sender as DataTable;
                int x = temp_DT.Rows.IndexOf(e.Row);
                URM.DeleteAction(x);
                HasChanges = true;
            }
        }

        private void DG_ViewModel_RowChanging(object sender, DataRowChangeEventArgs e)
        {
            if (!SuppressEvents)
            {
                switch (e.Action)
                {
                    case DataRowAction.Add:
                        HasChanges = true;
                        URM.InsertAction(e.Row);
                        break;
                }
            }
        }
        #endregion

        #region binded events

        public void DropAllFilters()
        {
            droppingAllFilters = true;
            foreach (DependencyObject DO in this.FilterControls)
                if (DO is TextBox)
                    ((TextBox)DO).Text = "";
                else if (DO is CheckBox)
                    ((CheckBox)DO).IsChecked = false;
                else if (DO is DatePicker)
                    ((DatePicker)DO).SelectedDate = null;

            this.FilterControls.Clear();

            droppingAllFilters = false;
            FiltersActivated = false;

            if (BLCV != null)
                ApplyFilters();
        }

        public void FillComboBox(object sender, RoutedEventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            DependencyObject DO = CB;
            do DO = VisualTreeHelper.GetParent(DO);
            while (!(DO is ComboboxyDGCHeader));
            ComboboxyDGCHeader CdgcH = (ComboboxyDGCHeader)DO;
            if (CdgcH.theEnumerationSource == misc.EnumerationSource.DataTable)
            {
                ObservableCollection<Business_logic.EnumDictionary> Temp = new ObservableCollection<Business_logic.EnumDictionary>();
                for (int i = 0; i < ds.Tables[CdgcH.theAddress].Rows.Count; i++)
                    Temp.Add(new Business_logic.EnumDictionary((long)ds.Tables[CdgcH.theAddress].Rows[i][0], (string)ds.Tables[CdgcH.theAddress].Rows[i][1]));
                CB.ItemsSource = Temp;
                //for (int i = 0; i < ds.Tables[CB.Tag.ToString()].Rows.Count; i++)
                //    Temp.Add(new Business_logic.EnumDictionary((long)ds.Tables[CB.Tag.ToString()].Rows[i][0], (string)ds.Tables[CB.Tag.ToString()].Rows[i][1]));
                //CB.ItemsSource = Temp;
            }
            else CB.ItemsSource = Enum.GetValues(Assembly.Load(new AssemblyName("RealtyDB.Common")).GetType("RealtyDB.Common.Classes." + CdgcH.theAddress, true, true));
        }

        public void DatePickerChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            CustomDatePicker CDP = (CustomDatePicker)sender;

            if (CDP.EventLocker)
            { CDP.EventLocker = false; return; }

            if (!FilterControls.Contains(CDP))
                FilterControls.Add(CDP);
            if (CDP.SelectedDate == null && !droppingAllFilters)
                FilterControls.Remove(CDP);

            if (!droppingAllFilters)
                FiltersActivated = FilterControls.Count > 0;

            string columnName = CDP.Tag.ToString();

            if (BLCV != null)
                PrepareFilters(CDP.SelectedDate, columnName, CDP.Period == datePickerType.until);
            CDP.EventLocker = true;
        }

        public void FilterCheckboxHandler(object sender, RoutedEventArgs e)
        {
            CheckBox CB = (CheckBox)sender;
            //TODO - найти типа ComboboxyDGCHeader, посмотреть чё это - таблица или энум, в зависимости от этого вызвать соответствующую перегрузку препэйр филтерс
            string columnName = CB.Tag.ToString(), criterion = CB.Content.ToString();

            if (CB.IsChecked.Value && !FilterControls.Contains(CB))
                FilterControls.Add(CB);

            if (!CB.IsChecked.Value && !droppingAllFilters)
                FilterControls.Remove(CB);

            if (!droppingAllFilters)
                FiltersActivated = FilterControls.Count > 0;

            if (BLCV != null)
                this.PrepareFilters(criterion, columnName, CB.IsChecked ?? false);
        }

        public void FilterTextHandler(object sender, TextChangedEventArgs e)
        {
            TextBox CTB = sender as TextBox;
            if (!FilterControls.Contains(CTB))
                FilterControls.Add(CTB);


            string columnName = CTB.Tag.ToString(), criterion = CTB.Text;
            #region bindinglistcollectionview criterion parsing
            if (BLCV != null)
            {
                FilterType ft = FilterType.None;
                if (criterion != "")
                {
                    if (ds.Tables[0].Columns[columnName].DataType == typeof(long) ||
                          ds.Tables[0].Columns[columnName].DataType == typeof(double) ||
                          ds.Tables[0].Columns[columnName].DataType == typeof(int) ||
                          ds.Tables[0].Columns[columnName].DataType == typeof(float) ||
                          ds.Tables[0].Columns[columnName].DataType == typeof(decimal))
                        ft = FilterType.Numeric;
                    else if (ds.Tables[0].Columns[columnName].DataType == typeof(string))
                        ft = FilterType.Text;
                }
                else if (!droppingAllFilters) FilterControls.Remove(CTB);

                if (!droppingAllFilters)
                    FiltersActivated = FilterControls.Count > 0;
                this.PrepareFilters(criterion, columnName, ft);
            }
            #endregion
        }

        #endregion

        #region preparing filters
        /// <summary>
        /// Перегрузка для текстового фильтра
        /// </summary>
        /// <param name="criterion">Параметр фильтрации (текст)</param>
        /// <param name="columnName">Имя столбца в DataTable</param>
        /// <param name="FT">Тип фильтра - числа/строки</param>
        private void PrepareFilters(string criterion, string columnName, FilterType FT)
        {
            if (criterion != "")
                switch (FT)
                {
                    case FilterType.Numeric:
                        long temp;
                        if (long.TryParse(criterion, out temp))
                            criterion = $"Convert({columnName}, 'System.String')  LIKE '%{criterion}%'";//????
                            //criterion = columnName + " = " + criterion;

                        //criterion = $"Convert(string, {columnName})  LIKE '%{criterion}%'";
                        else criterion = "";
                        //criterion = columnName + " LIKE" + "'%" + "Convert("+criterion+",System.String)" + "%'";
                        break;
                    case FilterType.Text:
                        criterion = columnName + " LIKE" + "'%" + criterion + "%'";
                        break;
                    default: break;
                }

            Filters[columnName] = criterion;

            if (!droppingAllFilters)
                ApplyFilters();
        }
        /// <summary>
        /// Перегрузка для дат.
        /// </summary>
        /// <param name="criterion">Значение дейттайм</param>
        /// <param name="columnName">имя столбца в DataTable</param>
        /// <param name="isUntil">Флаг определения левого или правого предела дейттаймпикера. true - до, false - от</param>
        private void PrepareFilters(DateTime? criterion, string columnName, bool isUntil)
        {
            if (criterion != null)
                if (isUntil)
                    if (Filters[columnName] == "")
                        Filters[columnName] = "[" + columnName + "]" + " <= " + "'" + criterion.Value.ToShortDateString() + "'" + " OR [" + columnName + "] is null";
                    else Filters[columnName] += " AND " + "[" + columnName + "]" + " <= " + "'" + criterion.Value.ToShortDateString() + "'" + " OR [" + columnName + "] is null";
                else if (Filters[columnName] == "")
                    Filters[columnName] = "[" + columnName + "]" + " >= " + "'" + criterion.Value.ToShortDateString() + "'" + " OR [" + columnName + "] is null";
                else Filters[columnName] += " AND " + "[" + columnName + "]" + " >= " + "'" + criterion.Value.ToShortDateString() + "'" + " OR [" + columnName + "] is null";
            else
            {
                if (Filters[columnName].Contains(" AND "))
                {
                    string[] x = Filters[columnName].Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                    if (isUntil)
                        if (x[0].Contains("<"))
                            Filters[columnName] = x[0];
                        else Filters[columnName] = x[1];
                    else if (x[0].Contains(">"))
                        Filters[columnName] = x[0];
                    else Filters[columnName] = x[1];
                }
                else Filters[columnName] = "";
            }

            if (!droppingAllFilters)
                ApplyFilters();
        }
        /// <summary>
        /// Перегрузка фильтрации для checkedcombobox, где источником данных является datatable
        /// </summary>
        /// <param name="criterion">Текст выбранного/сброшенного чекбокса</param>
        /// <param name="columnName">Название столбца в DataTable</param>
        /// <param name="Chosen">Чекбокс был отмечен(true) или наоборот, выделение было снято(false)</param>
        private void PrepareFilters(string criterion, string columnName, bool Chosen)
        {
            if (Filters[columnName] == "")
                Filters[columnName] = columnName + " in ('" + criterion + "')";
            else
            {
                if (Chosen)
                    Filters[columnName] = Filters[columnName].Remove(Filters[columnName].Length - 1) + "," + "'" + criterion + "'" + ")";
                else
                {
                    if (Filters[columnName].Contains("'" + criterion + "',"))
                        Filters[columnName] = Filters[columnName].Replace("'" + criterion + "',", "");
                    else Filters[columnName] = Filters[columnName].Replace("'" + criterion + "'", "");

                    if (Filters[columnName].Trim().Contains("()"))
                        Filters[columnName] = "";
                }
            }
            if (!droppingAllFilters)
                ApplyFilters();
        }
        /// <summary>
        /// Перегрузка фильтрации для checkedcombobox, где источником является enum
        /// </summary>
        /// <param name="criterion">Enum отмеченный</param>
        /// <param name="columnName">Название столбца в DataTable</param>
        /// <param name="Chosen">Был отмечен(true) или наоборот выключен(false)</param>
        /// <param name="T">Тип перечисления</param>
        private void PrepareFilters(Enum criterion, string columnName, bool Chosen, Type T)
        {
            if (Filters[columnName] == "")
                Filters[columnName] = columnName + " in ('" + criterion + "')";
            else
            {
                if (Chosen)
                    Filters[columnName] = Filters[columnName].Remove(Filters[columnName].Length - 1) + "," + "'" + criterion + "'" + ")";
                else
                {
                    if (Filters[columnName].Contains("'" + criterion + "',"))
                        Filters[columnName] = Filters[columnName].Replace("'" + criterion + "',", "");
                    else Filters[columnName] = Filters[columnName].Replace("'" + criterion + "'", "");

                    if (Filters[columnName].Trim().Contains("()"))
                        Filters[columnName] = "";
                }
            }
            if (!droppingAllFilters)
                ApplyFilters();
        }
        #endregion

        private void ApplyFilters()
        {
            #region bindinglistcollectionview concatenating and applying filter
            string newFilter = "";

            foreach (KeyValuePair<string, string> KVP in Filters)
                if (KVP.Value.Trim(new char[] { ' ' }) != "")
                    if (newFilter.Length > 0)
                        newFilter += " AND " + KVP.Value;
                    else newFilter += KVP.Value;

            DG.CancelEdit(DataGridEditingUnit.Row);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                StatusString = "Применяю фильтры";
                loadingIcon.RotateMe();
                BLCV.CustomFilter = newFilter;
                //StatusString = "";
                loadingIcon.StopRotation();
            }));

            #endregion
        }

        private void StatusStringTimer_Tick(object sender, EventArgs e)
        {
            StatusString = "";
            statusStringTimer.Stop();
        }

        public exitAction RequestClosure()
        {
            exitAction eA = exitAction.dont_save;
            if (HasChanges)
            {
                Notificational_Modal_Window nmw = new Notificational_Modal_Window("Есть несохраненные изменения. Выберите действие перед обновлением.", this.ownerWindow);
                nmw.ExtendedShowDialog(out eA);
            }
            return eA;
        }

        public enum FilterType : byte
        {
            Numeric = 0,
            Text = 1,
            DateTimes = 2,
            Enums = 4,
            None = 5
        }

        public void Submit()
        {
            bool isDataSetSophisticated = DS.Tables.Count > 1;
            DataTable dt = DS.Tables[0].GetChanges();
            if (dt != null)
            {
                List<alphaCommand<ServerTechInfo>> changedRecords = new List<alphaCommand<ServerTechInfo>>();
                for (int i = 0; i < dt.Rows.Count; i++)
                    switch (dt.Rows[i].RowState)
                    {
                        case DataRowState.Added:
                            List<object> bullshitInsert = prepareConstructorParameters(dt, i);
                            ClientKeeper.NPC.SendCommand(new Command_Insert((alphaObject)Activator.CreateInstance(tableItemType, bullshitInsert.ToArray())));

                            break;
                        case DataRowState.Deleted:
                            ClientKeeper.NPC.SendCommand(new Command_Delete((long)dt.Rows[i][dt.PrimaryKey[0], DataRowVersion.Original], tableName));
                            break;
                        case DataRowState.Modified:
                            List<object> bullshitUpdate = prepareConstructorParameters(dt, i);
                            ClientKeeper.NPC.SendCommand(new Command_Update((long)dt.Rows[i][dt.PrimaryKey[0]], (alphaObject)Activator.CreateInstance(tableItemType, bullshitUpdate.ToArray())));
                            break;
                        default: ClientKeeper.NPC.InformByPopupWindow(ServerTechInfo.Ambigous, "вот щас чо то не понял..."); break;
                    }
            }
            dt.AcceptChanges();
            this.HasChanges = false;
            this.CanRedo = false;
            URM.DropStacks();
        }

        protected virtual Task InitializeDatasetAsync() => Task.Run(() => { insideInitialization(); });
        //{
            //return Task.Run(async () => 
            //{
            //    insideInitialization();
            //});
        //}

        protected virtual async void insideInitialization()
        {
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { loadingIcon.RotateMe(); }));
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.StatusString = "Запросил таблицу";
            //Thread.Sleep(10000);
            this.DS = await RetrieveDS();
            this.StatusString = "Получил таблицу.";
            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                Filters.Add(ds.Tables[0].Columns[i].ColumnName.Substring(0, ds.Tables[0].Columns[i].ColumnName.IndexOf('>')), "");

            DS.Tables[0].RowChanging += DG_ViewModel_RowChanging;
            DS.Tables[0].RowDeleting += DG_ViewModel_RowDeleting;
            DS.Tables[0].ColumnChanging += DG_ViewModel_ColumnChanging;
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                BLCV = new BindingListCollectionView(DS.Tables[0].DefaultView);
                URM = new UndoRedoManager(DS.Tables[0]);
                loadingIcon.StopRotation();
            }));
        }

        public void Refresh()
        {
            exitAction eA;
            bool refreshFlag = true;
            if (this.HasChanges)
            {
                Notificational_Modal_Window nmw = new Notificational_Modal_Window("Есть несохраненные изменения. Выберите действие перед обновлением.", this.ownerWindow);
                nmw.ExtendedShowDialog(out eA);

                if (eA != exitAction.cancel)
                {
                    if (eA == exitAction.save)
                        this.Submit();
                }
                else refreshFlag = false;
            }
            if (refreshFlag)
            {
                DS?.Clear();
                DropAllFilters();
                Filters.Clear();
                HasChanges = false;
                InitializeDatasetAsync();
            }
        }


        Task<DataSet> RetrieveDS() => Task.Run(() =>
        {
            Command_TableSelect command = new Command_TableSelect(this.tableName);
            ClientKeeper.NPC.SendCommand(command);
            return command.Rezult.Copy();
        });


        //reflection 00h my god
        private List<object> prepareConstructorParameters(DataTable dt, int index)
        {
            List<object> bullshit = new List<object>();
            string convenience;
            bool referredToAnotherTable;
            ConstructorInfo[] CIs = tableItemType.GetConstructors();
            //this is bullshit. i am obligating myself to have only one constructor...
            ParameterInfo[] PIs = CIs[0].GetParameters();

            //dynamic involvedTables = "";
            //for (int x = 0; x < ds.Tables.Count; x++)
            //    involvedTables += ds.Tables[x].TableName + ";";
            //involvedTables = involvedTables.Split(new char[] { ';' });

            for (int k = 0; k < PIs.Length; k++)
            {
                //ridiculous as well. nomination of constructor parameters must match sql table column names, which makes it hard to update 
                referredToAnotherTable = false;
                convenience = PIs[k].Name;
                if (convenience.Contains("_FK"))
                {
                    convenience = convenience.Replace("_FK", "");
                    referredToAnotherTable = true;
                }
                convenience = convenience.Replace('_', ' ');
                //MessageBox.Show("convenience = " + convenience);

                //for (int i = 0; i < involvedTables.Length; i++)
                //    if (involvedTables[i].ToLower().Trim().Contains(convenience.ToLower().Trim()))
                //    { referredToAnotherTable = true; break; }

                //MessageBox.Show(convenience);

                if (dt.Rows[index][convenience] != null)
                    if (referredToAnotherTable)
                    {
                        //object a = DS.Tables[convenience];
                        //object b = dt.Rows[index][convenience];
                        //object c = DS.Tables[convenience].Select().First(F => F[convenience].Equals(dt.Rows[index][convenience]));
                        //object d = DS.Tables[convenience].PrimaryKey[0];
                        //object e = DS.Tables[convenience].Select().First(F => F[convenience].Equals(dt.Rows[index][convenience]))[DS.Tables[convenience].PrimaryKey[0]];
                        bullshit.Add(Convert.ChangeType(DS.Tables[convenience].Select().First(F => F[convenience].Equals(dt.Rows[index][convenience]))[DS.Tables[convenience].PrimaryKey[0]], PIs[k].ParameterType));
                    }
                    else bullshit.Add(Convert.ChangeType(dt.Rows[index][convenience], PIs[k].ParameterType));
                else bullshit.Add(null);
            }

            return bullshit;
        }


    }


    public class DG_ViewModel :alpha_DG_ViewModel
    {
        public DG_ViewModel(DataGrid dg, string tN, Window oW, LoadingIcon lI):base (dg,tN,oW,lI)
        {
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeDatasetAsync();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }



    

    public class Realty_DG_ViewModel:alpha_DG_ViewModel
    {
        private misc.ApplicationType theApplicationType;
        public Realty_DG_ViewModel(DataGrid dg,string tN, Window PrimaryWindow, LoadingIcon LI, misc.ApplicationType anApplicationType):base(dg,tN,PrimaryWindow, LI)
        {
            DataTemplate tempDT = new DataTemplate();
            this.theApplicationType = anApplicationType;
            dg.RowDetailsTemplate = new DataTemplate();
        }

        public void FindSimilar(object s, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async void insideInitialization()
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            base.insideInitialization();

            User_Controls.RealtyClient_Agent_rowDetailBlock rcardb = new User_Controls.RealtyClient_Agent_rowDetailBlock(DS);
            rcardb.FindSimilar.Click += FindSimilar;

            //they say creating datatemplates like this is moveton
            DataTemplate rowDetailsTemplate = new DataTemplate();

            FrameworkElementFactory spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            FrameworkElementFactory fef = new FrameworkElementFactory(typeof(User_Controls.RealtyClient_Agent_rowDetailBlock));
            spFactory.AppendChild(fef);
            rowDetailsTemplate.VisualTree = spFactory;

            this.DG.RowDetailsTemplate = rowDetailsTemplate;
        }

    }
}
