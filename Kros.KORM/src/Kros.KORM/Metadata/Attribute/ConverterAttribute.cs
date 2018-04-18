using Kros.Caching;
using Kros.KORM.Converter;
using System;

namespace Kros.KORM.Metadata.Attribute
{
    /// <summary>
    /// Attribute for getting data converter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConverterAttribute : System.Attribute
    {
        private static ICache<Type, IConverter> _converters = new Cache<Type, IConverter>();
        private Type _converterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        /// <exception cref="System.ArgumentNullException">converterType;Argument 'converterType' is required.</exception>
        public ConverterAttribute(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException("converterType", "Argument 'converterType' is required.");
            }
            if (!typeof(IConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException("Argument 'converterType' must implement IConverter.", "converterType");
            }

            _converterType = converterType;
        }

        /// <summary>
        /// Gets the converter for property.
        /// </summary>
        public IConverter Converter
        {
            get
            {
                return _converters.Get(_converterType, () => Activator.CreateInstance(_converterType) as IConverter);
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        internal static void ClearCache()
        {
            _converters.Clear();
        }
    }
}
