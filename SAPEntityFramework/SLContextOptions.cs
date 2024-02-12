namespace SAPSLFramework
{
    /// <summary>
    /// Opciones de configuración de service layer
    /// </summary>
    public class SLContextOptions
    {
        private string _url;

        /// <summary>
        /// Url de SAP Service Layer
        /// </summary>
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (value.Last() != '/')
                {
                    value += '/';
                }

                _url = value;
            }
        }

        /// <summary>
        /// Base de datos de compañía
        /// </summary>
        public string CompanyDB { get; set; }

        /// <summary>
        /// Usuario para autenticar
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Contraseña para autenticar
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Código de lenguaje
        /// </summary>
        public int? Language { get; set; }
    }
}
