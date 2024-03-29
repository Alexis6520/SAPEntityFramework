﻿using System.Linq.Expressions;

namespace SAPSLFramework
{
    internal class SLExpressionVisitor : ExpressionVisitor
    {
        private readonly string[] _queryFunctions = new string[] { "startswith", "endswith", "contains" };

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
            if (node.Expression == null || node.Expression.NodeType == ExpressionType.Constant || node.Expression.NodeType == ExpressionType.MemberAccess)
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
            var methodName = node.Method.Name.ToLower();

            if (_queryFunctions.Any(x => x == methodName))
            {
                var memberName = ((MemberExpression)node.Object).Member.Name;
                object argument = null;

                if (node.Arguments[0] is ConstantExpression consExp)
                {
                    argument = consExp.Value;
                }
                else 
                {
                    var memExp = (MemberExpression)node.Arguments[0];
                    var objectMember = Expression.Convert(memExp, typeof(object));
                    var getter = Expression.Lambda<Func<object>>(objectMember).Compile();
                    argument = getter();
                }

                Filter += $"{methodName}({memberName},{GetFormattedString(argument)})";
            }
            else
            {
                throw new InvalidOperationException("Query inválido");
            }

            return node;
        }

        private static string GetFormattedString(object obj)
        {
            if (obj is string s)
            {
                return $"'{s}'";
            }
            else if (obj is DateTime d)
            {
                return $"'{d:yyyy-MM-ddTHH:mm:ss}'";
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
                ExpressionType.OrElse => "or",
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
