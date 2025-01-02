using System.Threading.Tasks;
using VideoGameApi;

public class ErrorLoggingService : IErrorLoggingService
{
    private readonly ApplicationDbContext _context;

    public ErrorLoggingService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task LogValidationError(string method, string endpoint, object payload, string errorMessages)
    {
        var logEntry = new ErrorLog
        {
            Method = method,
            Endpoint = endpoint,
            Payload = payload.ToString(),  // Or use JsonConvert.SerializeObject(payload) if needed
            ErrorMessage = errorMessages,
            Timestamp = DateTime.UtcNow
        };

        Console.WriteLine("send");

        _context.ErrorLog.Add(logEntry);
        await _context.SaveChangesAsync();
    }
}
