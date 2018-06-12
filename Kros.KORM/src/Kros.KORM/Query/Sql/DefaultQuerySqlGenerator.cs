﻿using Kros.KORM.Metadata;
using Kros.KORM.Query.Expressions;
using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Kros.KORM.Query.Sql
{
    /// <summary>
    /// Default sql query visitor for generating SELECT command.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.Sql.ISqlExpressionVisitor" />
    public class DefaultQuerySqlGenerator : ExpressionVisitor, ISqlExpressionVisitor
    {
        private const string VbOperatorsClassName = "Microsoft.VisualBasic.CompilerServices.Operators";
        private const string VbEmbeddedOperatorsClassName = "Microsoft.VisualBasic.CompilerServices.EmbeddedOperators";

        private StringBuilder _sqlBuilder = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="databaseMapper">Database mapper.</param>
        public DefaultQuerySqlGenerator(IDatabaseMapper databaseMapper)
        {
            Check.NotNull(databaseMapper, nameof(databaseMapper));

            DatabaseMapper = databaseMapper;
        }

        /// <summary>
        /// Gets the database mapper.
        /// </summary>
        protected IDatabaseMapper DatabaseMapper { get; private set; }

        /// <summary>
        /// Generates the SQL from expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>SQL select command text.</returns>
        public virtual string GenerateSql(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));
            _wasAny = false;
            _sqlBuilder = new StringBuilder();
            _top = 0;
            LinqParameters = new Parameters();
            _orders = new List<OrderBy>();

            this.Visit(expression);

            AddOrderBy();
            AddAnyMethod();

            return _sqlBuilder.ToString();
        }

        private void AddAnyMethod()
        {
            if (_wasAny)
            {
                _sqlBuilder = new StringBuilder(BindAnyCondition(_sqlBuilder.ToString()));
            }
        }

        /// <summary>
        /// Adds any method to query.
        /// </summary>
        protected virtual string BindAnyCondition(string existsCondition)
                  => $"SELECT (CASE WHEN EXISTS({existsCondition}) THEN 1 ELSE 0 END)";

        /// <summary>
        /// Visits the SQL.
        /// </summary>
        /// <param name="sqlExpression">The SQL expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitSql(SqlExpression sqlExpression)
        {
            _sqlBuilder.Append(sqlExpression.Sql);

            return sqlExpression;
        }

        /// <summary>
        /// Visits the select.
        /// </summary>
        /// <param name="selectExpression">The select expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitSelect(SelectExpression selectExpression)
        {
            _sqlBuilder.Append(SelectExpression.SelectStatement);
            _sqlBuilder.Append(" ");

            if (_top > 0)
            {
                _sqlBuilder.Append($"TOP {_top} ");
            }

            this.VisitExtension(selectExpression);

            return selectExpression.Reduce();
        }

        /// <summary>
        /// Visits the columns.
        /// </summary>
        /// <param name="columnExpression">The column expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitColumns(ColumnsExpression columnExpression)
        {
            _sqlBuilder.Append(columnExpression.ColumnsPart);

            return columnExpression;
        }

        /// <summary>
        /// Visits the table.
        /// </summary>
        /// <param name="tableExpression">The table expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitTable(TableExpression tableExpression)
        {
            _sqlBuilder.AppendFormat(" {0} {1}", TableExpression.FromStatement, tableExpression.TablePart);

            return tableExpression;
        }

        /// <summary>
        /// Visits the where.
        /// </summary>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitWhere(WhereExpression whereExpression)
        {
            _sqlBuilder.AppendFormat(" {0} ({1})", WhereExpression.WhereStatement, whereExpression.Sql);

            return whereExpression;
        }

        /// <summary>
        /// Visits the group by.
        /// </summary>
        /// <param name="groupByExpression">The group by expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitGroupBy(GroupByExpression groupByExpression)
        {
            _sqlBuilder.AppendFormat(" {0} {1}", GroupByExpression.GroupByStatement, groupByExpression.GroupByPart);

            return groupByExpression;
        }

        /// <summary>
        /// Visits the order by.
        /// </summary>
        /// <param name="orderByExpression">The order by expression.</param>
        /// <returns>
        /// Expression
        /// </returns>
        public virtual Expression VisitOrderBy(OrderByExpression orderByExpression)
        {
            _sqlBuilder.AppendFormat(" {0} {1}", OrderByExpression.OrderByStatement, orderByExpression.OrderByPart);

            return orderByExpression;
        }

        #region LINQ

        /// <summary>
        /// Get root select expression.
        /// </summary>
        protected SelectExpression SelectExpression { get; private set; }

        /// <summary>
        /// Gets the linq string builder.
        /// </summary>
        protected StringBuilder LinqStringBuilder { get; private set; }
        private int _top = 0;

        /// <summary>
        /// Gets the linq query parameters.
        /// </summary>
        protected Parameters LinqParameters { get; private set; }

        private bool CanBeEvaluatedLocally(Expression expression)
        {
            if (expression is ConstantExpression cex)
            {
                if (cex.Value is IQueryable query && query.Provider == this)
                    return false;
            }
            if (expression is MethodCallExpression mc &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable)))
                return false;

            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;

            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        private static Expression ThrowNotSupportedException(MethodCallExpression expression) =>
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", expression.Method.Name));

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private SelectExpression FindSelectExpression(MethodCallExpression expression)
        {
            if (expression.Arguments[0] is SelectExpression selectExpression)
            {
                return selectExpression;
            }
            else if (expression.Arguments[0] is MethodCallExpression methodCallExpression)
            {
                return FindSelectExpression(methodCallExpression);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        public override Expression Visit(Expression node)
        {
            if (node is BinaryExpression binExp)
            {
                if (binExp.Left is MethodCallExpression mcExp && IsVbOperatorsExpression(mcExp))
                {
                    return base.Visit(VisitVbOperatorsMethods(mcExp, binExp.NodeType));
                }
            }

            return base.Visit(node);
        }

        private static bool IsVbOperatorsExpression(MethodCallExpression mcExp)
            => ((mcExp.Method.DeclaringType.FullName == VbOperatorsClassName) ||
                                mcExp.Method.DeclaringType.FullName == VbEmbeddedOperatorsClassName);

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The method call expression.</param>
        /// <returns>
        /// Reuced expression
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var expression = PartialEvaluator.Eval(m, CanBeEvaluatedLocally) as MethodCallExpression;

            if (expression.Method.DeclaringType == typeof(Queryable))
            {
                VisitLinqMethods(expression);

                return this.Visit(expression.Arguments[0]);
            }

            if (expression.Method.DeclaringType == typeof(string))
            {
                return VisitStringMethods(expression);
            }

            return ThrowNotSupportedException(expression);
        }

        /// <summary>
        /// Visit Visual Basic operators expressions.
        /// </summary>
        /// <param name="expression">Method call expression</param>
        /// <param name="binExpNodeType">Binary expression node type</param>
        /// <returns>
        /// Reduced expression.
        /// </returns>
        protected virtual Expression VisitVbOperatorsMethods(MethodCallExpression expression, ExpressionType binExpNodeType)
        {
            var methodName = expression.Method.Name;

            if (methodName.MatchMethodName("CompareString")) return VisitCompare(expression, binExpNodeType);

            return ThrowNotSupportedException(expression);
        }

        /// <summary>
        /// Visit Visual Basic Compare expression
        /// </summary>
        /// <param name="expression">Method call expression Compare expression</param>
        /// <param name="binExpNodeType">Binary expression node type</param>
        /// <returns>
        /// Reduced expression.
        /// </returns>
        protected virtual Expression VisitCompare(MethodCallExpression expression, ExpressionType binExpNodeType)
        {
            var left = expression.Arguments[0];
            var right = expression.Arguments[1];
            switch (binExpNodeType)
            {
                case ExpressionType.Equal:
                    return BinaryExpression.Equal(left, right);

                case ExpressionType.NotEqual:
                    return BinaryExpression.NotEqual(left, right);

                case ExpressionType.LessThan:
                    return BinaryExpression.LessThan(left, right);

                case ExpressionType.LessThanOrEqual:
                    return BinaryExpression.LessThanOrEqual(left, right);

                case ExpressionType.GreaterThan:
                    return BinaryExpression.GreaterThan(left, right);

                case ExpressionType.GreaterThanOrEqual:
                    return BinaryExpression.GreaterThanOrEqual(left, right);
            }

            return ThrowNotSupportedException(expression);
        }

        private void VisitLinqMethods(MethodCallExpression expression)
        {
            SelectExpression = FindSelectExpression(expression);
            LinqStringBuilder = new StringBuilder();

            VisitLinqExpression(expression);
        }

        /// <summary>
        /// Visits the linq expression.
        /// </summary>
        /// <param name="expression">The method call expression.</param>
        /// <returns>
        /// Reduced expression.
        /// </returns>
        protected virtual Expression VisitLinqExpression(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;

            if (methodName.MatchMethodName("Where")) return VisitWhere(expression);
            if (methodName.MatchMethodName("First")) return VisitFirst(expression);
            if (methodName.MatchMethodName("FirstOrDefault")) return VisitFirst(expression);
            if (methodName.MatchMethodName("Single")) return VisitFirst(expression);
            if (methodName.MatchMethodName("SingleOrDefault")) return VisitFirst(expression);
            if (methodName.MatchMethodName("Take")) return VisitTake(expression);
            if (methodName.MatchMethodName("GroupBy")) return VisitGroupBy(expression);
            if (methodName.MatchMethodName("Select")) return VisitSelect(expression);
            if (methodName.MatchMethodName("OrderBy")) return VisitOrderBy(expression, OrderType.Ascending);
            if (methodName.MatchMethodName("OrderByDescending")) return VisitOrderBy(expression, OrderType.Descending);
            if (methodName.MatchMethodName("ThenBy")) return VisitOrderBy(expression, OrderType.Ascending);
            if (methodName.MatchMethodName("ThenByDescending")) return VisitOrderBy(expression, OrderType.Descending);
            if (methodName.MatchMethodName("Count")) return VisitCount(expression);
            if (methodName.MatchMethodName("Min")) return VisitAggregate(expression, "Min");
            if (methodName.MatchMethodName("Max")) return VisitAggregate(expression, "Max");
            if (methodName.MatchMethodName("Sum")) return VisitAggregate(expression, "Sum");
            if (methodName.MatchMethodName("Any")) return VisitAny(expression);

            return ThrowNotSupportedException(expression);
        }

        private bool _wasAny;

        /// <summary>
        /// Visits the Linq Any method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitAny(MethodCallExpression expression)
        {
            if (expression.Arguments.Count > 1)
            {
                VisitWhere(expression);
            }
            SelectExpression.SetColumnsExpression(new ColumnsExpression("''"));
            _wasAny = true;

            return expression;
        }

        /// <summary>
        /// Visits the Linq Take method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <exception cref="System.NotSupportedException">If call of Take method is not supported.</exception>
        protected virtual Expression VisitTake(MethodCallExpression expression)
        {
            this.Visit(StripQuotes(expression.Arguments[1]));
            LinqStringBuilder.Clear();

            try
            {
                _top = (int)LinqParameters.GetParams().First();

                LinqParameters = new Parameters();
                return expression;
            }
            catch (Exception e)
            {
                throw new NotSupportedException("This call of Take method is not supported", e);
            }
        }

        /// <summary>
        /// Visits the Linq aggregate methods.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="aggregateName">Name of aggreage method.</param>
        protected virtual Expression VisitAggregate(MethodCallExpression expression, string aggregateName)
        {
            LinqStringBuilder.Append($"{aggregateName}(");
            this.Visit(StripQuotes(expression.Arguments[1]));
            LinqStringBuilder.Append(")");

            SelectExpression.SetColumnsExpression(new ColumnsExpression(LinqStringBuilder.ToString()));
            LinqStringBuilder.Clear();

            return expression;
        }

        /// <summary>
        /// Visits the Linq Count method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression VisitCount(MethodCallExpression expression)
        {
            if (expression.Arguments.Count > 1)
            {
                VisitWhere(expression);
            }
            SelectExpression.SetColumnsExpression(new ColumnsExpression("COUNT(*)"));

            return expression;
        }

        /// <summary>
        /// Order type.
        /// </summary>
        protected enum OrderType
        {
            /// <summary>
            /// The ascending.
            /// </summary>
            Ascending,
            /// <summary>
            /// The descending.
            /// </summary>
            Descending
        }

        /// <summary>
        /// Class which has information about order statement.
        /// </summary>
        protected class OrderBy
        {
            /// <summary>
            /// Gets or sets the name of the columna.
            /// </summary>
            public string ColumnaName { get; set; }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            public OrderType Type { get; set; }
        }

        private List<OrderBy> _orders;

        private Expression VisitOrderBy(MethodCallExpression expression, OrderType orderType)
        {
            var lambda = (LambdaExpression)StripQuotes(expression.Arguments[1]);

            var ret = this.Visit(lambda);
            _orders.Add(new OrderBy() { ColumnaName = LinqStringBuilder.ToString(), Type = orderType });
            LinqStringBuilder.Clear();

            return ret;
        }

        private Expression VisitSelect(MethodCallExpression expression)
            => ThrowNotSupportedException(expression);

        private Expression VisitGroupBy(MethodCallExpression expression) =>
            ThrowNotSupportedException(expression);

        /// <summary>
        /// Visits the Linq Where method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression VisitWhere(MethodCallExpression expression)
        {
            LambdaExpression lambda = (LambdaExpression)StripQuotes(expression.Arguments[1]);
            this.Visit(lambda.Body);

            SelectExpression.SetWhereExpression(new WhereExpression(LinqStringBuilder.ToString(), LinqParameters.GetParams()));
            LinqParameters.Clear();
            LinqStringBuilder = null;

            return expression;
        }

        /// <summary>
        /// Visits the Linq First.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression VisitFirst(MethodCallExpression expression)
        {
            if (expression.Arguments.Count > 1)
            {
                VisitWhere(expression);
            }

            _top = 1;

            return expression;
        }

        /// <summary>
        /// Visits the unary.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">If this unary expression is not supported.</exception>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    LinqStringBuilder.Append(" NOT ");
                    this.Visit(expression.Operand);
                    break;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    this.Visit(expression.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", expression.NodeType));
            }

            return expression;
        }

        /// <summary>
        /// Visits the Linq Binary.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">If this binary expression is not supported.</exception>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            LinqStringBuilder.Append("(");

            this.Visit(expression.Left);

            var op = GetOperator(expression);

            switch (expression.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.ExclusiveOr:
                    LinqStringBuilder.Append($" {op} ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported.", expression.NodeType));
            }

            this.Visit(expression.Right);

            LinqStringBuilder.Append(")");

            return expression;
        }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual string GetOperator(BinaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Equal:
                    return IsCompareToNull(expression) ? "IS" : "=";
                case ExpressionType.NotEqual:
                    return IsCompareToNull(expression) ? "IS NOT" : "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                default:
                    return "";
            }

            bool IsCompareToNull(BinaryExpression exp) =>
                IsNullable(exp) && (exp.Right is ConstantExpression constExpr) && constExpr.Value == null;

            bool IsNullable(BinaryExpression exp) =>
                Nullable.GetUnderlyingType(exp.Left.Type) != null;
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <exception cref="System.NotSupportedException">If type of constant is <see cref="System.Object"/>.</exception>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value == null)
            {
                LinqStringBuilder.Append("NULL");
            }
            else
            {
                var type = expression.Value.GetType();

                if (Type.GetTypeCode(type) == TypeCode.Object && (type != typeof(Guid)))
                {
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", expression.Value));
                }
                else
                {
                    LinqStringBuilder.Append(LinqParameters.GetNextParamName());
                    LinqParameters.AddParameter(expression.Value);
                }
            }

            return expression;
        }

        /// <summary>
        /// Visits the member.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <exception cref="System.NotSupportedException">If the member type is not supported.</exception>
        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression != null &&
                (expression.Expression.NodeType == ExpressionType.Parameter ||
                 expression.Expression.NodeType == ExpressionType.Convert))
            {
                var columnInfo = DatabaseMapper.GetTableInfo(expression.Member.DeclaringType).GetColumnInfoByPropertyName(expression.Member.Name);
                LinqStringBuilder.Append(columnInfo.Name);
                return expression;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported.", expression.Member.Name));
        }

        /// <summary>
        /// Visits the string methods.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <exception cref="System.NotSupportedException">If this <see cref="System.String"/> method is not supported.</exception>
        protected virtual Expression VisitStringMethods(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "StartsWith":
                    return BindStartWith(expression);
                case "EndsWith":
                    return BindEndWith(expression);
                case "Contains":
                    return BindContains(expression);
                case "IsNullOrEmpty":
                    return BindIsNullOrEmpty(expression);
                case "ToUpper":
                    return BindToUpper(expression);
                case "ToLower":
                    return BindToLower(expression);
                case "Replace":
                    return BindReplace(expression);
                case "Substring":
                    return BindSubstring(expression);
                case "Trim":
                    return BindTrim(expression);
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", expression.Method.Name));
        }

        /// <summary>
        /// Binds the <see cref="string.Trim()"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindTrim(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("RTRIM(LTRIM(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append("))");
            return expression;
        }

        /// <summary>
        /// Binds the <see cref="string.Substring(int)" autoUpgrade="true" /> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindSubstring(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("SUBSTRING(");
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
        /// Binds the <see cref="string.Replace(string, string)"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindReplace(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("REPLACE(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(", ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(", ");
            this.Visit(expression.Arguments[1]);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <summary>
        /// Binds to <see cref="string.ToLower()"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindToLower(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("LOWER(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <summary>
        /// Binds to <see cref="string.ToUpper()"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindToUpper(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("UPPER(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <summary>
        /// Binds the <see cref="string.IsNullOrEmpty(string)"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindIsNullOrEmpty(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("(");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(" IS NULL OR ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(" = '')");
            return expression;
        }

        /// <summary>
        /// Binds the <see cref="string.Contains(string)"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindContains(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(" LIKE '%' + ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(" + '%')");
            return expression;
        }

        /// <summary>
        /// Binds the <see cref="string.EndsWith(string)"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindEndWith(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(" LIKE '%' + ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(")");
            return expression;
        }

        /// <summary>
        /// Binds the <see cref="string.StartsWith(string)"/> method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual Expression BindStartWith(MethodCallExpression expression)
        {
            LinqStringBuilder.Append("(");
            this.Visit(expression.Object);
            LinqStringBuilder.Append(" LIKE ");
            this.Visit(expression.Arguments[0]);
            LinqStringBuilder.Append(" + '%')");
            return expression;
        }

        /// <summary>
        /// Class which help with Linq query parameters.
        /// </summary>
        protected class Parameters
        {
            private int _parametersCount = 0;
            private List<object> _params = new List<object>();

            /// <summary>
            /// Gets the name of the next parameter.
            /// </summary>
            public string GetNextParamName()
                => $"@{++_parametersCount}";

            /// <summary>
            /// Adds the parameter.
            /// </summary>
            /// <param name="param">The parameter.</param>
            public void AddParameter(object param)
                => _params.Add(param);

            /// <summary>
            /// Gets the parameters.
            /// </summary>
            public object[] GetParams()
                => _params.ToArray();

            /// <summary>
            /// Clears this instance.
            /// </summary>
            public void Clear() => _params.Clear();
        }

        private void AddOrderBy()
        {
            if (_orders.Count > 0)
            {
                string TypeToString(OrderType type) => type == OrderType.Ascending ? "ASC" : "DESC";

                VisitOrderBy(
                    new OrderByExpression(
                        string.Join(", ", _orders.AsEnumerable<OrderBy>().Reverse()
                            .Select(p => $"{p.ColumnaName} {TypeToString(p.Type)}"))));
            }
        }

        #endregion
    }

    internal static class StringExtension
    {
        public static bool MatchMethodName(this string methodName, string expected) =>
            methodName.Equals(expected, StringComparison.InvariantCultureIgnoreCase);
    }
}
