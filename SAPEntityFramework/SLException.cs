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
        public int? Code { get; set; }

        public SLException(string message, int? code = null, Exception innerException = null) : base(message, innerException)
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
        public int Code { get; set; }
        public SlErrorMessage Message { get; set; }
    }

    internal class SlErrorMessage
    {
        public string Value { get; set; }
    }
}
