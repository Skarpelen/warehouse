namespace Warehouse.BusinessLogic.Models
{
    public class SecureException : Exception
    {
        public string QueryParameters { get; set; }

        public string BodyParameters { get; set; }

        public SecureException(string message, string queryParameters = "", string bodyParameters = "")
            : base(message)
        {
            QueryParameters = queryParameters;
            BodyParameters = bodyParameters;
        }

        public SecureException(string message, Exception innerException, string queryParameters = "", string bodyParameters = "")
            : base(message, innerException)
        {
            QueryParameters = queryParameters;
            BodyParameters = bodyParameters;
        }
    }
}
