using Kros.Caching;
using Kros.Data;
using Kros.Data.BulkActions;
using Kros.Data.Schema;
using Kros.KORM.Data;
using Kros.KORM.Helper;
using Kros.KORM.Materializer;
using Kros.KORM.Query.Sql;
using Kros.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Kros.KORM.Query
{
    /// <summary>
    /// Provider, which know execute query.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.IQueryProvider" />
    public abstract class QueryProvider : IQueryProvider
    {

        #region Nested types

        private class IdGeneratorHelper : IIdGenerator
        {
            private readonly DbConnection _connection;
            private readonly IIdGenerator _idGenerator;

            public IdGeneratorHelper(IIdGenerator idGenerator, DbConnection connection)
            {
                _idGenerator = idGenerator;
                _connection = connection;
            }

            public int GetNext() => _idGenerator.GetNext();

            #region IDisposable Support

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

            private bool _disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _idGenerator.Dispose();
                        _connection.Dispose();
                    }

                    _disposedValue = true;
                }
            }


            public void Dispose() => Dispose(true);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

            #endregion
        }

        #endregion


        #region Constants

        private const string RETURN_VALUE_PARAM_NAME = "returnValue";

        #endregion


        #region Private fields

        private readonly ILogger _logger;
        private readonly ISqlExpressionVisitor _sqlGenerator;
        private readonly IModelBuilder _modelBuilder;
        private readonly ConnectionStringSettings _connectionSettings = null;
        private DbConnection _connection = null;
        private readonly Cache<string, TableSchema> _tableSchemas =
            new Cache<string, TableSchema>(StringComparer.OrdinalIgnoreCase);
        private MethodInfo _nonGenericMaterializeMethod = null;
        private Lazy<TransactionHelper> _transactionHelper;

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProvider" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection string settings.</param>
        /// <param name="sqlGenerator">The SQL generator.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="logger">The logger.</param>
        public QueryProvider(
            ConnectionStringSettings connectionSettings,
            ISqlExpressionVisitor sqlGenerator,
            IModelBuilder modelBuilder,
            ILogger logger)
        {
            Check.NotNull(connectionSettings, nameof(connectionSettings));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
            _connectionSettings = connectionSettings;
            IsExternalConnection = false;
            _sqlGenerator = sqlGenerator;
            _modelBuilder = modelBuilder;
            _transactionHelper = new Lazy<TransactionHelper>(() => new TransactionHelper(Connection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProvider" /> class.
        /// </summary>
        /// <param name="externalConnection">The connection.</param>
        /// <param name="sqlGenerator">The SQL generator.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="logger">The logger.</param>
        public QueryProvider(
            DbConnection externalConnection,
            ISqlExpressionVisitor sqlGenerator,
            IModelBuilder modelBuilder,
            ILogger logger)
        {
            Check.NotNull(externalConnection, nameof(externalConnection));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
            _connection = externalConnection;
            IsExternalConnection = true;
            _sqlGenerator = sqlGenerator;
            _modelBuilder = modelBuilder;
            _transactionHelper = new Lazy<TransactionHelper>(() => new TransactionHelper(Connection));
        }

        #endregion


        #region IQueryProvider

        /// <summary>
        /// Returns <see cref="DbProviderFactory"/> for current provider.
        /// </summary>
        public abstract DbProviderFactory DbProviderFactory { get; }

        /// <inheritdoc cref="IQueryProvider.SetParameterDbType(DbParameter, string, string)"/>
        public void SetParameterDbType(DbParameter parameter, string tableName, string columnName)
        {
            TableSchema table = _tableSchemas.Get(tableName, () => LoadTableSchema(tableName));
            if (!table.Columns.Contains(columnName))
            {
                throw new InvalidOperationException(string.Format(
                    "Could not get data type for column [{0}].[{1}]. Column [{1}] does not exist in table [{0}].",
                    tableName, columnName));
            }
            table.Columns[columnName].SetParameterDbType(parameter);
            parameter.Size = table.Columns[columnName].Size;
        }

        private TableSchema LoadTableSchema(string tableName)
        {
            IDatabaseSchemaLoader schemaLoader = GetSchemaLoader();
            TableSchema tableSchema = schemaLoader.LoadTableSchema(Connection, tableName);
            return tableSchema
                ?? throw new InvalidOperationException($"Could not get schema for table [{tableName}]. Table does not exists.");
        }

        /// <summary>
        /// Creates <see cref="IDatabaseSchemaLoader"/> for specific database.
        /// </summary>
        protected abstract IDatabaseSchemaLoader GetSchemaLoader();

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <typeparam name="T">Type of model result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>
        /// IEnumerable of models, which was materialized by query
        /// </returns>
        /// <exception cref="ArgumentNullException">If query is null.</exception>
        public IEnumerable<T> Execute<T>(IQuery<T> query)
        {
            Check.NotNull(query, nameof(query));

            Data.ConnectionHelper cnHelper = OpenConnection();
            DbCommand command = CreateCommand(query.Expression);
            _logger.LogCommand(command);
            IDataReader reader = new ModelBuilder.QueryDataReader(command, cnHelper.CloseConnection);
            return _modelBuilder.Materialize<T>(reader);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns a maximum of 2033 characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">If query is null.</exception>
        public object ExecuteScalar<T>(IQuery<T> query)
        {
            Check.NotNull(query, nameof(query));

            using (var cnHelper = OpenConnection())
            using (var command = CreateCommand(query.Expression))
            {
                _logger.LogCommand(command);
                var ret = command.ExecuteScalar();
                return ret;
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            using (OpenConnection())
            using (var transaction = _transactionHelper.Value.BeginTransaction())
            {
                try
                {
                    await action();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteNonQueryCommand(IDbCommand command)
        {
            Check.NotNull(command, nameof(command));
            _logger.LogCommand(command);

            return command.ExecuteNonQuery();
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteNonQueryCommandAsync(DbCommand command)
        {
            Check.NotNull(command, nameof(command));
            _logger.LogCommand(command);

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes arbitrary query.
        /// </summary>
        /// <param name="query">Arbitrary SQL query. It should not be SELECT query.</param>
        /// <returns>
        /// Number of affected rows.
        /// </returns>
        public int ExecuteNonQuery(string query)
        {
            return ExecuteNonQuery(query, null);
        }

        /// <summary>
        /// Executes arbitrary query with parameters.
        /// </summary>
        /// <param name="query">Arbitrary SQL query. It should not be SELECT query.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>
        /// Number of affected rows.
        /// </returns>
        /// <exception cref="ArgumentException">Value of any of the parameters is NULL and its data type
        /// (<see cref="CommandParameter.DataType"/>) is not set.</exception>
        public int ExecuteNonQuery(string query, CommandParameterCollection parameters)
        {
            CheckCommandParameters(parameters);

            using (OpenConnection())
            using (DbCommand command = CreateCommand(query, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc cref="IQueryProvider.ExecuteStoredProcedure{TResult}(string)"/>
        public TResult ExecuteStoredProcedure<TResult>(string storedProcedureName)
        {
            return ExecuteStoredProcedure<TResult>(storedProcedureName, null);
        }

        /// <inheritdoc cref="IQueryProvider.ExecuteStoredProcedure{TResult}(string, CommandParameterCollection)"/>
        public TResult ExecuteStoredProcedure<TResult>(string storedProcedureName, CommandParameterCollection parameters)
        {
            CheckCommandParameters(parameters);

            TResult result = default(TResult);

            Data.ConnectionHelper cnHelper = null;
            DbCommand command = null;
            bool callDispose = true;

            try
            {
                cnHelper = OpenConnection();
                command = CreateCommand(storedProcedureName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                _logger.LogCommand(command);

                DbParameter returnParameter = GetOrAddCommandReturnParameter(command);

                if (typeof(IEnumerable).IsAssignableFrom(typeof(TResult)))
                {
                    IDataReader reader = new ModelBuilder.QueryDataReader(command, cnHelper.CloseConnection);
                    result = MaterializeStoredProcedureResult<TResult>(reader);
                    callDispose = false;
                }
                else
                {
                    result = MaterializeStoredProcedureResult<TResult>(command, returnParameter);
                }
                FillOutputParameters(command, parameters);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (callDispose)
                {
                    command?.Dispose();
                    cnHelper?.Dispose();
                }
            }

            return result;
        }

        private TResult MaterializeStoredProcedureResult<TResult>(DbCommand command, DbParameter returnParameter)
        {
            TResult result = default(TResult);

            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    if (typeof(TResult).IsClass && (typeof(TResult) != typeof(string)))
                    {
                        result = _modelBuilder.Materialize<TResult>(reader).FirstOrDefault();
                    }
                    else
                    {
                        reader.Read();
                        result = (TResult)reader.GetValue(0);
                    }
                }
                else if (returnParameter != null)
                {
                    result = (TResult)returnParameter.Value;
                }
            }

            return result;
        }

        private void FillOutputParameters(DbCommand sourceCommand, CommandParameterCollection parameters)
        {
            if (parameters != null)
            {
                foreach (CommandParameter param in parameters)
                {
                    if ((param.Direction == ParameterDirection.Output) || (param.Direction == ParameterDirection.InputOutput))
                    {
                        param.Value = sourceCommand.Parameters[param.ParameterName].Value;
                    }
                }
            }
        }

        private TResult MaterializeStoredProcedureResult<TResult>(IDataReader reader)
        {
            Type tresult = typeof(TResult);
            if (!tresult.IsGenericType)
            {
                throw new InvalidOperationException("Result type must be generic: IEnumerable<T>");
            }
            if (_nonGenericMaterializeMethod == null)
            {
                _nonGenericMaterializeMethod = _modelBuilder.GetType().GetMethod(
                    nameof(IModelBuilder.Materialize), new Type[] { typeof(IDataReader) });
                if (_nonGenericMaterializeMethod == null)
                {
                    string modelBuilderType = _modelBuilder.GetType().FullName;
                    string methodName = nameof(IModelBuilder.Materialize);
                    throw new InvalidOperationException(
                        $"ModelBuilder instance ({modelBuilderType}) does not have method {methodName}(IDataReader).");
                }
            }
            MethodInfo materializeMethod = _nonGenericMaterializeMethod.MakeGenericMethod(tresult.GenericTypeArguments[0]);

            return (TResult)materializeMethod.Invoke(_modelBuilder, new object[] { reader });
        }

        /// <summary>
        /// Creates instance of <see cref="IBulkInsert"/>.
        /// </summary>
        /// <returns>Instance of <see cref="IBulkInsert"/>.</returns>
        public abstract IBulkInsert CreateBulkInsert();

        /// <summary>
        /// Creates instance of <see cref="IBulkUpdate"/>.
        /// </summary>
        /// <returns>Instance of <see cref="IBulkUpdate"/>.</returns>
        public abstract IBulkUpdate CreateBulkUpdate();

        /// <summary>
        /// Vytvorí inicializovaný príkaz <see cref="DbCommand"/>, pre aktuálnu transakciu.
        /// Používa sa iba v rámci volania <see cref="ExecuteInTransaction(Action)"/>.
        /// </summary>
        /// <returns>Inicializovaný príkaz.</returns>
        public DbCommand GetCommandForCurrentTransaction() => _transactionHelper.Value.CreateCommand();

        /// <inheritdoc/>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel) =>
            _transactionHelper.Value.BeginTransaction(isolationLevel);

        /// <inheritdoc/>
        public IIdGenerator CreateIdGenerator(string tableName, int batchSize)
        {
            var connection = (Connection as ICloneable).Clone() as DbConnection;
            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(@"There was a failure to open the database connection at the time " +
                    "the primary keys are generated. Try add 'Persist Security Info = TRUE' to connection string.", ex);
            }

            var factory = IdGeneratorFactories.GetFactory(connection);

            return new IdGeneratorHelper(factory.GetGenerator(tableName, batchSize), connection);
        }

        #endregion


        #region Linq

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <exception cref="NotImplementedException"></exception>
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException($"Creating non generic query is actualy not supported.");
        }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable`1"></see> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements of the <see cref="T:System.Linq.IQueryable`1"></see> that is returned.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1"></see> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new Query<TElement>(this, expression);

        /// <summary>
        /// Executing non generic result is not actualy supported.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <exception cref="NotImplementedException"></exception>
        public object Execute(Expression expression)
        {
            throw new NotImplementedException("Executing non generic result is not actualy supported.");
        }

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TResult">The type of the value that results from executing the query.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return this.Execute(new Query<TResult>(this, expression)).FirstOrDefault();
        }

        #endregion


        #region Helpers

        /// <summary>
        /// Gets current the transaction.
        /// </summary>
        /// <returns>Current transaction if is opened; otherwise null;</returns>
        protected DbTransaction GetCurrentTransaction() => _transactionHelper.Value.CurrentTransaction;

        /// <summary>
        /// Connection string na databázu, ktorý bol zadaný pri vytvorení inštancie triedy
        /// (<see cref="QueryProvider.QueryProvider(ConnectionStringSettings, ISqlExpressionVisitor, IModelBuilder, ILogger)"/>).
        /// Ak bola trieda vytvorená konkrétnou inštanciou spojenia, vráti <c>null</c>.
        /// </summary>
        protected string ConnectionString { get => _connectionSettings?.ConnectionString; }

        /// <summary>
        /// Vráti, či spojenie na databázu je externé, tzn. či bolo explicitne zadané zvonka v konštruktore.
        /// Ak bolo spojenie zadané explicitne, vráti <see langword="true"/>, ak bol v konštruktore zadaný iba connection
        /// string a spojenie je vytvorené interne, vráti <see langword="false"/>.
        /// </summary>
        protected bool IsExternalConnection { get; }

        /// <summary>
        /// Vráti spojenie na databázu s ktorou trieda pracuje. Ak trieda bola vytvorená iba so zadaným
        /// connection string-om, je vytvorené nové spojenie.
        /// </summary>
        protected DbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = DbProviderFactory.CreateConnection();
                    _connection.ConnectionString = _connectionSettings.ConnectionString;
                }
                return _connection;
            }
        }

        private Data.ConnectionHelper OpenConnection()
        {
            return new Data.ConnectionHelper(Connection);
        }

        private DbCommand CreateCommand(Expression expression)
        {
            var command = _transactionHelper.Value.CreateCommand();

            command.CommandText = _sqlGenerator.GenerateSql(expression);
            ParameterExtractingExpressionVisitor.ExtractParametersToCommand(command, expression);

            return command;
        }

        private DbCommand CreateCommand(string commandText, CommandParameterCollection parameters)
        {
            DbCommand command = _transactionHelper.Value.CreateCommand();
            command.CommandText = commandText;

            if (parameters?.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    AddCommandParameter(command, parameter);
                }
            }

            return command;
        }

        private void AddCommandParameter(DbCommand command, CommandParameter commandParameter)
        {
            DbParameter dbParameter = command.CreateParameter();
            dbParameter.ParameterName = commandParameter.ParameterName;
            dbParameter.Value = commandParameter.Value;
            dbParameter.Direction = commandParameter.Direction;
            if (commandParameter.DataType.HasValue)
            {
                dbParameter.DbType = commandParameter.DataType.Value;
            }
            command.Parameters.Add(dbParameter);
        }

        private DbParameter GetOrAddCommandReturnParameter(DbCommand command)
        {
            DbParameter returnParameter = GetReturnParameter(command);
            if (returnParameter == null)
            {
                returnParameter = command.CreateParameter();
                returnParameter.ParameterName = RETURN_VALUE_PARAM_NAME;
                returnParameter.Value = null;
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnParameter);
            }

            return returnParameter;
        }

        private DbParameter GetReturnParameter(DbCommand command)
        {
            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Direction == ParameterDirection.ReturnValue)
                {
                    return parameter;
                }
            }
            return null;
        }

        private void CheckCommandParameters(CommandParameterCollection parameters)
        {
            if (parameters != null)
            {
                foreach (CommandParameter parameter in parameters)
                {
                    if ((!parameter.DataType.HasValue) && ((parameter.Value == null) || (parameter.Value == DBNull.Value)))
                    {
                        throw new ArgumentException(
                            string.Format("Data type of the parameter must be set, when its value is NULL. Parameter name: {0}.",
                            parameter.ParameterName));
                    }
                }
            }
        }

        #endregion


        #region IDisposable

        private bool _disposedValue = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if ((_connection != null) && (!IsExternalConnection))
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}