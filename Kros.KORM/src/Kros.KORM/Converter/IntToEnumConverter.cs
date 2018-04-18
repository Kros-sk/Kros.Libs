using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kros.KORM.Converter
{
    /// <summary>
    /// Converter, which know convert int from Db to enum value
    /// </summary>
    /// <seealso cref="Kros.KORM.Converter.IConverter" />
    internal class IntToEnumConverter : IConverter
    {
        private Type _enumType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntToEnumConverter"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <exception cref="System.ArgumentNullException">enumType;Argument 'enumType' is required.</exception>
        /// <exception cref="System.ArgumentException">Type must be enum.;enumType</exception>
        public IntToEnumConverter(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType","Argument 'enumType' is required.");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type must be enum.", "enumType");
            }

            _enumType = enumType;
        }

        /// <summary>
        /// Converts the specified int value from Db to Clr enum value.
        /// </summary>
        /// <param name="value">The int value.</param>
        /// <returns>
        /// Converted enum value for Clr.
        /// </returns>
        public object Convert(object value)
        {
            return Enum.ToObject(_enumType, value);
        }

        /// <summary>
        /// Converts the enum value from Clr to Db int value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>
        /// Converted int value for Db
        /// </returns>
        public object ConvertBack(object value)
        {
            return (int)value;
        }
    }
}
