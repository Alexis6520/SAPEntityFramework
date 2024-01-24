using SAPSLFramework.Extensions.Http;

namespace SAPSLFramework
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

            HttpClient.DefaultRequestHeaders.Add("B1S-PageSize", "0");
            InitializeSets();
        }

        /// <summary>
        /// Cliente Http utilizado por el contexto
        /// </summary>
        public HttpClient HttpClient { get; private set; }

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
