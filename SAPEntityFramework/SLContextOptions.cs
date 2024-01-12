namespace SAPEntityFramework
{
    /// <summary>
    /// Opciones de configuración de service layer
    /// </summary>
    public class SLContextOptions
    {
        /// <summary>
        /// Url de SAP Service Layer
        /// </summary>
        public string Url { get; set; }
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
