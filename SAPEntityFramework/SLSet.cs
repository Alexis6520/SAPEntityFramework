using SAPEntityFramework.Extensions.Http;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SAPEntityFramework
{
    public class SLSet<T> where T : class
    {
        private readonly SLContext _slContext;
        private readonly string _path;

        public SLSet(SLContext slContext, string path)
        {
            _slContext = slContext;
            _path = path;
        }

        /// <summary>
        /// Obtiene un elemento por Id
        /// </summary>
        /// <param name="key">Id</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns></returns>
        public async Task<T> FindAsync(object key, CancellationToken cancellationToken = default)
        {
            await _slContext.LoginAsync(cancellationToken: cancellationToken);

            var keyProperty = typeof(T).GetProperties()
                .FirstOrDefault(x => x.GetCustomAttribute<KeyAttribute>() != null);

            string filter;

            if (keyProperty.PropertyType == typeof(string))
            {
                filter = $"('{key}')";
            }
            else
            {
                filter = $"({key})";
            }

            return await _slContext.HttpClient.GetJsonAsync<T>($"{_path}{filter}", cancellationToken);
        }
    }
}
