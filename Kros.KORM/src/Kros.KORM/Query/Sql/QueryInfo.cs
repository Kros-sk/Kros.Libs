namespace Kros.KORM.Query.Sql
{
    public class QueryInfo
    {
        public QueryInfo(string query)
            : this(query, 0, 0)
        {
        }

        public QueryInfo(string query, int limit, int offset)
        {
            Query = query;
            Limit = limit;
            Offset = offset;
        }

        public string Query { get; }
        public int Limit { get; }
        public int Offset { get; }
    }
}
