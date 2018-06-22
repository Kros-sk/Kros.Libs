namespace Kros.KORM.Query.Providers
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
