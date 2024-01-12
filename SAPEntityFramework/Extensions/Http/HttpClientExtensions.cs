using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

namespace SAPEntityFramework.Extensions.Http
{
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// Serializa un objeto y lo envía como Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Cliente Http</param>
        /// <param name="requestUri">Ruta</param>
        /// <param name="body">Objeto</param>
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
                throw new SLException("Error al realizar la petición", null, ex);
            }
        }

        private static async Task<SLException> GetExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new SLException("Ruta no encontrada", "404");
                case HttpStatusCode.BadRequest:
                    var errorResponse = await response.Content.ReadFromJsonAsync<SLErrorResponse>(cancellationToken: cancellationToken);
                    var error = errorResponse.Error;
                    return new SLException(error.Message, error.Code);
                default:
                    var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new SLException(stringContent);
            }
        }
    }
}
