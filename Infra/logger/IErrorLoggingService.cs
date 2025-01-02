public interface IErrorLoggingService
{
  Task LogValidationError(string method, string endpoint, object payload, string errorMessages);
}
