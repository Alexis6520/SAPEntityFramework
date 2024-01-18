namespace SAPSLFramework
{
    /// <summary>
    /// Excepción de SAP Service Layer
    /// </summary>
    public class SLException : Exception
    {
        /// <summary>
        /// Código de error
        /// </summary>
        public string Code { get; set; }

        public SLException(string message, string code = null, Exception innerException = null) : base(message, innerException)
        {
            Code = code;
        }
    }

    internal class SLErrorResponse
    {
        public SLError Error { get; set; }
    }

    internal class SLError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
