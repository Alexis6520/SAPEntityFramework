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

        public SLContext(SLContextOptions options)
        {
            _options = options;

            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; }
            };

            HttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_options.Url)
            };

            InitializeSets();
        }

        /// <summary>
        /// Cliente Http utilizado por el contexto
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        public string CompanyDB { get { return _options.CompanyDB; } set { _options.CompanyDB = value; _session = null; } }

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
                _options.Language
            };

            _session = await HttpClient.PostJsonAsync<SLSession>("Login", body, cancellationToken);
            _session.LastLogin = DateTime.Now.AddSeconds(1);
            HttpClient.DefaultRequestHeaders.Remove("B1SESSION");
            HttpClient.DefaultRequestHeaders.Add("B1SESSION", _session.SessionId);
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (_session == null || _session.IsExpired)
            {
                return;
            }

            await HttpClient.PostAsync("Logout", null, cancellationToken: cancellationToken);
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
            HttpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
