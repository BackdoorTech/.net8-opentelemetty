using VideoGameApi;
using System.Diagnostics;

public class DBLoggingService : IDBLoggingService
{
  private readonly ApplicationDbContext _context;

  public DBLoggingService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task LogError(string method, string endpoint, string statusCode, string payload, string errorMessages)
  {
    //Console.WriteLine(payload);
    var traceId = Activity.Current?.TraceId.ToString();
    var logEntry = new ErrorLog
    {
      Method = method,
      Endpoint = endpoint,
      Payload = payload,  // Or use JsonConvert.SerializeObject(payload) if needed
      ErrorMessage = errorMessages,
      Timestamp = DateTime.UtcNow,
      statusCode = statusCode,
      TraceId = traceId
    };

    //Console.WriteLine("send");

    _context.ErrorLog.Add(logEntry);
    await _context.SaveChangesAsync();
  }
}
