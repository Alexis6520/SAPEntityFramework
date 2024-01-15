using System.Net.Http.Json;
using System.Text.Json;

namespace SAPEntityFramework.Extensions.Http
{
    internal static class HttpClientExtensions
    {
        public static async Task PatchJsonAsync(this HttpClient client, string requestUri, object body, CancellationToken cancellationToken = default)
        {
            try
            {
                using var content = new StringContent(JsonSerializer.Serialize(body));
                using var response = await client.PatchAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var ex = await GetExceptionAsync(response, cancellationToken);
                    throw ex;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new SLException("No se pudo h¿realizar la petición a Service Layer", null, ex);
            }
        }

        /// <summary>
        /// Post con Json que devuelve respuesta deserializada
        /// </summary>
        /// <typeparam name="T">Tipo a devolver</typeparam>
        /// <param name="client">Cliente Http</param>
        /// <param name="requestUri">Ruta</param>
        /// <param name="body">Objeto a enviar como Json</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns></returns>
        /// <exception cref="SLException"></exception>
        public static async Task<T> PostJsonAsync<T>(this HttpClient client, string requestUri, object body, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new StringContent(JsonSerializer.Serialize(body));
                using var response = await client.PostAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var ex = await GetExceptionAsync(response, cancellationToken);
                    throw ex;
                }

                return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new SLException("No se pudo h¿realizar la petición a Service Layer", null, ex);
            }
        }

        /// <summary>
        /// Post con Json sin respuesta
        /// </summary>
        /// <typeparam name="T">Tipo a devolver</typeparam>
        /// <param name="client">Cliente Http</param>
        /// <param name="requestUri">Ruta</param>
        /// <param name="body">Objeto a enviar en el body</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns></returns>
        /// <exception cref="SLException"></exception>
        public static async Task PostJsonAsync(this HttpClient client, string requestUri, object body, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new StringContent(JsonSerializer.Serialize(body));
                using var response = await client.PostAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var ex = await GetExceptionAsync(response, cancellationToken);
                    throw ex;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new SLException("Error al realizar la petición", null, ex);
            }
        }

        /// <summary>
        /// Devuelve un objeto de una respuesta GET
        /// </summary>
        /// <typeparam name="T">Tipo a devolver</typeparam>
        /// <param name="client">Cliente Http</param>
        /// <param name="requestUri">Ruta</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns></returns>
        /// <exception cref="SLException"></exception>
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await client.GetAsync(requestUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var ex = await GetExceptionAsync(response, cancellationToken);
                    throw ex;
                }

                return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new SLException("Error al realizar la petición", null, ex);
            }
        }

        private static async Task<SLException> GetExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<SLErrorResponse>(cancellationToken: cancellationToken);
                var error = errorResponse.Error;
                return new SLException(error.Message, error.Code);
            }
            catch (Exception)
            {
                var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new SLException(stringContent, $"{response.StatusCode}");
            }
        }
    }
}
