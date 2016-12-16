using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RealtyDB_ClientView
{
    interface IDataGridCommand
    {
        void Execute();
        void Rollback();
    }

    class InsertCommand : IDataGridCommand
    {
        private DataTable DT;
        private object[] rowValues;
        //private DataRow dr;
        public InsertCommand(DataRow dr, DataTable DT)
        {
            rowValues = dr.ItemArray;
            //this.dr = dr;
            this.DT = DT;
        }
        public void Execute()// => DT.Rows.Add(dr);
        {
            DT.Rows.Add(rowValues);
        }
        public void Rollback()//=> DT.Rows.Remove(dr);
        {
            DataRow dr = DT.NewRow();
            dr.ItemArray = rowValues;
            DT.Rows.Remove((DT.Select().First(x => (long)x.ItemArray[0] == (long)dr.ItemArray[0])));
        }
    }
    class UpdateCommand:IDataGridCommand
    {
        private DataTable DT;
        private object previousValue;
        private object newValue;
        private Tuple<long, int> Coordinate;
        public UpdateCommand(object pV, object nV, Tuple<long, int> C, DataTable DT)
        {
            this.previousValue = pV; this.newValue = nV;
            this.Coordinate = C; this.DT = DT;
        }
        public void Execute()
        {
            //DT.Rows[Coordinate.Item1][Coordinate.Item2] = newValue;
            DT.Select().First(x => (long)x[DT.PrimaryKey[0]] == Coordinate.Item1)[Coordinate.Item2] = newValue;
            //this.DT.AcceptChanges();
        }
        public void Rollback()
        {
            DT.Select().First(x => (long)x[DT.PrimaryKey[0]] == Coordinate.Item1)[Coordinate.Item2] = previousValue;
            //this.DT.AcceptChanges();
        }
    }


    //TODO: другой механизм определения индекса для вставки. 
    //на данный момент индекс определяется так: берётся индекс строки в datatable в данный момент времени, 
    //что влечёт за собой некорректную вставку в дальнейшем
    class DeleteCommand:IDataGridCommand
    {
        private DataTable DT;
        private object[] DataRowValues;
        private int index;
        private long ID;//ну чё поделать?
        public DeleteCommand(int ind, DataTable dt)
        {
            this.index = ind; DT = dt;
            this.ID = (long)DT.Rows[index][DT.PrimaryKey[0]];
            DataRowValues = DT.Rows[index].ItemArray;
        }
        public void Execute()
        {
            this.DT.Rows.Find(this.ID).Delete();
        }
        public void Rollback()
        {
            DataRow DR = DT.NewRow();
            DR.ItemArray = DataRowValues;
            int i = DT.Rows.IndexOf(DT.Select().FirstOrDefault(x => (long)x.ItemArray[0] >= (long)DR.ItemArray[0]));
            DT.Rows.InsertAt(DR, i==-1?0:i);
        }
    }


    public class UndoRedoManager
    {
        private Stack<IDataGridCommand> UndoStack=new Stack<IDataGridCommand>();
        private Stack<IDataGridCommand> RedoStack=new Stack<IDataGridCommand>();
        private DataTable targetDT;
        public UndoRedoManager(DataTable dt)
        {this.targetDT = dt;}

        public bool Undo(int level)
        {
            for (int i = 0; i<level&&UndoStack.Count>0; i++)
            {
                IDataGridCommand idgCommand = UndoStack.Pop();
                RedoStack.Push(idgCommand);
                idgCommand.Rollback();
            }
            return UndoStack.Count > 0;
        }
        public bool Redo(int level)
        {
            for (int i = 0; i<level&&RedoStack.Count>0; i++)
            {
                IDataGridCommand idgCommand = RedoStack.Pop();
                UndoStack.Push(idgCommand);
                idgCommand.Execute();
            }
            return RedoStack.Count > 0;
        }

        public void DropStacks()
        {UndoStack.Clear();RedoStack.Clear();}

        /// <summary>
        /// Перегрузка для добавления ряда
        /// </summary>
        /// <param name="dr">добавляемый ряд</param>
        public void InsertAction(DataRow dr)
        {
            InsertCommand ic = new InsertCommand(dr, targetDT);
            UndoStack.Push(ic);
            //ic.Execute();
            RedoStack.Clear();
        }
        /// <summary>
        /// Перегрузка для обновления ячейки ряда
        /// </summary>
        /// <param name="pV">текущее значение ячейки</param>
        /// <param name="nV">новое значение ячейки</param>
        /// <param name="C">"Координата" ячейки - пара натуральных чисел [ряд:столбец]</param>
        public void UpdateAction(object pV, object nV, Tuple<long,int> C)
        {
            UpdateCommand uc = new UpdateCommand(pV, nV, C, targetDT);
            UndoStack.Push(uc);
            //uc.Execute();
            RedoStack.Clear();
        }
        /// <summary>
        /// Перегрузка для удаления ряда
        /// </summary>
        /// <param name="index">индекс ряда, который нужно удалить</param>
        public void DeleteAction(int index)
        {
            DeleteCommand dc = new DeleteCommand(index, this.targetDT);
            UndoStack.Push(dc);
            //dc.Execute();
            RedoStack.Clear();
        }
    }
}
