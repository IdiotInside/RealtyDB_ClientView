using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RealtyDB_ClientView.Business_logic
{
    public class EnumDictionary
    {
        public long ID { get; set; }
        public string TableName { get; set; }
        public EnumDictionary(long id, string tableName)
        { this.ID = id; this.TableName = tableName; }
    }

    public class StringDigitsOnlyValidationRule : ValidationRule
    {
        //documentType dT;
        private string propName = "";
        public StringDigitsOnlyValidationRule(string pN)
        { this.propName = pN; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isValid = true;
            string errorMessage = "";
            string X = ((string)value).Replace(" ", "");
            X = X.Replace("-", "");
            X = X.Replace("_", "");
            for (int i = 0; i < X.Length; i++)
                if (!char.IsDigit(X[i]))
                {
                    isValid = false;
                    errorMessage = string.Format("В поле \"{0}\" допускается вводить только цифры и разделители: _- и пробел", propName);
                    break;
                }
            return new ValidationResult(isValid, errorMessage);
        }
    }

    public class EstimatedLengthConstraintValidationRule : ValidationRule
    {
        private int estLength = 0;
        private string propName = "";

        //constructor
        public EstimatedLengthConstraintValidationRule(int eL, string pN)
        { this.estLength = eL; this.propName = pN; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isValid = true;
            string errorMessage = "";
            //bitch learn some regex will ya?
            string X = ((string)value).Replace(" ", "");
            X = X.Replace("-", "");
            X = X.Replace("_", "");
            if (X.Length != estLength)
            {
                isValid = false;
                errorMessage = string.Format("В поле \"{0}\" ожидается {1} цифры без учёта разделителей: _- и пробел", propName, estLength);
            }
            return new ValidationResult(isValid, errorMessage);
        }
    }

}
