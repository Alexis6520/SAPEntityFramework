using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;

namespace SAPSLFramework
{
    /// <summary>
    /// Proporciona acceso a un recurso de Service Layer
    /// </summary>
    /// <typeparam name="T">Tipo en el que se basa el recurso</typeparam>
    public class SLSet<T> : SLQuery<T>
    {
        private readonly SLContext _context;

        public SLSet(SLContext slContext, string resource) : base(slContext, resource)
        {
            _context = slContext;
        }

        public SLSet(SLContext slContext, string resource, IDictionary<string, Expression> expressions) : base(slContext, resource, expressions)
        {
            _context = slContext;
        }

        /// <summary>
        /// Agrega un nuevo elemento al recurso
        /// </summary>
        /// <param name="entity">Elemento a agregar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Resource)
            {
                Content = new StringContent(JsonSerializer.Serialize(entity)),
            };

            entity = await _context.ExecuteAsync<T>(request, cancellationToken);
        }

        /// <summary>
        /// Acutaliza un elemento del recurso
        /// </summary>
        /// <param name="entity">Elemento a actualizar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var values = GetKeyValue(entity);
            var uri = $"{Resource}({string.Join(',', values)})";

            using var request = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(entity))
            };

            await _context.ExecuteAsync(request, cancellationToken);
        }

        /// <summary>
        /// Elimina un elemento del recurso
        /// </summary>
        /// <param name="entity">Elemento a eliminar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            var values = GetKeyValue(entity);
            var uri = $"{Resource}({string.Join(',', values)})";
            using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            await _context.ExecuteAsync(request, cancellationToken);
        }

        public override SLQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            _expressions.Remove("select");

            if (_expressions.ContainsKey("query"))
            {
                _expressions["query"] = predicate;
            }
            else
            {
                _expressions.Add("query", predicate);
            }

            return new SLSet<T>(_context, Resource, _expressions);
        }

        public override SLQuery<I> Select<I>(Expression<Func<T, I>> selector)
        {
            if (_expressions.ContainsKey("select"))
            {
                _expressions["select"] = selector;
            }
            else
            {
                _expressions.Add("select", selector);
            }

            return new SLSet<I>(_context, Resource, _expressions);
        }

        public override SLQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> key)
        {
            if (_expressions.ContainsKey("orderby"))
            {
                _expressions["orderby"] = key;
            }
            else
            {
                _expressions.Add("orderby", key);
            }

            return new SLSet<T>(_context, Resource, _expressions);
        }

        /// <summary>
        /// Ejecuta una acción para el elemento de este recurso
        /// </summary>
        /// <param name="entity">Elemento del recurso</param>
        /// <param name="actionName">Acción a ejecutar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExecuteActionAsync(T entity, string actionName, CancellationToken cancellationToken = default)
        {
            var values = GetKeyValue(entity);
            var uri = $"{Resource}({string.Join(',', values)})/{actionName}";
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            await _context.ExecuteAsync(request, cancellationToken);
        }

        private static List<string> GetKeyValue(T entity)
        {
            var keyProperties = typeof(T).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0);

            if (!keyProperties.Any())
            {
                throw new ArgumentException($"La clase {typeof(T).Name} no tiene definida una propiedad como llave");
            }

            var values = new List<string>();

            foreach (var key in keyProperties)
            {
                object value = key.GetValue(entity) ?? throw new Exception("El valor de la llave no puede ser null");
                var keyName = $"{char.ToUpper(key.Name[0])}{key.Name[1..]}";

                if (key.PropertyType == typeof(string))
                {
                    values.Add($"{keyName}='{value}'");
                }
                else
                {
                    values.Add($"{keyName}={value}");
                }
            }

            return values;
        }
    }
}
