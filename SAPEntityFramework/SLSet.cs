using SAPEntityFramework.Extensions.Http;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace SAPEntityFramework
{
    /// <summary>
    /// Proporciona acceso a un recurso de Service Layer
    /// </summary>
    /// <typeparam name="T">Tipo en el que se basa el recurso</typeparam>
    public class SLSet<T> : IQueryable<T>
    {
        private readonly SLContext _slContext;
        private readonly string _path;

        public SLSet(SLContext slContext, string path)
        {
            _slContext = slContext;
            _path = path;
            Expression = Expression.Constant(this);
            Provider = new SLQueryProvider(_slContext, path);
        }

        public SLSet(SLQueryProvider queryProvider, Expression predicate)
        {
            _slContext = queryProvider.Context;
            _path = queryProvider.Path;
            Expression = predicate;
            Provider = queryProvider;
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; internal set; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var keyProperties = typeof(T).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0);

            if (!keyProperties.Any())
            {
                throw new ArgumentException($"La clase {typeof(T).Name} no tiene definida una propiedad como llave");
            }

            var values= new List<string>();

            foreach ( var key in keyProperties )
            {
                object value = key.GetValue(entity) ?? throw new Exception("El valor de la llave no puede ser null");

                if (key.PropertyType == typeof(string))
                {
                    values.Add($"{key.Name}='{value}'");
                }
                else
                {
                    values.Add($"{key.Name}={value}");
                }
            }

            var uri = $"{_path}({string.Join(',', values)})";
            await _slContext.HttpClient.PatchJsonAsync(uri, entity, cancellationToken);
        }
    }
}
