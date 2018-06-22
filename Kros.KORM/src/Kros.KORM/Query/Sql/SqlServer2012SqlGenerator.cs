using Kros.KORM.Metadata;
using Kros.KORM.Query.Providers;

namespace Kros.KORM.Query.Sql
{
    public class SqlServer2012SqlGenerator : DefaultQuerySqlGenerator
    {
        public SqlServer2012SqlGenerator(IDatabaseMapper databaseMapper) : base(databaseMapper)
        {
        }

        protected override void AddLimitAndOffset()
        {
            if (Skip == 0)
            {
                base.AddLimitAndOffset();
            }
            else
            {
                SqlBuilder.AppendFormat(" OFFSET {0} ROWS", Skip);
                if (Top > 0)
                {
                    SqlBuilder.AppendFormat(" FETCH NEXT {0} ROWS ONLY", Top);
                }
            }
        }

        protected override IDataReaderEnvelope CreateQueryReader() => null;
    }
}
