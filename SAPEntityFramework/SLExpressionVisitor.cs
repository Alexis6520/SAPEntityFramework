using System.Linq.Expressions;
using System.Text;

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
            Filter+=node.Member.Name;
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var type = node.Value.GetType();

            if (type.IsClass && type!=typeof(string))
            {
                return node;
            }

            if (node.Value.GetType() == typeof(string))
            {
                Filter+="'" + node.Value.ToString() + "'";
            }
            else
            {
                Filter+=$"{node.Value}";
            }

            return node;
        }

        private static string GetOperator(ExpressionType tipo)
        {
            switch (tipo)
            {
                case ExpressionType.Equal:
                    return "eq";
                default:
                    throw new NotSupportedException($"El operador {tipo} no es compatible.");
            }
        }
    }
}
