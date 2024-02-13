using System.Linq.Expressions;
using System.Text.Json;

namespace SAPSLFramework
{
    /// <summary>
    /// Clase base que representa una consulta a Service Layer
    /// </summary>
    public abstract class SLQuery<T>
    {
        private readonly SLContext _context;
        protected readonly Expression _queryExpression;
        protected readonly Expression _selectExpression;

        public SLQuery(SLContext context, string resource, Expression queryExpression = null, Expression selectExpression = null)
        {
            _context = context;
            _queryExpression = queryExpression;
            Resource = resource;
            _selectExpression = selectExpression;
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
            var uri = GetUri(Resource);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _context.ExecuteAsync<SLResponse<List<object>>>(request, cancellationToken);
            var jsonValues = response.Value.Select(x => JsonSerializer.Serialize(x));

            if (_selectExpression == null)
            {
                return jsonValues.Select(x => JsonSerializer.Deserialize<T>(x)).ToList();
            }

            var exp = (LambdaExpression)_selectExpression;
            var type = exp.Parameters[0].Type;
            var deleg = exp.Compile();
            var results = jsonValues.Select(x => JsonSerializer.Deserialize(x, type));
            var finalResults = results.Select(x => deleg.DynamicInvoke(x));
            return finalResults.Select(x => (T)x).ToList();
        }

        /// <summary>
        /// Obtiene la primera coincidencia
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri(Resource);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri + "&$top=1");
            var response = await _context.ExecuteAsync<SLResponse<List<object>>>(request, cancellationToken);
            var jsonValues = response.Value.Select(x => JsonSerializer.Serialize(x));

            if (_selectExpression == null)
            {
                return jsonValues.Select(x => JsonSerializer.Deserialize<T>(x)).FirstOrDefault();
            }

            var exp = (LambdaExpression)_selectExpression;
            var type = exp.Parameters[0].Type;
            var deleg = exp.Compile();
            var results = jsonValues.Select(x => JsonSerializer.Deserialize(x, type));
            var finalResults = results.Select(x => deleg.DynamicInvoke(x));
            return (T)finalResults.FirstOrDefault();
        }

        /// <summary>
        /// Determina si existe al menos una coincidencia
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri($"{Resource}/$count");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _context.ExecuteAsync<int>(request, cancellationToken);
            return response > 0;
        }

        /// <summary>
        /// Selecciona los campos a devolver
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public abstract SLQuery<I> Select<I>(Expression<Func<T, I>> selector);

        private string GetUri(string resource)
        {
            var expVisitor = new SLExpressionVisitor();
            expVisitor.Visit(_queryExpression);

            var queries = new List<string>
            {
                string.IsNullOrEmpty(expVisitor.Filter) ? null : $"$filter={expVisitor.Filter}",
                $"$select={Select()}"
            };

            var uri = $"{resource}?{string.Join('&', queries.Where(x => x != null))}";
            return uri;
        }

        private string Select()
        {
            Type type = typeof(T);
            IEnumerable<string> names;

            if (_selectExpression == null)
            {
                names = type.GetProperties()
                    .Select(x => $"{char.ToUpper(x.Name[0])}{x.Name[1..]}");
            }
            else
            {
                var exp = (LambdaExpression)_selectExpression;

                if (exp.Body is MemberInitExpression body)
                {
                    body = (MemberInitExpression)exp.Body;

                    var assigments = body.Bindings
                        .Where(x => x.BindingType == MemberBindingType.Assignment)
                        .Select(x => (MemberAssignment)x);

                    names = assigments.Select(x => ((MemberExpression)x.Expression).Member.Name);
                }
                else if(exp.Body is MemberExpression memberExp)
                {
                    names = new string[] { memberExp.Member.Name };
                }
                else
                {
                    throw new InvalidOperationException("Expresión inválida");
                }
            }

            var fields = string.Join(',', names);
            return fields;
        }
    }
}
