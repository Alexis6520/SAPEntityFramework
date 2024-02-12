using SAPSLFramework.Extensions.Http;

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

        internal HttpClient HttpClient { get { return _httpClient; } }

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
                CompanyDB = _options.CompanyDB ?? throw new SLException("No se ha proporcionado una base de datos"),
                UserName = _options.UserName ?? throw new SLException("No se proporcionó un usuario"),
                Password = _options.Password ?? throw new SLException("No se proporcionó una contraseña"),
                Language = _options.Language ?? 25
            };

            _session = await _httpClient.PostJsonAsync<SLSession>("Login", body, cancellationToken);
            _session.LastLogin = DateTime.Now.AddSeconds(1);
            _httpClient.DefaultRequestHeaders.Remove("B1SESSION");
            _httpClient.DefaultRequestHeaders.Add("B1SESSION", _session.SessionId);
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (_session == null || _session.IsExpired)
            {
                return;
            }

            await _httpClient.PostAsync("Logout", null, cancellationToken: cancellationToken);
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
            if (_session != null && !_session.IsExpired)
            {
                LogoutAsync().Wait();
            }

            _options = null;
            _session = null;
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
