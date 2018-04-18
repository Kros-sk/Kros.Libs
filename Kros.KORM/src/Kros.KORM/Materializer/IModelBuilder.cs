using System.Collections.Generic;
using System.Data;

namespace Kros.KORM.Materializer
{

    /// <summary>
    /// Interface for ModelBuilder, which know materialize data from Ado to objects.
    /// </summary>
    /// <example>
    /// <code source="..\Examples\Kros.KORM.Examples\IModelBuilderExample.cs"
    ///       title="Materialize data table"
    ///       region="ModelBuilderExample" lang="C#"  />
    /// </example>
    public interface IModelBuilder
    {
        /// <summary>
        /// Materialize data from reader to instances of model type.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="reader">The reader from which materialize data.</param>
        /// <returns>
        ///  IEnumerable of models.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IModelBuilderExample.cs"
        ///       title="Materialize data table"
        ///       region="ModelBuilderDataTableExample" lang="C#"  />
        /// </example>
        /// <remarks>Doesn' call dispose over reader.</remarks>
        IEnumerable<T> Materialize<T>(IDataReader reader);

        /// <summary>
        /// Materialize data from data table to instances of model type .
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="table">The table.</param>
        /// <returns>
        /// IEnumerable of models.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IModelBuilderExample.cs"
        ///       title="Materialize data reader"
        ///       region="ModelBuilderReaderExample" lang="C#"  />
        /// </example>
        IEnumerable<T> Materialize<T>(DataTable table);

        /// <summary>
        /// Materialize data from <paramref name="dataRow"/> to instances of model type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="dataRow">Data row of the table.</param>
        /// <returns>
        /// Model.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IModelBuilderExample.cs"
        ///       title="Materialize data row"
        ///       region="ModelBuilderDataRowExample" lang="C#"  />
        /// </example>
        T Materialize<T>(DataRow dataRow);
    }
}