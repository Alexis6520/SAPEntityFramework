using SAPEntityFramework.Extensions.Http;

namespace SAPEntityFramework
{
    /// <summary>
    /// Clase base para crear un contexto de SAP Service Layer. No admite multi-hilos.
    /// </summary>
    public abstract class SLContext : IDisposable
    {
        private SLContextOptions _options;
        private SLSession _session;
        private readonly HttpClient _httpClient;

        public SLContext(SLContextOptions options)
        {
            _options = options;

            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; }
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_options.Url,new UriCreationOptions { })
            };
        }

        /// <summary>
        /// Inicia sesión en Service Layer
        /// </summary>
        /// <param name="forceLogin">Forzar el login aunque no haya expirado la sesión</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns></returns>
        public async Task LoginAsync(bool forceLogin = false, CancellationToken cancellationToken = default)
        {
            if (_session != null)
            {
                if (!_session.IsExpired && !forceLogin)
                {
                    return;
                }
            }

            var body = new
            {
                _options.CompanyDB,
                _options.UserName,
                _options.Password,
                //_options.Language
            };

            _session = await _httpClient.PostJsonAsync<SLSession>("/b1s/v2/Login", body, cancellationToken);
            _session.LastLogin = DateTime.Now.AddSeconds(1);
        }

        public void Dispose()
        {
            _options = null;
            _session = null;
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
