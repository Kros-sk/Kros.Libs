using Kros.KORM.Metadata;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Kros.KORM.Query.Sql
{
    /// <summary>
    /// Generator sql query for SQL server.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.Sql.DefaultQuerySqlGenerator" />
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="databaseMapper">Database mapper.</param>
        public SqlServerQuerySqlGenerator(IDatabaseMapper databaseMapper)
            : base(databaseMapper)
        {
        }

        /// <summary>
        /// Generates the SQL from expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// SQL select command text.
        /// </returns>
        public override string GenerateSql(Expression expression)
        {
            var sql = base.GenerateSql(expression);

            return SqlClientChecker.CheckSql(sql);
        }

        private class SqlClientChecker
        {
            public static string CheckSql(string iSql)
            {
                if (!string.IsNullOrEmpty(iSql))
                {
                    iSql = ReplaceEx(iSql, Environment.NewLine, " ");
                    iSql = Replace(iSql, "FALSE", "0", RegexOptions.IgnoreCase);
                    iSql = Replace(iSql, "TRUE", "1", RegexOptions.IgnoreCase);
                    iSql = Replace(iSql, "\\bNow\\(\\)", "GetDate()", RegexOptions.IgnoreCase);
                    iSql = ReplaceEx(iSql, "&", "+");
                    iSql = SqlIif(iSql);
                    iSql = SQLDatum(iSql);
                    iSql = Replace(iSql, "\\bUCase\\(", "UPPER(", RegexOptions.IgnoreCase);
                    iSql = Replace(iSql, "\\bMID\\(", "SUBSTRING(", RegexOptions.IgnoreCase);
                    iSql = Replace(iSql, "\\bLEN\\(", "DATALENGTH(", RegexOptions.IgnoreCase);
                    iSql = SqlDateIif(iSql);
                    iSql = SqlBitAnd(iSql);
                    iSql = SqlBitMap(iSql);
                    iSql = Replace(iSql, " PAAPI ", " & ", RegexOptions.IgnoreCase);
                }
                return iSql;
            }

            private static string ReplaceEx(string iSql, string iCo, string iHodnota)
            {
                StringBuilder hodnota = new StringBuilder();
                StringBuilder text = new StringBuilder();
                StringBuilder ret = new StringBuilder();
                int start;
                int kon = 0;
                int i;

                if ((iSql.Length == 0))
                    return iSql;
                i = 0;
                while ((i <= iSql.Length))
                {
                    start = iSql.IndexOf("'", i, StringComparison.Ordinal);
                    if ((start > 0))
                    {
                        kon = iSql.IndexOf("'", start + 1, StringComparison.Ordinal);
                    }
                    if ((start > 0) & (kon > 0))
                    {
                        hodnota.AppendClear(iSql.Substring(i, start - i));
                        text.AppendClear(iSql.Substring(start, kon - start + 1));
                        i = kon + 1;
                    }
                    else
                    {
                        hodnota.AppendClear(iSql.Substring(i, iSql.Length - i));
                        text.Clear();
                        i = iSql.Length + 1;
                    }
                    ret.AppendClear(string.Format("{0}{1}{2}", ret, hodnota.Replace(iCo, iHodnota), text));
                }

                return ret.ToString();
            }

            private static string SqlIif(string query)
            {
                StringBuilder ret = new StringBuilder();
                const int iifLength = 4; // "IIF("

                int iifIndex;

                ret.Append(query);
                iifIndex = query.IndexOf("IIF(", StringComparison.OrdinalIgnoreCase);
                while (iifIndex > 0)
                {
                    iifIndex += iifLength;
                    (int level, int iifStart, int iifTrue, int iifFalse, int iifEnd) = GetIifParts(query, iifIndex);
                    if (level != 0)
                    {
                        return ret.ToString();
                    }
                    ret.Clear();
                    ret.Append(query.Substring(0, iifStart - iifLength).Trim());
                    ret.Append(" (CASE WHEN ");
                    ret.Append(query.Substring(iifStart, iifTrue - iifStart - 1).Trim());
                    ret.Append(" THEN ");
                    ret.Append(query.Substring(iifTrue, iifFalse - iifTrue - 1).Trim());
                    ret.Append(" ELSE ");
                    ret.Append(query.Substring(iifFalse, iifEnd - (iifFalse)).Trim());
                    ret.Append(" END)");

                    if (iifEnd < query.Length)
                    {
                        ret.Append(query.Substring(iifEnd + 1));
                    }

                    query = ret.ToString();
                    iifIndex = query.IndexOf("IIF(", StringComparison.OrdinalIgnoreCase);
                }

                return ret.ToString();
            }

            private static (int level, int iifStart, int iifTrue, int iifFalse, int iifEnd)
                GetIifParts(string query, int currentCharIndex)
            {
                const int iifPartStart = 0;
                const int iifPartTrue = 1;
                const int iifPartFalse = 2;
                const int iifPartEnd = 3;

                int[] iifParts = new int[4];
                iifParts[iifPartStart] = currentCharIndex;

                int iifPart = iifPartTrue;
                int level = 1;

                while ((level != 0) & (currentCharIndex < query.Length))
                {
                    char currentChar = query[currentCharIndex];
                    if (currentChar == '(')
                    {
                        level++;
                    }
                    else if (currentChar == ')')
                    {
                        level--;
                        if (level == 0)
                        {
                            iifParts[iifPartEnd] = currentCharIndex;
                        }
                    }
                    else if (currentChar == ',')
                    {
                        if (level == 1)
                        {
                            iifParts[iifPart] = currentCharIndex + 1;
                            iifPart++;
                        }
                    }
                    currentCharIndex++;
                }

                return (level, iifParts[iifPartStart], iifParts[iifPartTrue], iifParts[iifPartFalse], iifParts[iifPartEnd]);
            }

            private static string SQLDatum(string iSql)
            {
                StringBuilder ret = new StringBuilder();
                double dateNumber;
                int i;
                int j;

                ret.Append(iSql);
                i = iSql.IndexOf("CDate(", 1, StringComparison.OrdinalIgnoreCase);
                while ((i > 0))
                {
                    j = iSql.IndexOf(")", i + 6, StringComparison.OrdinalIgnoreCase);
                    if ((j == 0))
                        break;
                    ret.AppendClear(iSql.Substring(i + 6, j - (i + 6)));
                    ret.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                    ret.Replace(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                    dateNumber = (double.Parse(ret.ToString()) - 2);
                    ret.AppendClear(iSql.Substring(0, i));
                    ret.AppendClear(string.Format("{0}CONVERT(DateTime, {1})", ret, VratCisloSql(dateNumber.ToString())));
                    ret.AppendClear(string.Format("{0}{1}", ret, iSql.Substring(j + 1)));
                    iSql = ret.ToString();
                    i = iSql.IndexOf("CDate(", i + 6, StringComparison.OrdinalIgnoreCase);
                }

                return ret.ToString();
            }

            public static string VratCisloSql(string iNumber)
            {

                if ((iNumber.Length == 0))
                {
                    return iNumber;
                }
                else
                {
                    return iNumber.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
                }
            }

            private static string SqlDateIif(string iSql)
            {
                StringBuilder text = new StringBuilder();
                int i;
                int j;
                StringBuilder ret = new StringBuilder();

                ret.Append(iSql);
                i = ret.ToString().IndexOf("DateDiff(", 0, StringComparison.OrdinalIgnoreCase);
                while ((i >= 0))
                {
                    j = ret.ToString().IndexOf(",", i + 9, StringComparison.OrdinalIgnoreCase);
                    ret.AppendClear(iSql.Substring(0, (i + 9)));
                    for (i = (i + 9); i <= j; i++)
                    {
                        text.AppendClear(iSql.Substring(i, 1));
                        if ((text.ToString() != "'"))
                        {
                            ret.Append(text);
                        }
                    }
                    ret.Append(iSql.Substring(j + 1));
                    iSql = ret.ToString();
                    i = ret.ToString().IndexOf("DateDiff(", j, StringComparison.OrdinalIgnoreCase);
                }

                return ret.ToString();
            }

            private static string SqlBitAnd(string iSql)
            {
                StringBuilder hodnota = new StringBuilder();
                StringBuilder stlpec = new StringBuilder();
                int i;
                int j;
                int k;
                StringBuilder ret = new StringBuilder();

                ret.Append(iSql);
                i = ret.ToString().IndexOf("BITAND(", 0, StringComparison.OrdinalIgnoreCase);
                while ((i > 0))
                {
                    j = ret.ToString().IndexOf(",", i + 7, StringComparison.OrdinalIgnoreCase);
                    stlpec.AppendClear(iSql.Substring(i + 7, j - (i + 7)).Trim());
                    k = ret.ToString().IndexOf(")", j + 1, StringComparison.OrdinalIgnoreCase);
                    hodnota.AppendClear(iSql.Substring(j + 1, k - (j + 1)).Trim());
                    ret.AppendClear(string.Format("{0}(({1} & {2}) = {2}){3}", iSql.Substring(0, i - 1), stlpec, hodnota, iSql.Substring(k + 1)));
                    iSql = ret.ToString();
                    i = ret.ToString().IndexOf("BITAND(", 0, StringComparison.OrdinalIgnoreCase);
                }

                return ret.ToString();
            }

            private static string SqlBitMap(string iSql)
            {
                StringBuilder hodnota = new StringBuilder();
                StringBuilder stlpec = new StringBuilder();
                StringBuilder maska = new StringBuilder();
                StringBuilder ret = new StringBuilder();
                int i;
                int j;
                int k;

                ret.Append(iSql);
                i = ret.ToString().IndexOf("BITMAP(", 0, StringComparison.OrdinalIgnoreCase);
                while ((i > 0))
                {
                    j = ret.ToString().IndexOf(",", i + 7, StringComparison.OrdinalIgnoreCase);
                    stlpec.AppendClear(iSql.Substring(i + 7, j - (i + 7)).Trim());
                    k = ret.ToString().IndexOf(",", j + 1, StringComparison.OrdinalIgnoreCase);
                    maska.AppendClear(iSql.Substring(j + 1, k - (j + 1)).Trim());
                    j = k;
                    k = ret.ToString().IndexOf(")", j + 1, StringComparison.OrdinalIgnoreCase);
                    hodnota.AppendClear(iSql.Substring(j + 1, k - (j + 1)).Trim());
                    ret.AppendClear(string.Format("{0}(({1} & {2}) = {3}){4}", iSql.Substring(0, i - 1), stlpec, maska, hodnota, iSql.Substring(k + 1)));
                    iSql = ret.ToString();
                    i = ret.ToString().IndexOf("BITMAP(", 0, StringComparison.OrdinalIgnoreCase);
                }

                return ret.ToString();
            }

            public static string Replace(string iText, string iInput, string iReplacement, RegexOptions iOptions)
            {
                if (iText == null)
                {
                    return iText;
                }
                else
                {
                    return Regex.Replace(iText, iInput, iReplacement, iOptions);
                }
            }

        }
    }

    internal static class StringBuilderExtensions
    {

        /// <summary>
        /// Clear and append.
        /// </summary>
        /// <param name="iBuilder">The i builder.</param>
        /// <param name="iValue">The i value.</param>
        /// <returns></returns>
        public static StringBuilder AppendClear(this StringBuilder iBuilder, string iValue)
        {
            if ((iBuilder.Length > 0))
            {
                iBuilder.Clear();
            }
            iBuilder.Append(iValue);

            return iBuilder;
        }

    }
}
