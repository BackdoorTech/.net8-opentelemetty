using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using VideoGameApi;
using OpenTelemetry;
using OpenTelemetry.Resources;
using CleanArchitecture.Infrastructure.Interface;
using OpenTelemetry.Metrics;



using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(); // Add this line to enable controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR().AddJsonProtocol(options =>
  {
    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
  });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(
    builder.Configuration.GetConnectionString("Default"),
    sqlOptions => sqlOptions.EnableRetryOnFailure()

  )
);
builder.Services.AddScoped<IVideoGameRepository, VideoGameRepository>();
builder.Services.AddScoped<ErrorLoggingService>();
// Register the custom middleware in the pipeline

// builder.Services.AddScoped<ValidateModelAttribute>();

// Set up OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("BackEndDotnet"))
            .AddAspNetCoreInstrumentation() // Trace ASP.NET requests
            //.AddHttpClientInstrumentation() // Trace HTTP client calls
            .AddSqlClientInstrumentation(options =>
            {
              options.SetDbStatementForText = true; // Capture full SQL statements
            })
            .AddSource("MyApplicationSource") // Your custom sources
            .SetSampler(new AlwaysOnSampler()) // Sampler - always capture for testing
            .AddOtlpExporter(options =>
            {
              //options.Endpoint = new Uri("https://a0138f1d3cfe46c6b660a300bc5100d8.apm.us-central1.gcp.cloud.es.io:443");
              //options.Endpoint = new Uri("http://localhost:4318/v1/traces");
              // options.Headers = "Authorization=Bearer q7aNwoXyMrFXzI8bW8";
            })
    );
// Add OpenTelemetry metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(options =>
    {
        // Add application and runtime metrics
        options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("BackEndDotnet"));
        options.AddAspNetCoreInstrumentation(); // ASP.NET Core metrics
        options.AddSqlClientInstrumentation();

        // Optional: Add custom metrics
        options.AddMeter("MyApp.Metrics");

        // Configure the OTLP exporter to send data to the OpenTelemetry Collector
        options.AddOtlpExporter();
    });



// Set up Serilog as the logging framework
builder.Host.UseSerilog((context, loggerConfig) =>
{

  //loggerConfig.MinimumLevel.Warning();
  loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter())
  .WriteTo.OpenTelemetry(options =>
  {
    //options.Endpoint = "http://<collector-endpoint>:55681/v1/logs";
    //options.Endpoint = new Uri("http://<collector-endpoint>:55681/v1/logs");  // Replace with your OpenTelemetry Collector endpoint
  });
});



//builder.Services.AddSingleton(provider => TracerProvider.Default.GetTracer("CustomSpan"));
// Add CORS policy
builder.Services.AddCors();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(x => x
  .AllowAnyMethod()
  .AllowAnyHeader()
  .SetIsOriginAllowed(origin => true) // allow any origin
  //.WithOrigins("https://localhost:44351")); // Allow only this origin can also have multiple origins separated with comma
  .AllowCredentials()); // allow credentials

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapScalarApiReference();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Add this line for controller routing

// Map controllers
app.UseAuthorization();
app.MapControllers(); // This line connects controller routes

// Retain your minimal API example
var summaries = new[]
{
  "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
  var forecast =  Enumerable.Range(1, 5).Select(index =>
    new WeatherForecast
    (
        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
    ))
    .ToArray();
  return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("MyApplicationSource")
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("MyApplicationService"))
    .AddOtlpExporter() // Export tracing data to OTLP (can be adjusted)
    .Build();

var tracer = tracerProvider.GetTracer("MyApplicationSource");

app.MapGet("/", () =>
{
  var span = tracer.StartSpan("CustomOperation");

  // Add tags, events, or logic for the span
  span.SetAttribute("custom.attribute", "example-value");
  span.AddEvent("Custom event triggered");


  span.End();

  return "Hello from OpenTelemetry with manual span!";
});


// app.UseEndpoints(endpoints =>
// {
//   var unused = endpoints.MapHub<ChatHub>("/chat");
// });

app.MapHub<ChatHub>("/chathub");
// Register SignalR
//app.MapHub<ChatHub>("/chat");

// app.MapHealthChecks("/health", new HealthCheckOptions {
//   ResponseWriter = WriteJsonResponse
// });


// static Task WriteJsonResponse(HttpContext context, HealthReport result)
// {
//     context.Response.ContentType = "application/json";
//     var json = JsonSerializer.Serialize(new
//     {
//         status = result.Status.ToString(),
//         results = result.Entries.Select(entry => new
//         {
//             key = entry.Key,
//             status = entry.Value.Status.ToString(),
//             description = entry.Value.Description,
//             data = entry.Value.Data
//         })
//     });

//     return context.Response.WriteAsync(json);
// }

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
