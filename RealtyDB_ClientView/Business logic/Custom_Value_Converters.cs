
using System;
using System.Globalization;
using System.Windows.Data;
namespace RealtyDB_ClientView.Business_logic
{
    public abstract class baseEnumConverter
    {
        protected Type theEnumType;
        public baseEnumConverter(Type anEnumType)
        { this.theEnumType = anEnumType; }
    }

    public class EnumTextblockConverter : baseEnumConverter, IValueConverter
    {
        public EnumTextblockConverter(Type T) : base(T)
        { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { return value.GetType().IsEquivalentTo(typeof(DBNull)) ? "Выебрите Ъ" : Enum.ToObject(theEnumType, value).ToString(); }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}