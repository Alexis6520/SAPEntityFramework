using SAPSLFramework.Extensions.Http;
using System.Linq.Expressions;

namespace SAPSLFramework
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
            var uri = GetUri(expression, typeof(T));
            var response = await Context.HttpClient.GetJsonAsync<SLResponse<List<T>>>(uri, CancellationToken.None);
            return response.Value;
        }

        public async Task<T> ExecuteFirstOrDefaultAsync<T>(Expression expression, CancellationToken cancellationToken = default)
        {
            await Context.LoginAsync(cancellationToken: cancellationToken);
            var uri = GetUri(expression, typeof(T));
            var response = await Context.HttpClient.GetJsonAsync<SLResponse<List<T>>>(uri, CancellationToken.None);
            return response.Value.FirstOrDefault();
        }

        private string GetUri(Expression expression, Type type)
        {
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(expression);
            var filter = $"$filter={expVisitor.Filter}";
            var select = $"$select={Select(type)}";
            var uri = $"{Path}?{filter}&{select}";
            return uri;
        }

        private static string Select(Type type)
        {
            if (!type.IsClass)
            {
                throw new NotSupportedException("Tipo de dato no soportado");
            }

            var names = type.GetProperties()
                .Select(x => $"{char.ToUpper(x.Name[0])}{x.Name[1..]}");

            var fields = string.Join(',', names);
            return fields;
        }
    }
}
