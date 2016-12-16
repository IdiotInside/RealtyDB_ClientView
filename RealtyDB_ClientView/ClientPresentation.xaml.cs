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
using System.Windows.Shapes;



using System.ComponentModel;
using System.Windows.Threading;
using WPF_UC_Library;
using RealtyDB.Common.Commands;

namespace RealtyDB_ClientView
{
    /// <summary>
    /// Interaction logic for ClientPresentation.xaml
    /// </summary>
    public partial class ClientPresentation : Window
    {
        #region fields
        private PresentationViewModel VM;
        #endregion
        #region properties

        #endregion
        //Popup_Notification_Window PNW;

        public ClientPresentation()
        {
            InitializeComponent();
            VM = new PresentationViewModel();
            if (ClientKeeper.NPC != null && ClientKeeper.NPC.OnAir)
            {
                VM.isConnected = true;
                //Debug.InformByPopupWindow(ServerTechInfo.Accomplished, "Добро пожаловать, " + ClientKeeper.NPC.Username);
                Task X = Task.Run(() =>
                {
                    UserPrivileges up = ClientKeeper.NPC.GetPrivileges();
                    switch(up)
                    {
                        case UserPrivileges.Admin:
                            VM.PathData = (Geometry)this.Resources["chess_kingStyle"];
                            break;
                        case UserPrivileges.Guest:
                            Dispatcher.BeginInvoke(new Action(() => { setGuestRestrictions(); }));
                            Dispatcher.BeginInvoke(new Action(() => { setUserRestrictions(); }));
                            VM.PathData = (Geometry)this.Resources["chess_bishopStyle"];
                            break;
                        case UserPrivileges.User:
                            Dispatcher.BeginInvoke(new Action(() => { setUserRestrictions(); }));
                            VM.PathData = (Geometry)this.Resources["chess_bishopStyle"];
                            break;
                    }
                });
            }
            this.DataContext = VM;
            #region hide
            //ClosableTabItem CTI = new ClosableTabItem();
            //TextBlock H = new TextBlock() { Text = "TEHNOBLOG" };
            //CTI.SetHeader(H);
            //CTI.Style = (Style)this.Resources["TabItemStyle"];
            //Tabs.Items.Add(CTI);


            //ClosableTabItem CTI2 = new ClosableTabItem();
            //TextBlock H2 = new TextBlock() { Text = "pizdec" };
            //CTI2.SetHeader(H2);
            //CTI2.Style = (Style)this.Resources["TabItemStyle"];
            //Tabs.Items.Add(CTI2);
            //PNW = new Popup_Notification_Window();
            //PNW.Show();
            //DispatcherTimer T = new DispatcherTimer();
            //T.Interval = new TimeSpan(0, 0, 2);
            //T.Tick += new EventHandler((object s, EventArgs e) => { NamedPipeClient.ShowNotification("раз два проверка", RealtyDB.Commands.ServerTechInfo.Accomplished); T.Stop(); });
            //T.Tick += new EventHandler((object s, EventArgs e) => { Popup_Notification_Window PNW = new Popup_Notification_Window(); T.Stop(); });
            //T.Start();
            #endregion
        }

        void setGuestRestrictions()
        {
            //ADD_OPCP.IsEnabled = false; ADD_OPE.IsEnabled = false; ADD_OPRP.IsEnabled = false; ADD_ORCP.IsEnabled = false;
            //ADD_ORE.IsEnabled = false; ADD_ORRP.IsEnabled = false; ADD_RPCP.IsEnabled = false; ADD_RPE.IsEnabled = false;
            //ADD_RPRP.IsEnabled = false; ADD_RRCP.IsEnabled = false; ADD_RRE.IsEnabled = false; ADD_RRRP.IsEnabled = false;
            //GET_D.IsEnabled = false; /*GET_E.IsEnabled = false;*/ GET_J.IsEnabled = false; GET_JuC.IsEnabled = false;
            //GET_PhC.IsEnabled = false; GET_R.IsEnabled = false; GET_S.IsEnabled = false;

        }
        void setUserRestrictions()
        {
            //ADD_D.IsEnabled = false; /*ADD_E.IsEnabled = false;*/ ADD_J.IsEnabled = false; ADD_JuC.IsEnabled = false;
            //ADD_PhC.IsEnabled = false; ADD_S.IsEnabled = false; 
        }
       
        private void GetTable(object s, RoutedEventArgs e)
        {
            ClosableRefreshableTabItem CRTI = new ClosableRefreshableTabItem();
            TextBlock H = new TextBlock() { Text = "Редактирование: "+(s as FrameworkElement).Name };
            CRTI.SetHeader(H);
            CRTI.Style = (Style)this.Resources["TabItemStyle"];
            CRTI.Content = new Frame();
            this.Tabs.Items.Add(CRTI);
            this.Tabs.SelectedIndex = this.Tabs.Items.Count - 1;

            Custom_Controls.ExtendedMenuItem EMI = s as Custom_Controls.ExtendedMenuItem;
            getTable GT = new getTable(EMI.OrderedTableName, CRTI, EMI.TableType);

            CRTI.RTB.Click += GT.RefreshClick;
            CRTI.CTB.Click += GT.CloseClick;
            (CRTI.Content as Frame).Content = GT;
            e.Handled = true;
        }
    }


    public class PresentationViewModel:INotifyPropertyChanged
    {
        #region private variables
        private bool connectionState;
        private Geometry statusIcon;
        #endregion
        #region properties
        public bool isConnected 
        {
            get { return this.connectionState; }
            set 
            {
                if (this.connectionState != value) 
                { this.connectionState = value; NotifyPropertyChanged("isConnected"); NotifyPropertyChanged(nameof(isDisconnected)); } }
        }
        public bool isDisconnected
        {get { return !this.connectionState; }}

        public Geometry PathData
        {
            get { return this.statusIcon; }
            set
            {
                if (!value.Equals(this.statusIcon))
                {
                    this.statusIcon = value;
                    NotifyPropertyChanged(nameof(PathData));
                }
            }
        }

        #endregion

        #region INOtifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)=>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
