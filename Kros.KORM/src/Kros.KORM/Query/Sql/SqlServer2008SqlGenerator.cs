using System.Linq.Expressions;
using Kros.KORM.Metadata;
using Kros.KORM.Query.Expressions;
using Kros.KORM.Query.Providers;

namespace Kros.KORM.Query.Sql
{
    public class SqlServer2008SqlGenerator : DefaultQuerySqlGenerator
    {
        public SqlServer2008SqlGenerator(IDatabaseMapper databaseMapper) : base(databaseMapper)
        {
        }

        private string _orderByString;

        public override Expression VisitOrderBy(OrderByExpression orderByExpression)
        {
            if (Skip == 0)
            {
                _orderByString = string.Empty;
                return base.VisitOrderBy(orderByExpression);
            }
            else
            {
                _orderByString = CreateOrderByString(orderByExpression);
                return orderByExpression;
            }
        }

        private const string CteQueryOffset =
            "WITH Results_CTE AS ({0}) SELECT * FROM Results_CTE WHERE __RowNum__ > {1}";
        private const string CteQueryLimitOffset =
            "WITH Results_CTE AS ({0}) SELECT * FROM Results_CTE WHERE __RowNum__ > {1} AND __RowNum__ <= {2}";

        protected override void AddLimitAndOffset()
        {
            if (Skip == 0)
            {
                base.AddLimitAndOffset();
            }
            else
            {
                if (!string.IsNullOrEmpty(_orderByString))
                {
                    SqlBuilder.Insert(ColumnsPosition, $", ROW_NUMBER() OVER({_orderByString}) AS __RowNum__");
                }
                string baseSql = SqlBuilder.ToString();
                SqlBuilder.Clear();
                if (Top > 0)
                {
                    SqlBuilder.AppendFormat(CteQueryLimitOffset, baseSql, Skip, Skip + Top);
                }
                else
                {
                    SqlBuilder.AppendFormat(CteQueryOffset, baseSql, Skip);
                }
            }
        }

        protected override IDataReaderEnvelope CreateQueryReader() => null;
    }
}
