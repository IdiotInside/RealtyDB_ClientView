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
using System.Collections.ObjectModel;


using System.ComponentModel;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using RealtyDB.ClientSide;
using RealtyDB.Common.Commands;

namespace RealtyDB_ClientView
{
    public partial class MainWindow : Window
    {
        ViewModel VM;
        //public NamedPipeClient NPC;
        public MainWindow()
        {
            VM = new ViewModel("server list.txt", "user list.txt");
            InitializeComponent();
            this.DataContext = VM;
        }
        void Authorize(object s, RoutedEventArgs e)
        {

            if (VM.SelectedServer != "" && VM.SelectedUser != "")
            {
                ClientKeeper.NPC = new NamedPipeClient(VM.SelectedServer, "Secretary", VM.SelectedUser, VM.UserPassword);
                ClientKeeper.NPC.Connect();
                if (ClientKeeper.NPC.OnAir)
                {
                    ClientPresentation CP = new ClientPresentation();
                    ClientKeeper.NPC.CreateNotificationQueue(CP);
                    CP.Show();
                    ClientKeeper.NPC.InformByPopupWindow(ServerTechInfo.Accomplished, "Добро пожаловать, " + ClientKeeper.NPC.Username);

                    VM.Shutdown();
                    this.Close();
                }
                #region hide
                //NPC = new NamedPipeClient(VM.SelectedServer, "Secretary", VM.SelectedUser, VM.UserPassword);
                //NPC.Connect();
                //if (NPC.OnAir)
                //{
                //    ClientPresentation CP = new ClientPresentation();
                //    CP.TransferClient(ref this.NPC);
                //    CP.Show();
                //    VM.Shutdown();
                //    //this.Hide();
                //    this.Close();
                //}
                #endregion
            }
            else MessageBox.Show("Выберите сервер и пользователя для подключения", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    public class ViewModel:INotifyPropertyChanged
    {
        #region fields
        private string ul_path="";
        private string sl_path="";
        private string selectedServer="";
        private string selectedUser="";
        private string userPassword="";
        private List<string> Passwords;
        private List<bool> RememberedPasswords;
        private bool rememberPassword;
        private bool rememberUser;
        private bool rememberServer;
#endregion

        #region INotifyPropertyChange implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChangedFUCKYOU(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ObservableCollection<string> ServerList { get; set; }
        public ObservableCollection<string> UserList { get; set; }

        #region notifiable properties
        public string SelectedServer 
        { 
            get { return this.selectedServer; }
            set { if (this.selectedServer != value) { this.selectedServer = value; NotifyPropertyChangedFUCKYOU("SelectedServer"); } }
        }
        public string SelectedUser
        {
            get { return this.selectedUser; }
            set
            {
                if (this.selectedUser != value)
                {
                    this.selectedUser = value;
                    if (UserList.Contains(SelectedUser))
                        if (RememberedPasswords[UserList.IndexOf(value)])
                            this.UserPassword = Passwords[UserList.IndexOf(SelectedUser)];
                        else this.UserPassword = "";
                    //if (UserPassword!="")
                    //    RememberPassword = true;
                    NotifyPropertyChangedFUCKYOU("SelectedUser");
                }
            }
        }
        public string UserPassword
        {
            get {return this.userPassword;}
            set 
            {
                if (this.userPassword != value) 
                {
                    this.userPassword = value;
                    if (value != "")
                        RememberPassword = true;
                    NotifyPropertyChangedFUCKYOU("UserPassword"); 
                }
            }
        }
        public bool RememberUser 
        {
            get { return this.rememberUser; }
            set 
            {
                if (this.rememberUser != value) 
                { 
                    this.rememberUser = value;
                    if (value == false&&RememberPassword)
                        RememberPassword = false;
                    NotifyPropertyChangedFUCKYOU("RememberUser"); 
                }
            }
        }
        public bool RememberServer
        {
            get { return this.rememberServer; }
            set { if (this.rememberServer != value) { this.rememberServer = value; NotifyPropertyChangedFUCKYOU("RememberServer"); } }
        }
        public bool RememberPassword
        {
            get { return this.rememberPassword; }
            set { if (this.rememberPassword != value) { this.rememberPassword = value; NotifyPropertyChangedFUCKYOU("RememberPassword"); } }
        }
        #endregion
        public ViewModel(string serverListPath, string userListPath)
        {
            this.sl_path=serverListPath;
            this.ul_path=userListPath;
            this.userPassword = "";
            ServerList = new ObservableCollection<string>();
            UserList = new ObservableCollection<string>();
            RememberedPasswords = new List<bool>();
            Passwords = new List<string>();
            #region reading server list
            using (StreamReader SR = new StreamReader(new FileStream(sl_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read), Encoding.GetEncoding(1251)))
            {
                string[] cache;
                while (SR.Peek() != -1)
                {
                    cache = SR.ReadLine().Split(';');
                    ServerList.Add(cache[0]);
                    if (cache[1] == "*")
                    {
                        SelectedServer = cache[0];
                        RememberServer = true;
                    }
                }
            }
            #endregion
            #region reading user and password lists
            using (StreamReader SR = new StreamReader(new FileStream(ul_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read), Encoding.GetEncoding(1251)))
            {
                string[] cache;
                while (SR.Peek() != -1)
                {
                    cache = SR.ReadLine().Split(';');
                    UserList.Add(cache[0]);
                    Passwords.Add(cache[1]);
                    if (cache[2] == "*")
                        RememberedPasswords.Add(true);
                    else RememberedPasswords.Add(false);
                    if (cache[3] == "*")
                    {
                        SelectedUser = cache[0];
                        RememberUser = true;
                    }
                    
                }
            }
            #endregion
        }

        public void Shutdown()
        {
            #region writing servers
            //порядок данных в списке серверов: %имя сервера%;%пустая строка или знак "*". знак "*" означает, что данный сервер будет выбран автоматически при следующем входе%
            using (StreamWriter SW = new StreamWriter(new FileStream(sl_path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read), Encoding.GetEncoding(1251)))
            {
                int i = 0;
                //добавление нового сервера
                if (!ServerList.Contains(SelectedServer))
                {
                    SW.Write(SelectedServer);
                    if (RememberServer)
                        SW.WriteLine(";*;");
                    else SW.WriteLine(";;");
                }
                for (i = 0; i < ServerList.Count-1; i++)
                {
                    SW.Write(ServerList[i]);
                    if (ServerList[i] == SelectedServer && RememberServer)
                        SW.WriteLine(";*;");
                    else SW.WriteLine(";;");
                }
                #region last record
                SW.Write(ServerList[i]);
                if (ServerList[i] == SelectedServer && RememberServer)
                    SW.Write(";*;");
                else SW.Write(";;");
                #endregion
            }
            #endregion

            #region writing users and their passwords
            //формат записи списка пользователей: %имя пользователя%;%пароль%;[*];[*]
            //[*] значит либо пустая строка либо звёздочка. звёздочка --- запомнить пароль для автозаполнения; запомнить пользователя для автовыбора при следующем входе
            using (StreamWriter SW = new StreamWriter(new FileStream(ul_path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read), Encoding.GetEncoding(1251)))
            {
                int i;
                //это для добавления нового пользователя.
                if (!UserList.Contains(SelectedUser))
                {
                    SW.Write(SelectedUser+";"+UserPassword);//пишу новый логин и пароль
                    if (RememberPassword)//если надо запомнить пароль
                        SW.Write(";*");//запоминаю
                    else SW.Write(";");//иначе не запоминмаю
                    if (RememberUser)//если запомнить пользователя для автовыбора
                        SW.WriteLine(";*;");
                    else SW.WriteLine(";;");
                }
                //это для записи уже существующих пользователей
                for (i = 0; i < UserList.Count-1; i++)//цикл идёт до последней записи, томуша последняя записывается так, шобе не было пустой строки
                {
                    SW.Write(UserList[i]);//сначала пишем пользователя
                    SW.Write(";"+Passwords[i]);//потом его пароль
                    if (SelectedUser!=UserList[i])//текущий пользователь в текущей записываемой строке не тот, который выбран в комбобоксе
                    {
                        if (RememberedPasswords[i])//если пароль был ранее отмечен как запоминаемый
                            SW.Write(";*");//оставляю как есть
                        else SW.Write(";");//иначе - тоже оставляю как есть
                        SW.WriteLine(";;");//ну и так как данный пользователь не выбран, то помечаю его как незапоминаемый
                    }
                    else //если пользователь выбран из списка
                    {
                        if (RememberPassword)//если пароль выбран для автозаполнения и пользователь не хочет его "забывать"
                            SW.Write(";*");//запоминаю
                        else SW.Write(";");//иначе - нет
                        if (RememberUser)
                            SW.WriteLine(";*");
                        else SW.WriteLine(";;");
                    }
                }
                #region last record
                SW.Write(UserList[i]);//сначала пишем пользователя
                SW.Write(";" + Passwords[i]);//потом его пароль
                if (SelectedUser != UserList[i])//текущий пользователь в текущей записываемой строке не тот, который выбран в комбобоксе
                {
                    if (RememberedPasswords[i])//если пароль был ранее отмечен как запоминаемый
                        SW.Write(";*");//оставляю как есть
                    else SW.Write(";");//иначе - тоже оставляю как есть
                    SW.Write(";;");//ну и так как данный пользователь не выбран, то помечаю его как незапоминаемый
                }
                else //если пользователь выбран из списка
                {
                    if (RememberPassword)//если пароль выбран для автозаполнения и пользователь не хочет его "забывать"
                        SW.Write(";*");//запоминаю
                    else SW.Write(";");//иначе - нет
                    if (RememberUser)
                        SW.Write(";*");
                    else SW.Write(";;");
                }
                #endregion
            }
            #endregion
        }



    }
    public static class ClientKeeper
    {public static NamedPipeClient NPC { get; set; }}
}
