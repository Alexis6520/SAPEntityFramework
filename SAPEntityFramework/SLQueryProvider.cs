using SAPEntityFramework.Extensions.Http;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Xml.Xsl;

namespace SAPEntityFramework
{
    public class SLQueryProvider : IQueryProvider
    {
        public SLQueryProvider(SLContext context, string path)
        {
            Context = context;
            Path = path;
        }

        public SLContext Context { get; }
        public string Path { get; }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SLSet<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Context.Login();
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(expression);
            var filter = $"$filter={expVisitor.Filter}";
            var uri = $"{Path}?{filter}";
            var type = typeof(TResult);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var task = Context.HttpClient.GetJsonAsync<SLResponse<TResult>>(uri, CancellationToken.None);
                task.Wait();
                return task.Result.Value;
            }
            else
            {
                var task = Context.HttpClient.GetJsonAsync<SLResponse<List<TResult>>>(uri, CancellationToken.None);
                task.Wait();
                return task.Result.Value.FirstOrDefault();
            }
        }
    }
}
