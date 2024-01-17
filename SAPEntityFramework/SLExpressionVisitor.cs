using System.Linq.Expressions;
using System.Reflection;

namespace SAPEntityFramework
{
    internal class SLExpressionVisitor : ExpressionVisitor
    {
        public SLExpressionVisitor()
        {
            Filter = string.Empty;
        }

        public string Filter { get; private set; }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Filter += '(';
            Visit(node.Left);
            Filter += ' ';
            Filter += GetOperator(node.NodeType);
            Filter += ' ';
            Visit(node.Right);
            Filter += ')';
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.MemberAccess || node.Expression.NodeType == ExpressionType.Constant)
            {
                var objectMember = Expression.Convert(node, typeof(object));
                var getter = Expression.Lambda<Func<object>>(objectMember).Compile();
                var value = getter();
                Filter += GetFormattedString(value);
            }
            else
            {
                Filter += $"{char.ToUpper(node.Member.Name[0])}{node.Member.Name[1..]}";
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var type = node.Value.GetType();

            if (type.IsClass && type != typeof(string))
            {
                return node;
            }

            Filter += GetFormattedString(node.Value);
            return node;
        }

        private static string GetFormattedString(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return $"'{obj}'";
            }
            else
            {
                return $"{obj}";
            }
        }

        private static string GetOperator(ExpressionType tipo)
        {
            switch (tipo)
            {
                case ExpressionType.Equal:
                    return "eq";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Or:
                    return "or";
                case ExpressionType.Not:
                    return "not";
                case ExpressionType.LessThanOrEqual:
                    return "le";
                case ExpressionType.LessThan:
                    return "lt";
                case ExpressionType.GreaterThanOrEqual:
                    return "ge";
                case ExpressionType.GreaterThan:
                    return "gt";
                case ExpressionType.NotEqual:
                    return "ne";
                default:
                    throw new NotSupportedException($"El operador {tipo} no es compatible con Service Layer");
            }
        }
    }
}
