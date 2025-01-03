using System.Diagnostics;
using Serilog;

public class RequestLoggingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private const int MaxRequestBodySize = 500 * 1024; // 500KB

  public RequestLoggingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
  {
    _next = next;
    _serviceScopeFactory = serviceScopeFactory;
  }

  public async Task InvokeAsync(HttpContext httpContext)
  {
    // Enable buffering so the body can be read multiple times
    httpContext.Request.EnableBuffering();

    // Save the original body stream
    var originalBodyStream = httpContext.Response.Body;

    // Use a memory stream to intercept the response body
    using var responseStream = new MemoryStream();
    httpContext.Response.Body = responseStream;



    try
    {
      // Process the incoming request
      await _next(httpContext);

      // Check the request body size
      if (httpContext.Request.ContentLength.HasValue && httpContext.Request.ContentLength.Value <= MaxRequestBodySize)
      {
        // Log the request body if it meets the size condition
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        string requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        // Log the request body to a logger or output
        // Console.WriteLine($"Request Payload: {requestBody}");

        // Reset the request body so it can be used later by other middleware
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        // Log the response body after the request is processed
        responseStream.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(responseStream).ReadToEndAsync();
        // Log the response body to a logger or output
        // Console.WriteLine($"Response Payload: {responseBody}");

        // Optionally, you can log additional info like method, status code, etc.
        string method = httpContext.Request.Method;
        string endpoint = httpContext.Request.Path;
        string statusCode = httpContext.Response.StatusCode.ToString();

        // Only log if the status code is >= 400
        if (httpContext.Response.StatusCode >= 400)
        {
          var activity = Activity.Current;
          activity.SetStatus(ActivityStatusCode.Error, responseBody);
          using (var scope = _serviceScopeFactory.CreateScope())
          {
            var loggerDb = scope.ServiceProvider.GetRequiredService<DBLoggingService>();
            await loggerDb.LogError(method, endpoint, statusCode, requestBody, responseBody);
          }
        }
      }
    }
    catch (Exception ex)
    {
      // Log the error information
      string method = httpContext.Request.Method;
      string endpoint = httpContext.Request.Path;
      string requestBody = string.Empty;

      if (httpContext.Request.ContentLength.HasValue && httpContext.Request.ContentLength.Value <= MaxRequestBodySize)
      {
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
      }

      // Ensure TraceId is set in case of an exception
      // Set response status code to 400 (Bad Request) or appropriate value
      httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

      var activity = Activity.Current;
      activity.SetStatus(ActivityStatusCode.Error, ex.ToString());

      // send to collector
      Log.Error(ex, "Error processing request: {Method} {Endpoint}, Response Status: {StatusCode} " +"Payload: {@Payload}",
      method, endpoint, httpContext.Response.StatusCode, new { RequestPayload = requestBody });

      // send to database
      using (var scope = _serviceScopeFactory.CreateScope())
      {
        var loggerDb = scope.ServiceProvider.GetRequiredService<DBLoggingService>();
        await loggerDb.LogError(method, endpoint, httpContext.Response.StatusCode.ToString(), requestBody, ex.ToString());
      }
    }
    finally
    {
      var traceId = Activity.Current?.TraceId.ToString();
      if (!string.IsNullOrEmpty(traceId))
      {
        Console.WriteLine("TraceId: " + traceId);
        httpContext.Response.Headers.Add("X-TraceId", traceId);
      }

      // Reset the response stream position before copying back to the original stream
      responseStream.Seek(0, SeekOrigin.Begin);
      await responseStream.CopyToAsync(originalBodyStream);
    }
  }
}
