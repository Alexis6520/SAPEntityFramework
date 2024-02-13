using System.Linq.Expressions;

namespace SAPSLFramework
{
    /// <summary>
    /// Clase base que representa una consulta a Service Layer
    /// </summary>
    public abstract class SLQuery<T>
    {
        private readonly SLContext _context;
        private readonly Expression _queryExpression;

        public SLQuery(SLContext context, string resource, Expression queryExpression = null)
        {
            _context = context;
            _queryExpression = queryExpression;
            Resource = resource;
        }

        public string Resource { get; private set; }

        /// <summary>
        /// Filtra la consulta
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public abstract SLQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Realiza la consulta y devuelve una lista con los resultados
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri(Resource, typeof(T));
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _context.ExecuteAsync<SLResponse<List<T>>>(request, cancellationToken);
            return response.Value;
        }

        /// <summary>
        /// Obtiene la primera coincidencia
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri(Resource, typeof(T));
            using var request = new HttpRequestMessage(HttpMethod.Get, uri + "&$top=1");
            var response = await _context.ExecuteAsync<SLResponse<List<T>>>(request, cancellationToken);
            return response.Value.FirstOrDefault();
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri($"{Resource}/$count", typeof(T));
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _context.ExecuteAsync<int>(request, cancellationToken);
            return response > 0;
        }

        private string GetUri(string resource, Type type)
        {
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(_queryExpression);

            var queries = new List<string>
            {
                string.IsNullOrEmpty(expVisitor.Filter) ? null : $"$filter={expVisitor.Filter}",
                $"$select={Select(type)}"
            };

            var uri = $"{resource}?{string.Join('&', queries.Where(x => x != null))}";
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
