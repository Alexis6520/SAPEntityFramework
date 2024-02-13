using System.Linq.Expressions;

namespace SAPSLFramework
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name.ToLower())
            {
                case "startswith":
                    var memberName = ((MemberExpression)node.Object).Member.Name;
                    var argument = ((ConstantExpression)node.Arguments[0]).Value;
                    Filter += $"startswith({memberName},{GetFormattedString(argument)})";
                    break;
                default:
                    throw new InvalidOperationException("Query inválido");
            }

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
            return tipo switch
            {
                ExpressionType.Equal => "eq",
                ExpressionType.And or ExpressionType.AndAlso => "and",
                ExpressionType.Or => "or",
                ExpressionType.Not => "not",
                ExpressionType.LessThanOrEqual => "le",
                ExpressionType.LessThan => "lt",
                ExpressionType.GreaterThanOrEqual => "ge",
                ExpressionType.GreaterThan => "gt",
                ExpressionType.NotEqual => "ne",
                _ => throw new NotSupportedException($"El operador {tipo} no es compatible con Service Layer"),
            };
        }
    }
}
