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
            throw new NotImplementedException();
        }

        public async Task<List<T>> ExecuteToListAsync<T>(Expression expression, CancellationToken cancellationToken = default)
        {
            await Context.LoginAsync(cancellationToken: cancellationToken);
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(expression);
            var filter = $"$filter={expVisitor.Filter}";
            var uri = $"{Path}?{filter}";

            var response = await Context.HttpClient.GetJsonAsync<SLResponse<List<T>>>(uri, CancellationToken.None);
            return response.Value;
        }

        public async Task<T> ExecuteFirstOrDefaultAsync<T>(Expression expression, CancellationToken cancellationToken = default)
        {
            await Context.LoginAsync(cancellationToken: cancellationToken);
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(expression);
            var filter = $"$filter={expVisitor.Filter}";
            var uri = $"{Path}?{filter}";

            var response = await Context.HttpClient.GetJsonAsync<SLResponse<List<T>>>(uri, CancellationToken.None);
            return response.Value.FirstOrDefault();
        }
    }
}
