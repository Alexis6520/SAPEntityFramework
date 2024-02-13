using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;

namespace SAPSLFramework
{
    /// <summary>
    /// Proporciona acceso a un recurso de Service Layer
    /// </summary>
    /// <typeparam name="T">Tipo en el que se basa el recurso</typeparam>
    public class SLSet<T> : SLQuery<T> where T : class
    {
        private readonly SLContext _context;

        public SLSet(SLContext sl_context, string resource) : base(sl_context, resource)
        {
            _context = sl_context;
        }

        public SLSet(SLContext sl_context, string resource, Expression defaultExpression) : base(sl_context, resource, defaultExpression)
        {
            _context = sl_context;
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
            return new SLSet<T>(_context, Resource, predicate);
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
