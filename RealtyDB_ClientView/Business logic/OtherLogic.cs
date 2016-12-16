using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtyDB_ClientView.misc
{
    public static class OtherLogic
    {
        public static object[] getSubArray(object[] originalArray, int start, int end)
        {
            object[] rezult = new object[end - start];
            for (int x = start, i = 0; x < end; x++, i++)
                rezult[i] = originalArray[x];
            return rezult;
        }
        public static string ParseEndSign(ref string x)
        {
            string t = x.Substring(0, x.LastOrDefault(a => a == '>'));
            return t == "" ? x : t;
        }
    }
    
    public enum TableGroup
    {
        /// <summary>
        /// Деталей ващпе не будет. Подходит для вспомогательных таблиц
        /// </summary>
        None=0,
        /// <summary>
        /// Используется для предложения объекта недвижимости, все листбоксы будут с радиобаттонами.
        /// </summary>
        Realty_Offer,
        /// <summary>
        /// Используется для спросов объекта недвижимости. все листбоксы будут с чекбоксами
        /// </summary>
        Realty_Demand,
        /// <summary>
        /// ТУДУ
        /// </summary>
        Client,
        /// <summary>
        /// ТУДУ
        /// </summary>
        Employee,
        /// <summary>
        /// ТУДУ
        /// </summary>
        Service

    }
    public enum ApplicationType:byte
    {
        Предложение=0,
        Спрос=1
    }
    public enum EnumerationSource:byte
    {
        Enumeration=1,
        DataTable=2
    }

}
