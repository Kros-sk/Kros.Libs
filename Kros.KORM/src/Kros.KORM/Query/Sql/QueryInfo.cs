using Kros.KORM.Query.Providers;

namespace Kros.KORM.Query.Sql
{
    public class QueryInfo
    {
        public QueryInfo(string query)
            : this(query, null)
        {
        }

        public QueryInfo(string query, IDataReaderEnvelope reader)
        {
            Query = query;
            Reader = reader;
        }

        public string Query { get; }
        public IDataReaderEnvelope Reader { get; }
    }
}
