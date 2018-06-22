﻿using Kros.KORM.Converter;
using Kros.KORM.Helper;
using Kros.KORM.Injection;
using Kros.KORM.Materializer;
using Kros.KORM.Metadata.Attribute;
using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Kros.KORM.Metadata
{
    /// <summary>
    /// Model mapper, which know define convention for name mapping.
    /// </summary>
    /// <seealso cref="Kros.KORM.Metadata.IModelMapper" />
    public class ConventionModelMapper : IModelMapper
    {
        private const string ID_NAME = "ID";
        private static readonly string _onAfterMaterializeName = MethodName<IMaterialize>.GetName(p => p.OnAfterMaterialize(null));
        private Dictionary<Type, Dictionary<string, string>> _columnMap = new Dictionary<Type, Dictionary<string, string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionModelMapper"/> class.
        /// </summary>
        public ConventionModelMapper()
        {
            this.MapColumnName = (columnInfo, type) =>
            {
                return columnInfo.PropertyInfo.Name;
            };

            this.MapTableName = (tableInfo, tableType) =>
            {
                return tableType.Name;
            };

            this.MapPrimaryKey = (tableInfo) =>
            {
                return OnMapPrimaryKey(tableInfo);
            };
        }

        /// <summary>
        /// Gets or sets the column name mapping logic.
        /// </summary>
        /// <remarks>
        /// Params:
        ///     ColumnInfo - info about column.
        ///     Type - Type of model.
        ///     string - return column name.
        /// </remarks>
        public Func<ColumnInfo, Type, string> MapColumnName { get; set; }

        /// <summary>
        /// Set column name for specific property.
        /// </summary>
        /// <param name="modelProperty">Expression for defined property to.</param>
        /// <param name="columnName">Database column name.</param>
        /// <example>
        /// <code source="..\..\..\..\Documentation\Examples\Kros.KORM.Examples\ModelMapperExample.cs" title="SetColumnName" region="SetColumnName" language="cs" />
        /// </example>
        public void SetColumnName<TModel, TValue>(Expression<Func<TModel, TValue>> modelProperty, string columnName) where TModel : class
        {
            Check.NotNull(modelProperty, nameof(modelProperty));
            Check.NotNullOrEmpty(columnName, nameof(columnName));

            if (!_columnMap.ContainsKey(typeof(TModel)))
            {
                _columnMap[typeof(TModel)] = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }

            _columnMap[typeof(TModel)][PropertyName<TModel>.GetPropertyName(modelProperty)] = columnName;
        }

        /// <summary>
        /// Gets or sets the table name mapping logic.
        /// </summary>
        public Func<TableInfo, Type, string> MapTableName { get; set; }

        /// <summary>
        /// Gets or sets the primary key mapping logic.
        /// </summary>
        public Func<TableInfo, IEnumerable<ColumnInfo>> MapPrimaryKey { get; set; }

        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <returns>
        /// Table info.
        /// </returns>
        public TableInfo GetTableInfo<T>() => CreateTableInfo(typeof(T));

        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>
        /// Table info.
        /// </returns>
        public TableInfo GetTableInfo(Type modelType) => CreateTableInfo(modelType);

        #region Injection

        private Dictionary<Type, IInjector> _injectors = new Dictionary<Type, IInjector>();

        /// <summary>
        /// Get property injection configuration for model T.
        /// </summary>
        /// <example>
        /// <code source="..\..\..\..\Documentation\Examples\Kros.KORM.Examples\WelcomeExample.cs" title="Injection" region="InectionConfiguration" language="cs" />
        /// </example>
        public IInjectionConfigurator<T> InjectionConfigurator<T>()
        {
            var injector = new InjectionConfiguration<T>();

            _injectors[typeof(T)] = injector;

            return injector;
        }

        /// <summary>
        /// Get property service injector.
        /// </summary>
        /// <typeparam name="T">Model type.</typeparam>
        /// <returns>Service property injector.</returns>
        public IInjector GetInjector<T>()
        {
            return GetInjector(typeof(T));
        }

        private IInjector GetInjector(Type modelType)
        {
            if (_injectors.ContainsKey(modelType))
            {
                return _injectors[modelType];
            }
            else
            {
                return DummyInjector.Default;
            }
        }

        private class DummyInjector : IInjector
        {
            public static IInjector Default { get; } = new DummyInjector();

            public object GetValue(string propertyName) =>
                throw new NotImplementedException();

            public bool IsInjectable(string propertyName) => false;
        }

        #endregion

        #region Private Helpers

        private TableInfo CreateTableInfo(Type modelType)
        {
            var injector = GetInjector(modelType);
            var properties = GetModelProperties(modelType).
                Where(p =>
                {
                    return p.CanWrite &&
                        (p.GetCustomAttributes(typeof(NoMapAttribute), true).Length == 0) &&
                        !injector.IsInjectable(p.Name);
                });

            var columns = properties.Select(p => CreateColumnInfo(p, modelType));
            var onAfterMaterialize = GetOnAfterMaterializeInfo(modelType);
            TableInfo tableInfo = new TableInfo(columns, GetModelProperties(modelType), onAfterMaterialize);

            tableInfo.Name = GetTableName(tableInfo, modelType);

            foreach (var key in this.MapPrimaryKey(tableInfo))
            {
                key.IsPrimaryKey = true;
            }

            return tableInfo;
        }

        private static PropertyInfo[] GetModelProperties(Type modelType) =>
            modelType.GetProperties(BindingFlags.Public |
                BindingFlags.GetProperty |
                BindingFlags.SetProperty |
                BindingFlags.Instance);

        private MethodInfo GetOnAfterMaterializeInfo(Type modelType)
        {
            MethodInfo onAfterMaterialize = null;

            if (typeof(IMaterialize).IsAssignableFrom(modelType))
            {
                onAfterMaterialize = typeof(IMaterialize).GetMethod(_onAfterMaterializeName);
            }

            return onAfterMaterialize;
        }

        private string GetTableName(TableInfo tableInfo, Type modelType)
        {
            var name = GetName(modelType);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = this.MapTableName(tableInfo, modelType);
            }

            return name;
        }

        private ColumnInfo CreateColumnInfo(PropertyInfo propertyInfo, Type modelType)
        {
            var columnInfo = new ColumnInfo();

            columnInfo.PropertyInfo = propertyInfo;
            columnInfo.Name = GetColumnName(columnInfo, modelType);

            columnInfo.Converter = GetConverter(propertyInfo);

            return columnInfo;
        }

        private string GetColumnName(ColumnInfo columnInfo, Type modelType)
        {
            var name = GetName(columnInfo.PropertyInfo);

            if (_columnMap.ContainsKey(modelType) && _columnMap[modelType].ContainsKey(columnInfo.PropertyInfo.Name))
            {
                name = _columnMap[modelType][columnInfo.PropertyInfo.Name];
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = this.MapColumnName(columnInfo, modelType);
            }

            return name;
        }

        private IConverter GetConverter(PropertyInfo propertyInfo)
        {
            var attributes = propertyInfo.GetCustomAttributes(typeof(ConverterAttribute), true);
            if (attributes.Length == 1)
            {
                return (attributes[0] as ConverterAttribute).Converter;
            }
            else
            {
                return null;
            }
        }

        private string GetName(ICustomAttributeProvider attributeProvider)
        {
            AliasAttribute aliasAttr = attributeProvider.GetCustomAttributes(typeof(AliasAttribute), true)
                .FirstOrDefault() as AliasAttribute;

            return aliasAttr?.Alias;
        }

        private static IEnumerable<ColumnInfo> OnMapPrimaryKey(TableInfo tableInfo)
        {
            var ret = new List<ColumnInfo>();

            foreach (var column in tableInfo.Columns)
            {
                var attributes = column.PropertyInfo.GetCustomAttributes(typeof(KeyAttribute), true);
                if (attributes.Length == 1)
                {
                    column.AutoIncrementMethodType = (attributes[0] as KeyAttribute).AutoIncrementMethodType;
                    column.IsPrimaryKey = true;
                    ret.Add(column);
                }
                else if (column.Name.Equals(ID_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    column.IsPrimaryKey = true;
                    ret.Add(column);
                }
            }

            return ret;
        }

        #endregion
    }
}