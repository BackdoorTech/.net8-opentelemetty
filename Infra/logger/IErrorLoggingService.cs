public interface IErrorLoggingService
{
  Task LogValidationError(string method, string endpoint, string statusCode, string payload, string errorMessages);
}
