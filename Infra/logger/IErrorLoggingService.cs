public interface IDBLoggingService
{
  Task LogError(string method, string endpoint, string statusCode, string payload, string errorMessages);
}
