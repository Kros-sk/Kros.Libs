using System.Linq.Expressions;
using Kros.KORM.Metadata;

namespace Kros.KORM.Query.Sql.MsAccess
{
    /// <summary>
    /// Generator sql query for MS Access.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.Sql.DefaultQuerySqlGenerator" />
    public class MsAccessQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessQuerySqlGenerator"/> class.
        /// </summary>
        /// <param name="databaseMapper">Database mapper.</param>
        public MsAccessQuerySqlGenerator(IDatabaseMapper databaseMapper)
            : base(databaseMapper)
        {
        }

        /// <inheritdoc/>
        protected override Expression BindTrim(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("TRIM(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <inheritdoc/>
        protected override Expression BindToUpper(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("UCASE(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <inheritdoc/>
        protected override Expression BindToLower(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("LCASE(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <inheritdoc/>
        protected override Expression BindSubstring(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("MID(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(", ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(" + 1, ");
            if (expression.Arguments.Count == 2)
            {
                this.Visit(expression.Arguments[1]);
            }
            else
            {
                LinqStringBuilder.Append(LinqParameters.GetNextParamName());
                LinqParameters.AddParameter(8000);
            }
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <summary>
        /// Adds any method to query.
        /// </summary>
        protected override string BindAnyCondition(string existsCondition)
            => $"SELECT TOP 1 IIF(EXISTS({existsCondition}), 1, 0) FROM {SelectExpression.TableExpression.TablePart}";
    }
}
