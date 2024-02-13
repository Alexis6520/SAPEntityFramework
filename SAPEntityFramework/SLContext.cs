using System.Net.Http.Json;
using System.Text.Json;

namespace SAPSLFramework
{
    /// <summary>
    /// Clase base para crear un contexto de SAP Service Layer.
    /// </summary>
    public abstract class SLContext : IDisposable
    {
        private SLContextOptions _options;
        private SLSession _session;
        private readonly HttpClient _httpClient;
        private bool _inLogin;

        public SLContext(SLContextOptions options)
        {
            _options = options;

            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; }
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_options.Url)
            };

            _httpClient.DefaultRequestHeaders.Add("B1S-PageSize", "0");
            InitializeSets();
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

        internal async Task ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (_session == null || _session.IsExpired)
            {
                await LoginAsync(cancellationToken: cancellationToken);
            }

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw await GetExceptionAsync(response, cancellationToken);
                }
            }
            catch (SLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SLException("Error al realizar la petición", null, ex);
            }
        }

        internal async Task<T> ExecuteAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if ((_session == null || _session.IsExpired) && !_inLogin)
            {
                await LoginAsync(cancellationToken: cancellationToken);
            }

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw await GetExceptionAsync(response, cancellationToken);
                }

                var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return await response.Content.ReadFromJsonAsync<T>(serializerOptions, cancellationToken: cancellationToken);
            }
            catch (SLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SLException("Error al realizar la petición", null, ex);
            }
        }

        private async Task LoginAsync(CancellationToken cancellationToken = default)
        {
            _inLogin = true;

            if (_session == null || _session.IsExpired)
            {
                var body = new
                {
                    CompanyDB = _options.CompanyDB ?? throw new SLException("No se ha proporcionado una base de datos"),
                    UserName = _options.UserName ?? throw new SLException("No se proporcionó un usuario"),
                    Password = _options.Password ?? throw new SLException("No se proporcionó una contraseña"),
                    Language = _options.Language ?? 25
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "Login")
                {
                    Content = new StringContent(JsonSerializer.Serialize(body))
                };

                _session = await ExecuteAsync<SLSession>(request, cancellationToken);
                _session.LastLogin = DateTime.Now.AddSeconds(1);
                _httpClient.DefaultRequestHeaders.Remove("B1SESSION");
                _httpClient.DefaultRequestHeaders.Add("B1SESSION", _session.SessionId);
            }

            _inLogin = false;
        }

        private async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (_session != null && !_session.IsExpired)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "Logout");
                await ExecuteAsync(request, cancellationToken);
            }
        }

        private void InitializeSets()
        {
            var setsProps = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(SLSet<>));

            foreach (var set in setsProps)
            {
                var instance = Activator.CreateInstance(set.PropertyType, this, set.Name);
                set.SetValue(this, instance);
            }
        }

        public void Dispose()
        {
            LogoutAsync().Wait();
            _options = null;
            _session = null;
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
