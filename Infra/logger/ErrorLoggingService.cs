using VideoGameApi;

public class ErrorLoggingService : IErrorLoggingService
{
  private readonly ApplicationDbContext _context;

  public ErrorLoggingService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task LogValidationError(string method, string endpoint, string statusCode, string payload, string errorMessages)
  {
    //Console.WriteLine(payload);
    var logEntry = new ErrorLog
    {
      Method = method,
      Endpoint = endpoint,
      Payload = payload,  // Or use JsonConvert.SerializeObject(payload) if needed
      ErrorMessage = errorMessages,
      Timestamp = DateTime.UtcNow,
      statusCode = statusCode
    };

    //Console.WriteLine("send");

    _context.ErrorLog.Add(logEntry);
    await _context.SaveChangesAsync();
  }
}
