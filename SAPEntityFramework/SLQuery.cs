﻿using System.Linq.Expressions;
using System.Text.Json;

namespace SAPSLFramework
{
    /// <summary>
    /// Clase base que representa una consulta a Service Layer
    /// </summary>
    public abstract class SLQuery<T>
    {
        private readonly SLContext _context;
        protected readonly IDictionary<string, Expression> _expressions;

        public SLQuery(SLContext context, string resource, IDictionary<string, Expression> expressions = null)
        {
            _expressions = expressions ?? new Dictionary<string, Expression>();
            _context = context;
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
            var uri = GetUri(Resource);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _context.ExecuteAsync<SLResponse<List<object>>>(request, cancellationToken);
            var jsonValues = response.Value.Select(x => JsonSerializer.Serialize(x));

            if (!_expressions.ContainsKey("select"))
            {
                return jsonValues.Select(x => JsonSerializer.Deserialize<T>(x)).ToList();
            }

            var exp = (LambdaExpression)_expressions["select"];
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

            if (!_expressions.ContainsKey("select"))
            {
                return jsonValues.Select(x => JsonSerializer.Deserialize<T>(x)).FirstOrDefault();
            }

            var exp = (LambdaExpression)_expressions["select"];
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
            var count = await CountAsync(cancellationToken);
            return count > 0;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri($"{Resource}/$count");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await _context.ExecuteAsync<int>(request, cancellationToken);
        }

        /// <summary>
        /// Selecciona los campos a devolver
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public abstract SLQuery<I> Select<I>(Expression<Func<T, I>> selector);

        /// <summary>
        /// Ordena los resultados por las propiedades seleccionadas
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public abstract SLQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> expression);

        /// <summary>
        /// Toma los siguientes n elementos
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public abstract SLQuery<T> Top(int n);

        /// <summary>
        /// Salta los primeros n elementos
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public abstract SLQuery<T> Skip(int n);

        private string GetUri(string resource)
        {
            var expVisitor = new SLExpressionVisitor();

            if (_expressions.ContainsKey("query"))
            {
                expVisitor.Visit(_expressions["query"]);
            }

            var select = Select();

            var queries = new List<string>
            {
                string.IsNullOrEmpty(expVisitor.Filter) ? null : $"$filter={expVisitor.Filter}",
                string.IsNullOrEmpty(select)? null : $"$select={select}",
                _expressions.ContainsKey("orderby")? $"$orderby={Order()}":null,
                _expressions.ContainsKey("top")?$"$top={Top()}":null,
                _expressions.ContainsKey("skip")?$"$skip={Skip()}":null,
            };

            var uri = $"{resource}?{string.Join('&', queries.Where(x => x != null))}";
            return uri;
        }

        private string Top()
        {
            var exp = (LambdaExpression)_expressions["top"];
            var del = exp.Compile();
            var n = (int)del.DynamicInvoke();
            return $"{n}";
        }

        private string Skip()
        {
            var exp = (LambdaExpression)_expressions["skip"];
            var del = exp.Compile();
            var n = (int)del.DynamicInvoke();
            return $"{n}";
        }

        private string Order()
        {
            var exp = (LambdaExpression)_expressions["orderby"];
            var names = new List<string>();

            if (exp.Body is MemberExpression m)
            {
                names.Add(m.Member.Name);
            }
            else if (exp.Body is NewExpression n)
            {
                names.AddRange(n.Arguments.Select(x => ((MemberExpression)x).Member.Name));
            }

            return exp != null ? string.Join(" asc,", names) : null;
        }

        private string Select()
        {
            Type type = typeof(T);
            List<string> names = new();

            if (_expressions.ContainsKey("select"))
            {
                var exp = (LambdaExpression)_expressions["select"];
                var availableProps = exp.Parameters[0].Type.GetProperties().Select(x => x.Name).ToList();

                switch (exp.Body)
                {
                    case MemberInitExpression body:
                        var assigments = body.Bindings
                            .Where(x => x.BindingType == MemberBindingType.Assignment)
                            .Select(x => (MemberAssignment)x);

                        var assigmentNames = GetNamesFromAssigments(assigments);
                        assigmentNames = assigmentNames.Where(x => availableProps.Any(y => y == x));
                        names.AddRange(assigmentNames);
                        break;
                    case MemberExpression body:
                        names = new List<string> { body.Member.Name };
                        break;
                    case NewExpression body:
                        var members = body.Arguments.Select(x => (MemberExpression)x);
                        names = members.Select(x => x.Member.Name).ToList();
                        break;
                    default:
                        throw new InvalidOperationException("Expresión select inválida");
                }

                return string.Join(',', names);
            }

            names = type.GetProperties()
                    .Select(x => $"{char.ToUpper(x.Name[0])}{x.Name[1..]}").ToList();

            return string.Join(',', names);
        }

        private static IEnumerable<string> GetNamesFromAssigments(IEnumerable<MemberAssignment> assigments)
        {
            var names = new List<string>();

            foreach (var assigment in assigments)
            {
                if (assigment.Expression is MemberExpression membExp)
                {
                    names.Add(membExp.Member.Name);
                }
                else if (assigment.Expression is MethodCallExpression methCallExp)
                {
                    foreach (var arg in methCallExp.Arguments)
                    {
                        if (arg is UnaryExpression unaryExp)
                        {
                            if (unaryExp.Operand is MemberExpression memberExp)
                            {
                                names.Add(memberExp.Member.Name);
                            }
                        }
                        else if (arg is MemberExpression memberExp)
                        {
                            names.Add(memberExp.Member.Name);
                        }
                    }
                }
            }

            return names;
        }
    }
}
