using System;
using System.Collections.Generic;
using System.Data;
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
using System.Reflection;
namespace RealtyDB_ClientView.User_Controls
{
    /// <summary>
    /// Interaction logic for RealtyClient_Agent_rowDetailBlock.xaml
    /// </summary>
    /// 
    public partial class RealtyClient_Agent_rowDetailBlock : UserControl
    {
        public RealtyClient_Agent_rowDetailBlock(DataSet source)
        {
            InitializeComponent();
            DispenceColumns(source);
            this.ClientCB.ItemsSource = source.Tables["Клиент"].DefaultView;
            this.AgentCB.ItemsSource = source.Tables["Агент"].DefaultView;

            //DGCbB.SelectedValuePath = dgvm.DS.Tables[e.PropertyName].Columns[e.PropertyName].ColumnName;
            //DGCbB.DisplayMemberPath = dgvm.DS.Tables[e.PropertyName].Columns[e.PropertyName].ColumnName;

            //DGCbB.SelectedValueBinding = new Binding(dgvm.DS.Tables[e.PropertyName].Columns[1].ColumnName)
            //{ UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };

            //ObjectDataProvider OBD = new ObjectDataProvider()
            //{
            //    MethodName = "GetValues",
            //    //ObjectType = Assembly.Load(new AssemblyName("RealtyDB.Common")).GetType("RealtyDB.Common.Classes." + tableName, true, true),
            //    IsAsynchronous = true
            //};
            this.AfterInitialize();
        }

        private void BindCombobox(ComboBox CB, DataTable theView)
        {
            CB.ItemsSource = theView.DefaultView;
            CB.SelectedValuePath = theView.PrimaryKey[0].ColumnName;
            CB.DisplayMemberPath = theView.TableName;

        }

        public void DispenceColumns(DataSet source)
        {
            //for (int i = 1;i<source.Tables[0].Columns.Count)
            //{
            //    if (source.Tables[0].Columns[i].ColumnName.Contains("_SUB"))
            //        if (source.Tables[0].Columns[i].ColumnName.Contains("_ENUM"))

            //}
            BindCombobox(AgentCB, source.Tables[AgentCB.Tag.ToString()]);
            BindCombobox(ClientCB, source.Tables[ClientCB.Tag.ToString()]);

        }
        private async void AfterInitialize()
        {
            await FillRowDetailsAsync();
        }
        Task FillRowDetailsAsync() => Task.Run(() =>
        {

        });
    }
}
